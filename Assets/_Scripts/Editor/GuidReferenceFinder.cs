using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace _Scripts.Editor
{
    /// <summary>
    /// Reverse reference lookup: given an asset (or a raw GUID), scan the project's text-serialized
    /// files for that GUID and list everything pointing at it. Unity's own dependency API only walks
    /// forwards, so this fills the gap - useful before renaming, moving or deleting an asset, and for
    /// answering "what is this GUID?" when reading a .unity or .prefab diff by hand.
    /// </summary>
    public class GuidReferenceFinder : EditorWindow
    {
        // Text-serialized asset formats that can hold a GUID reference. Binary formats (textures,
        // meshes, audio) never do, so scanning them would only cost time.
        private static readonly string[] SearchableExtensions =
        {
            ".unity", ".prefab", ".asset", ".mat", ".controller", ".overrideController",
            ".anim", ".playable", ".mixer", ".physicMaterial", ".physicsMaterial2D",
            ".spriteatlas", ".guiskin", ".fontsettings", ".preset", ".shadervariants",
            ".lighting", ".renderTexture", ".signal", ".asmdef", ".inputactions"
        };

        // ProjectSettings holds GUID references too - EditorBuildSettings names scenes by GUID
        private static readonly string[] SearchRoots = { "Assets", "ProjectSettings" };

        private UnityEngine.Object _target;
        private string _guid = string.Empty;
        private bool _includeMetaFiles;

        private readonly List<string> _results = new();
        private string _status = string.Empty;
        private Vector2 _scroll;

        [MenuItem(PolarAssetMenu.Root + "Find References by GUID")]
        private static void Open()
        {
            GetWindow<GuidReferenceFinder>("GUID References");
        }

        [MenuItem("Assets/" + PolarAssetMenu.Root + "Find References in Project", false, PolarAssetMenu.Order)]
        private static void FindForSelection()
        {
            var window = GetWindow<GuidReferenceFinder>("GUID References");
            window.SetTarget(Selection.activeObject);
            window.Search();
        }

        [MenuItem("Assets/" + PolarAssetMenu.Root + "Find References in Project", true)]
        private static bool FindForSelectionValidate()
        {
            return Selection.activeObject != null && AssetDatabase.Contains(Selection.activeObject);
        }

        private void SetTarget(UnityEngine.Object asset)
        {
            _target = asset;
            _guid = ToGuid(asset);
        }

        private static string ToGuid(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }

        private void OnGUI()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorGUILayout.HelpBox(
                    "Asset Serialization is not set to Force Text. Binary assets can't be scanned, " +
                    "so results will be incomplete. Edit > Project Settings > Editor > Asset Serialization.",
                    MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            _target = EditorGUILayout.ObjectField("Asset", _target, typeof(UnityEngine.Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                _guid = ToGuid(_target);
            }

            EditorGUI.BeginChangeCheck();
            _guid = EditorGUILayout.TextField("GUID", _guid);
            if (EditorGUI.EndChangeCheck())
            {
                // Paste a GUID straight from a .unity/.prefab diff and it resolves back to the asset
                var path = AssetDatabase.GUIDToAssetPath(_guid);
                _target = string.IsNullOrEmpty(path)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }

            _includeMetaFiles = EditorGUILayout.Toggle(
                new GUIContent(".meta files", "Importer settings can reference other assets by GUID"),
                _includeMetaFiles);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_guid)))
            {
                if (GUILayout.Button("Find References"))
                {
                    Search();
                }
            }

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.LabelField(_status, EditorStyles.boldLabel);
            }

            DrawResults();
        }

        private void DrawResults()
        {
            if (_results.Count == 0)
            {
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var path in _results)
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

                using (new EditorGUILayout.HorizontalScope())
                {
                    // ProjectSettings files aren't in the AssetDatabase, so there's nothing to ping
                    using (new EditorGUI.DisabledScope(asset == null))
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(45)))
                        {
                            EditorGUIUtility.PingObject(asset);
                            Selection.activeObject = asset;
                        }
                    }

                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Search()
        {
            _results.Clear();
            _status = string.Empty;

            var guid = _guid?.Trim();
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            // The asset's own .meta declares this GUID - that's the definition, not a reference
            var ownMetaPath = AssetDatabase.GUIDToAssetPath(guid);
            ownMetaPath = string.IsNullOrEmpty(ownMetaPath) ? null : ownMetaPath + ".meta";

            var files = CollectFiles();
            var cancelled = false;

            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var path = files[i];

                    if (i % 50 == 0 && EditorUtility.DisplayCancelableProgressBar(
                            "Finding references",
                            $"{i} / {files.Count} - {_results.Count} found",
                            (float)i / files.Count))
                    {
                        cancelled = true;
                        break;
                    }

                    if (path == ownMetaPath)
                    {
                        continue;
                    }

                    try
                    {
                        if (File.ReadAllText(path).IndexOf(guid, StringComparison.Ordinal) >= 0)
                        {
                            _results.Add(path);
                        }
                    }
                    catch (IOException)
                    {
                        // Locked or mid-write by the Editor - skip rather than abort the whole scan
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            _status = cancelled
                ? $"Cancelled - {_results.Count} reference(s) so far"
                : $"{_results.Count} reference(s) in {files.Count} scanned file(s)";

            Repaint();
        }

        private List<string> CollectFiles()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;

            return SearchRoots
                   .Select(root => Path.Combine(projectRoot, root))
                   .Where(Directory.Exists)
                   .SelectMany(root => Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                   .Where(IsSearchable)
                   .Select(absolute => ToProjectRelative(absolute, projectRoot))
                   .ToList();
        }

        private bool IsSearchable(string path)
        {
            if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                return _includeMetaFiles;
            }

            var extension = Path.GetExtension(path);
            return SearchableExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private static string ToProjectRelative(string absolutePath, string projectRoot)
        {
            // AssetDatabase only accepts forward slashes and project-relative paths
            var relative = absolutePath.Substring(projectRoot.Length).TrimStart('/', '\\');
            return relative.Replace('\\', '/');
        }
    }
}
