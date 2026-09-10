using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Structures;

using UnityEditor;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Editor.MeshBaking
{
    /// <summary>
    /// Collapses every mesh under a prefab into a single mesh asset and writes a "baked" copy of the
    /// prefab that renders it from one MeshRenderer.
    ///
    /// The point is entity count, not draw calls. Structure prefabs here are thin wrappers around deep
    /// third-party prefab nests (a house resolves to ~50-150 leaf MeshRenderers), and Entities Graphics
    /// bakes one entity per MeshRenderer. Every placed building therefore instantiates that many child
    /// entities, each carrying LocalTransform/Parent/LocalToWorld that LocalToWorldSystem recomputes
    /// every frame - for objects that never move once placed. Baking turns that into one entity.
    /// Draw calls barely change: BatchRendererGroup already instances these, and the art pack shares
    /// atlas materials. Triangle count does not change at all - this is not a decimation tool.
    ///
    /// Geometry is grouped by material into one submesh each, so the result is pixel-identical to the
    /// source. Nothing is atlased and no UVs are touched.
    /// </summary>
    public static class PrefabMeshBaker
    {
        public const string OutputFolder = "Assets/Resources/Prefabs/Worlds/Structures/Baked";
        public const string BakedSuffix = "_Baked";

        private const string MeshSuffix = "_BakedMesh";

        // Above this a mesh needs 32-bit indices, and the format has to be set before CombineMeshes runs
        private const int UInt16IndexLimit = 65535;

        public readonly struct BakeReport
        {
            public string PrefabPath { get; }
            public string MeshPath { get; }
            public int SourceRenderers { get; }
            public int SubMeshes { get; }
            public int Vertices { get; }
            public int Triangles { get; }

            public BakeReport(string prefabPath, string meshPath, int sourceRenderers, int subMeshes,
                int vertices, int triangles)
            {
                PrefabPath = prefabPath;
                MeshPath = meshPath;
                SourceRenderers = sourceRenderers;
                SubMeshes = subMeshes;
                Vertices = vertices;
                Triangles = triangles;
            }

            public override string ToString()
            {
                return $"{SourceRenderers} renderers -> 1 ({SubMeshes} submesh(es), " +
                       $"{Vertices:N0} verts, {Triangles:N0} tris) | {PrefabPath}";
            }
        }

        /// <summary>
        /// Bakes <paramref name="prefabAsset"/> into <see cref="OutputFolder"/>. The source prefab is
        /// never modified. Re-baking overwrites in place, keeping both asset GUIDs so anything already
        /// referencing the baked prefab or its mesh survives.
        /// </summary>
        public static bool TryBake(GameObject prefabAsset, out BakeReport report, out string error)
        {
            report = default;
            error = null;

            var sourcePath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(sourcePath))
            {
                error = $"'{prefabAsset.name}' is not a prefab asset";
                return false;
            }

            if (prefabAsset.name.EndsWith(BakedSuffix))
            {
                error = $"'{prefabAsset.name}' is already a baked prefab - bake the source instead";
                return false;
            }

            // LoadPrefabContents resolves the whole nested-PrefabInstance chain into real GameObjects,
            // which is what makes the two-layer asset-pack nesting a non-issue here
            var root = PrefabUtility.LoadPrefabContents(sourcePath);

            try
            {
                return BakeLoadedContents(root, prefabAsset.name, sourcePath, out report, out error);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool BakeLoadedContents(GameObject root, string sourceName, string sourcePath,
            out BakeReport report, out string error)
        {
            report = default;

            var sources = CollectSourceFilters(root);
            if (sources.Count == 0)
            {
                error = $"'{sourceName}' has no active mesh renderers to bake";
                return false;
            }

            WarnAboutUnreadableMeshes(sourceName, sources);

            var temporaryMeshes = new List<Mesh>();
            Mesh combined;
            List<Material> materials;

            try
            {
                combined = Combine(root.transform, sources, temporaryMeshes, out materials);
            }
            finally
            {
                foreach (var temporary in temporaryMeshes)
                {
                    Object.DestroyImmediate(temporary);
                }
            }

            if (combined == null)
            {
                error = $"'{sourceName}' produced no geometry - every submesh had a null material or mesh";
                return false;
            }

            if (!EnsureOutputFolder(out error))
            {
                Object.DestroyImmediate(combined);
                return false;
            }

            var meshPath = $"{OutputFolder}/{sourceName}{MeshSuffix}.asset";
            var prefabPath = $"{OutputFolder}/{sourceName}{BakedSuffix}.prefab";

            combined.name = $"{sourceName}{MeshSuffix}";
            var meshAsset = WriteMeshAsset(combined, meshPath);

            if (!TryWriteBakedPrefab(root, sources[0], sourceName, meshAsset, materials, prefabPath, out error))
            {
                return false;
            }

            report = new BakeReport(prefabPath, meshPath, sources.Count, meshAsset.subMeshCount,
                meshAsset.vertexCount, meshAsset.triangles.Length / 3);

            Debug.Log($"[PrefabMeshBaker] {report}\nSource: {sourcePath}",
                AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));

            error = null;
            return true;
        }

        #region Collection

        private static List<MeshFilter> CollectSourceFilters(GameObject root)
        {
            var excluded = CollectNonLod0Renderers(root);
            var sources = new List<MeshFilter>();

            // Deliberately fetching inactive too and resolving activeness by walking up to the prefab
            // root: activeInHierarchy is unreliable for objects living in a preview scene
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || !renderer.enabled || excluded.Contains(renderer))
                {
                    continue;
                }

                if (!IsActiveUnder(filter.transform, root.transform))
                {
                    continue;
                }

                sources.Add(filter);
            }

            return sources;
        }

        /// <summary>
        /// Every renderer that only exists as a lower LOD level. Combining them all would stack the
        /// LOD meshes on top of each other - StoneRoadPrefab has three LODs of the same tile.
        /// </summary>
        private static HashSet<Renderer> CollectNonLod0Renderers(GameObject root)
        {
            var excluded = new HashSet<Renderer>();

            foreach (var group in root.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = group.GetLODs();
                if (lods.Length == 0)
                {
                    continue;
                }

                for (var level = 1; level < lods.Length; level++)
                {
                    foreach (var renderer in lods[level].renderers)
                    {
                        if (renderer != null)
                        {
                            excluded.Add(renderer);
                        }
                    }
                }

                // A renderer may be listed in several levels at once - being in LOD0 always wins
                foreach (var renderer in lods[0].renderers)
                {
                    if (renderer != null)
                    {
                        excluded.Remove(renderer);
                    }
                }
            }

            return excluded;
        }

        private static bool IsActiveUnder(Transform transform, Transform root)
        {
            for (var current = transform; current != null; current = current.parent)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == root)
                {
                    break;
                }
            }

            return true;
        }

        private static void WarnAboutUnreadableMeshes(string sourceName, List<MeshFilter> sources)
        {
            var unreadable = sources
                             .Select(filter => filter.sharedMesh)
                             .Where(mesh => !mesh.isReadable)
                             .Select(mesh => mesh.name)
                             .Distinct()
                             .ToList();

            if (unreadable.Count == 0)
            {
                return;
            }

            // Edit-time reads normally still succeed; this only matters if the combine throws
            Debug.LogWarning($"[PrefabMeshBaker] '{sourceName}' uses mesh(es) without Read/Write enabled: " +
                             $"{string.Join(", ", unreadable)}. Enable it on the model importer if baking fails.");
        }

        #endregion

        #region Combining

        private static Mesh Combine(Transform root, List<MeshFilter> sources, List<Mesh> temporaryMeshes,
            out List<Material> materials)
        {
            // Root-local, not world: Structure.OnValidate force-writes the root's localScale from its
            // 'scale' field, so a world-space bake would end up with that scale applied twice
            var toRootLocal = root.worldToLocalMatrix;

            materials = new List<Material>();
            var grouped = new List<List<CombineInstance>>();
            var totalVertices = 0;

            foreach (var filter in sources)
            {
                var mesh = filter.sharedMesh;
                var renderer = filter.GetComponent<MeshRenderer>();
                var rendererMaterials = renderer.sharedMaterials;
                var matrix = toRootLocal * filter.transform.localToWorldMatrix;

                // CombineMeshes transforms positions and normals but never re-winds triangles, so a
                // mirrored transform would render inside out unless the flip is baked into a copy
                var mirrored = matrix.determinant < 0f;

                for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                {
                    // Unity falls back to the last material when a mesh has more submeshes than slots
                    var material = rendererMaterials.Length == 0
                        ? null
                        : rendererMaterials[Mathf.Min(subMesh, rendererMaterials.Length - 1)];

                    var instance = new CombineInstance { transform = matrix };

                    if (mirrored)
                    {
                        var flipped = CreateRewoundSubMesh(mesh, subMesh);
                        temporaryMeshes.Add(flipped);
                        instance.mesh = flipped;
                        instance.subMeshIndex = 0;
                    }
                    else
                    {
                        instance.mesh = mesh;
                        instance.subMeshIndex = subMesh;
                    }

                    GroupFor(materials, grouped, material).Add(instance);
                    totalVertices += mesh.vertexCount;
                }
            }

            if (grouped.Count == 0)
            {
                return null;
            }

            return CombineGroups(grouped, totalVertices, temporaryMeshes);
        }

        private static List<CombineInstance> GroupFor(List<Material> materials,
            List<List<CombineInstance>> grouped, Material material)
        {
            // Linear scan rather than a Dictionary: material counts are tiny, and a null material has
            // to stay groupable (Dictionary rejects a null key)
            var index = materials.IndexOf(material);

            if (index >= 0)
            {
                return grouped[index];
            }

            materials.Add(material);
            grouped.Add(new List<CombineInstance>());

            return grouped[^1];
        }

        private static Mesh CombineGroups(List<List<CombineInstance>> grouped, int totalVertices,
            List<Mesh> temporaryMeshes)
        {
            var needsWideIndices = totalVertices > UInt16IndexLimit;

            // Stage one: every material's geometry merged down to a single-submesh mesh
            var stageOne = new CombineInstance[grouped.Count];

            for (var i = 0; i < grouped.Count; i++)
            {
                var merged = new Mesh { name = $"BakeGroup{i}" };

                if (needsWideIndices)
                {
                    merged.indexFormat = IndexFormat.UInt32;
                }

                merged.CombineMeshes(grouped[i].ToArray(), true, true);
                temporaryMeshes.Add(merged);

                stageOne[i] = new CombineInstance { mesh = merged, subMeshIndex = 0 };
            }

            // Stage two: keep the groups apart so submesh order lines up with the material order
            var combined = new Mesh();

            if (needsWideIndices)
            {
                combined.indexFormat = IndexFormat.UInt32;
            }

            combined.CombineMeshes(stageOne, false, false);
            combined.RecalculateBounds();

            // Static geometry, so a one-off vertex-cache reorder is worth the bake time
            combined.Optimize();

            return combined;
        }

        /// <summary>
        /// A single submesh copied out with its winding reversed. Reversing the whole index array flips
        /// every triangle's orientation as a side effect of reversing their order.
        /// </summary>
        private static Mesh CreateRewoundSubMesh(Mesh source, int subMeshIndex)
        {
            var triangles = source.GetTriangles(subMeshIndex);
            System.Array.Reverse(triangles);

            var copy = new Mesh
            {
                name = $"{source.name}_rewound{subMeshIndex}",
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                normals = source.normals,
                tangents = source.tangents,
                uv = source.uv,
                uv2 = source.uv2,
                colors = source.colors
            };

            copy.SetTriangles(triangles, 0);

            return copy;
        }

        #endregion

        #region Writing

        private static bool EnsureOutputFolder(out string error)
        {
            error = null;

            if (AssetDatabase.IsValidFolder(OutputFolder))
            {
                return true;
            }

            var segments = OutputFolder.Split('/');
            var current = segments[0];

            for (var i = 1; i < segments.Length; i++)
            {
                var next = $"{current}/{segments[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }

            if (AssetDatabase.IsValidFolder(OutputFolder))
            {
                return true;
            }

            error = $"Could not create the output folder '{OutputFolder}'";
            return false;
        }

        /// <summary>
        /// Writes over the existing mesh asset when there is one, so its GUID survives a re-bake and the
        /// baked prefab keeps pointing at it.
        /// </summary>
        private static Mesh WriteMeshAsset(Mesh combined, string meshPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

            if (existing == null)
            {
                AssetDatabase.CreateAsset(combined, meshPath);
                return combined;
            }

            var name = combined.name;
            EditorUtility.CopySerialized(combined, existing);
            existing.name = name;

            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(combined);

            return existing;
        }

        private static bool TryWriteBakedPrefab(GameObject root, MeshFilter template, string sourceName,
            Mesh mesh, List<Material> materials, string prefabPath, out string error)
        {
            error = null;
            var baked = new GameObject($"{sourceName}{BakedSuffix}");

            try
            {
                baked.transform.localScale = root.transform.localScale;

                // Must carry the concrete Structure subclass across: BaseStructureData.structurePrefab is
                // typed Structure, not GameObject, so a baked prefab without one can't be assigned in the
                // StructureDictionary. CopyComponent preserves the subclass and its serialised fields.
                var structure = root.GetComponent<Structure>();

                if (structure != null)
                {
                    ComponentUtility.CopyComponent(structure);
                    ComponentUtility.PasteComponentAsNew(baked);
                }
                else
                {
                    Debug.LogWarning($"[PrefabMeshBaker] '{sourceName}' has no Structure component on its " +
                                     "root, so the baked copy can't be assigned to a BaseStructureData.");
                }

                baked.AddComponent<MeshFilter>().sharedMesh = mesh;

                var renderer = baked.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials.ToArray();
                CopyRendererSettings(template.GetComponent<MeshRenderer>(), renderer);

                PrefabUtility.SaveAsPrefabAsset(baked, prefabPath, out var success);

                if (!success)
                {
                    error = $"Failed to save the baked prefab at '{prefabPath}'";
                    return false;
                }

                return true;
            }
            finally
            {
                Object.DestroyImmediate(baked);
            }
        }

        private static void CopyRendererSettings(MeshRenderer from, MeshRenderer to)
        {
            to.shadowCastingMode = from.shadowCastingMode;
            to.receiveShadows = from.receiveShadows;
            to.lightProbeUsage = from.lightProbeUsage;
            to.reflectionProbeUsage = from.reflectionProbeUsage;
            to.motionVectorGenerationMode = from.motionVectorGenerationMode;
            to.renderingLayerMask = from.renderingLayerMask;
            to.allowOcclusionWhenDynamic = from.allowOcclusionWhenDynamic;
        }

        #endregion
    }
}
