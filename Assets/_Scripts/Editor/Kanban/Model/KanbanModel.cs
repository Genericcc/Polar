using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace _Scripts.Editor.Kanban.Model
{
    /// <summary>
    /// Root of the board. Plain [Serializable] classes so both JsonUtility (disk) and Unity's own
    /// serializer (Undo, through KanbanBoardHolder) can walk the same objects.
    /// </summary>
    [Serializable]
    public class KanbanBoard
    {
        // Bump whenever the on-disk shape changes in a way a loader has to branch on.
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string Title = "Polar Board";

        // Monotonic id source. Ids are never reused, so a card keeps its identity across moves -
        // which is what the expand-state set (and, later, sorting) keys off.
        public int NextId = 1;

        public List<KanbanColumn> Columns = new();

        public static KanbanBoard CreateDefault()
        {
            var board = new KanbanBoard();
            board.AddColumn("Backlog");
            board.AddColumn("In Progress");
            board.AddColumn("Done");
            return board;
        }

        public string AllocateId()
        {
            return (NextId++).ToString(CultureInfo.InvariantCulture);
        }

        public KanbanColumn AddColumn(string title)
        {
            var column = new KanbanColumn
            {
                Id = AllocateId(),
                Title = title,
                ColorHex = KanbanPalette.NextColumnHex(Columns.Count)
            };

            Columns.Add(column);
            return column;
        }

        public int IndexOf(KanbanColumn column)
        {
            return Columns.IndexOf(column);
        }

        public void MoveColumn(int from, int to)
        {
            if (from < 0 || from >= Columns.Count) return;
            if (to < 0 || to >= Columns.Count) return;
            if (from == to) return;

            var column = Columns[from];
            Columns.RemoveAt(from);
            Columns.Insert(to, column);
        }

        /// <summary>
        /// Repairs anything a hand-edited (or older) JSON file might be missing. Load always runs this,
        /// so the rest of the tool can assume non-null lists and unique, non-empty ids.
        /// </summary>
        public void EnsureValid()
        {
            Columns ??= new List<KanbanColumn>();
            Title ??= "Polar Board";

            // First pass: normalise nulls and find the highest id already in use, so freshly handed-out
            // ids cannot collide with one further down the file.
            var highestSeen = 0;

            foreach (var column in Columns)
            {
                column.Tasks ??= new List<KanbanTask>();
                column.Title ??= string.Empty;
                column.Glyph ??= string.Empty;

                TrackHighestId(column.Id, ref highestSeen);

                foreach (var task in column.Tasks)
                {
                    task.Title ??= string.Empty;
                    task.Glyph ??= string.Empty;
                    task.Description ??= string.Empty;
                    task.Fields ??= new List<KanbanField>();

                    TrackHighestId(task.Id, ref highestSeen);
                }
            }

            if (NextId <= highestSeen) NextId = highestSeen + 1;
            if (NextId < 1) NextId = 1;

            // Second pass: anything missing an id, or repeating one already claimed earlier in the file,
            // gets a fresh one. HashSet.Add returning false is the duplicate check.
            var claimed = new HashSet<string>();

            foreach (var column in Columns)
            {
                if (string.IsNullOrEmpty(column.Id) || !claimed.Add(column.Id)) column.Id = TakeId(claimed);

                foreach (var task in column.Tasks)
                {
                    if (string.IsNullOrEmpty(task.Id) || !claimed.Add(task.Id)) task.Id = TakeId(claimed);
                }
            }

            SchemaVersion = CurrentSchemaVersion;
        }

        private static void TrackHighestId(string id, ref int highestSeen)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
                && numeric > highestSeen)
            {
                highestSeen = numeric;
            }
        }

        private string TakeId(ISet<string> claimed)
        {
            string id;
            do
            {
                id = AllocateId();
            }
            while (!claimed.Add(id));

            return id;
        }
    }

    [Serializable]
    public class KanbanColumn
    {
        public string Id = string.Empty;
        public string Title = string.Empty;

        // "#RRGGBB" - kept as text so the JSON stays readable and diffable by hand.
        public string ColorHex = string.Empty;

        public string Glyph = string.Empty;
        public List<KanbanTask> Tasks = new();

        public KanbanTask AddTask(KanbanBoard board, string title = "New task")
        {
            var task = new KanbanTask
            {
                Id = board.AllocateId(),
                Title = title
            };

            Tasks.Add(task);
            return task;
        }

        public Color ResolveColor()
        {
            if (!string.IsNullOrEmpty(ColorHex) && ColorUtility.TryParseHtmlString(ColorHex, out var parsed))
            {
                return parsed;
            }

            return KanbanPalette.Fallback;
        }

        public void SetColor(Color color)
        {
            ColorHex = "#" + ColorUtility.ToHtmlStringRGB(color);
        }
    }

    [Serializable]
    public class KanbanTask
    {
        public string Id = string.Empty;
        public string Title = string.Empty;
        public string Glyph = string.Empty;
        public string Description = string.Empty;

        // Reserved for the descriptor layer (priority, status, assignee...). Unused in the MVP;
        // JsonUtility writes it as [] which costs one line and keeps older files loadable later.
        public List<KanbanField> Fields = new();
    }

    [Serializable]
    public class KanbanField
    {
        public string Key = string.Empty;
        public string Value = string.Empty;
    }

    /// <summary>
    /// Default column colours. New columns cycle through these so a fresh board is legible without
    /// anyone having to open a colour picker.
    /// </summary>
    public static class KanbanPalette
    {
        public static readonly Color Fallback = new(0.42f, 0.47f, 0.56f);

        private static readonly string[] ColumnHexes =
        {
            "#5B8FF9", "#61DDAA", "#F6BD16", "#E8684A", "#9270CA", "#6DC8EC"
        };

        public static string NextColumnHex(int columnIndex)
        {
            if (columnIndex < 0) columnIndex = 0;
            return ColumnHexes[columnIndex % ColumnHexes.Length];
        }
    }
}
