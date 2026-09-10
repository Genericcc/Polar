using System;
using System.IO;

using _Scripts.Editor.Kanban.Model;

using UnityEngine;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// Disk side of the board. The file lives in ProjectSettings rather than Assets on purpose:
    /// it is still tracked by git (so the board is shared and diffable) but writing it never makes
    /// Unity reimport an asset, which is what would otherwise happen on every keystroke.
    /// </summary>
    public static class KanbanStore
    {
        public const string BoardPath = "ProjectSettings/Kanban/board.json";

        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        /// <summary>Absolute path, for File IO and for "reveal in explorer".</summary>
        public static string AbsoluteBoardPath =>
            Path.GetFullPath(Path.Combine(ProjectRoot, BoardPath));

        private static string ProjectRoot =>
            // Application.dataPath is "<project>/Assets".
            Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();

        public static bool Exists()
        {
            return File.Exists(AbsoluteBoardPath);
        }

        public static DateTime LastWriteUtc()
        {
            var path = AbsoluteBoardPath;
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }

        /// <summary>
        /// Reads the board, falling back to a fresh default board when the file is missing or
        /// unreadable. Never throws - a broken board file must not make the window unopenable.
        /// </summary>
        public static KanbanBoard Load()
        {
            var path = AbsoluteBoardPath;

            if (!File.Exists(path))
            {
                return KanbanBoard.CreateDefault();
            }

            try
            {
                var json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return KanbanBoard.CreateDefault();
                }

                var board = JsonUtility.FromJson<KanbanBoard>(json);

                if (board == null)
                {
                    Debug.LogWarning($"[Kanban] '{BoardPath}' did not parse as a board - starting from a default board. " +
                                     "The unreadable file is left untouched on disk.");
                    return KanbanBoard.CreateDefault();
                }

                board.EnsureValid();
                return board;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Kanban] Failed to read '{BoardPath}': {exception.Message}");
                return KanbanBoard.CreateDefault();
            }
        }

        /// <summary>
        /// Writes the board atomically - full write to a temp file, then a replace - so a crash or a
        /// full disk mid-write can never leave a truncated board behind.
        /// </summary>
        public static bool Save(KanbanBoard board)
        {
            if (board == null) return false;

            var path = AbsoluteBoardPath;

            try
            {
                var directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonUtility.ToJson(board, true);
                var tempPath = path + TempSuffix;

                File.WriteAllText(tempPath, json);

                if (File.Exists(path))
                {
                    // File.Replace keeps the write atomic on NTFS. The backup copy it insists on is
                    // deleted straight after; it only exists to survive a failure inside Replace itself.
                    var backupPath = path + BackupSuffix;
                    File.Replace(tempPath, path, backupPath, true);

                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Kanban] Failed to write '{BoardPath}': {exception.Message}");
                return false;
            }
        }
    }
}
