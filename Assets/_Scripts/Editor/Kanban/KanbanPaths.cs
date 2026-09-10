using System.IO;

using UnityEditor;

using UnityEngine;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// Locates the tool's own UXML/USS next to its scripts by asking the AssetDatabase where the
    /// anchoring script actually is, instead of hardcoding "Assets/_Scripts/Editor/Kanban/UI".
    /// The project already loses runtime DI silently when a Resources path is renamed - there is no
    /// reason to add the same failure mode on the editor side.
    /// </summary>
    public static class KanbanPaths
    {
        private const string UiFolderName = "UI";

        private static string _cachedUiFolder;

        /// <param name="anchor">Any ScriptableObject (EditorWindow included) whose script sits in the Kanban root folder.</param>
        public static string UiFolder(ScriptableObject anchor)
        {
            if (!string.IsNullOrEmpty(_cachedUiFolder)) return _cachedUiFolder;

            var script = MonoScript.FromScriptableObject(anchor);
            var scriptPath = script != null ? AssetDatabase.GetAssetPath(script) : null;

            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("[Kanban] Could not locate the Kanban scripts in the AssetDatabase; UI assets will not load.");
                return null;
            }

            var folder = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
            _cachedUiFolder = $"{folder}/{UiFolderName}";
            return _cachedUiFolder;
        }

        public static T LoadUiAsset<T>(ScriptableObject anchor, string fileName) where T : Object
        {
            var folder = UiFolder(anchor);

            if (string.IsNullOrEmpty(folder)) return null;

            var asset = AssetDatabase.LoadAssetAtPath<T>($"{folder}/{fileName}");

            if (asset == null)
            {
                Debug.LogError($"[Kanban] Missing UI asset '{fileName}' under '{folder}'.");
            }

            return asset;
        }
    }
}
