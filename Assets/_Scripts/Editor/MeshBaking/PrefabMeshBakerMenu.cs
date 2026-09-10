using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace _Scripts.Editor.MeshBaking
{
    /// <summary>
    /// Project window entry point for <see cref="PrefabMeshBaker"/>: select one or more prefabs,
    /// right-click, bake. Results land in <see cref="PrefabMeshBaker.OutputFolder"/> and the sources are
    /// left untouched, so re-baking after an art change is just running this again.
    /// </summary>
    public static class PrefabMeshBakerMenu
    {
        private const string MenuName = "Bake Combined Mesh";

        [MenuItem("Assets/" + PolarAssetMenu.Root + MenuName, false, PolarAssetMenu.Order)]
        private static void BakeSelection()
        {
            var prefabs = SelectedPrefabs();
            var reports = new List<PrefabMeshBaker.BakeReport>();
            var failures = new List<string>();

            try
            {
                for (var i = 0; i < prefabs.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Baking combined meshes",
                        prefabs[i].name, (float)i / prefabs.Count);

                    if (PrefabMeshBaker.TryBake(prefabs[i], out var report, out var error))
                    {
                        reports.Add(report);
                    }
                    else
                    {
                        failures.Add(error);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LogSummary(reports, failures);
        }

        [MenuItem("Assets/" + PolarAssetMenu.Root + MenuName, true)]
        private static bool BakeSelectionValidate()
        {
            return SelectedPrefabs().Count > 0;
        }

        private static List<GameObject> SelectedPrefabs()
        {
            return Selection.GetFiltered<GameObject>(SelectionMode.Assets)
                            .Where(asset => PrefabUtility.IsPartOfPrefabAsset(asset))
                            .Where(asset => !asset.name.EndsWith(PrefabMeshBaker.BakedSuffix))
                            .ToList();
        }

        private static void LogSummary(List<PrefabMeshBaker.BakeReport> reports, List<string> failures)
        {
            foreach (var failure in failures)
            {
                Debug.LogWarning($"[PrefabMeshBaker] {failure}");
            }

            if (reports.Count == 0)
            {
                return;
            }

            // The saving is in renderer count, so make that the headline - one entity per MeshRenderer
            // is what Entities Graphics bakes, and that is what the placed structures pay for
            var renderersBefore = reports.Sum(report => report.SourceRenderers);

            Debug.Log($"[PrefabMeshBaker] Baked {reports.Count} prefab(s): {renderersBefore} mesh " +
                      $"renderer(s) collapsed to {reports.Count}, i.e. ~{renderersBefore - reports.Count} " +
                      "fewer entities per set of placed structures.\n" +
                      string.Join("\n", reports.Select(report => report.ToString())));
        }
    }
}
