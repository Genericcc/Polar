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
        // 2: added FieldDefs + Sort. A v1 file still loads - the new members simply default to
        // "no descriptors, manual order", which is exactly what a v1 board meant.
        public const int CurrentSchemaVersion = 2;

        public int SchemaVersion = CurrentSchemaVersion;
        public string Title = "Polar Board";

        // Monotonic id source. Ids are never reused, so a card keeps its identity across moves -
        // which is what the expand-state set and the sort tiebreak key off.
        public int NextId = 1;

        public List<KanbanColumn> Columns = new();

        /// <summary>Task descriptors available board-wide. See <see cref="KanbanFieldDef"/>.</summary>
        public List<KanbanFieldDef> FieldDefs = new();

        public KanbanSortState Sort = new();

        public static KanbanBoard CreateDefault()
        {
            var board = new KanbanBoard();
            board.AddColumn("Backlog");
            board.AddColumn("In Progress");
            board.AddColumn("Done");

            // A board with no descriptors cannot demonstrate sorting, and priority is the one every
            // user adds first anyway. Ordered high-to-low so ascending reads as "most urgent first".
            var priority = board.AddFieldDef("Priority", KanbanFieldKind.Select);
            priority.AddOption("Critical");
            priority.AddOption("High");
            priority.AddOption("Normal");
            priority.AddOption("Low");

            // New cards start at Normal rather than unset: an unset priority sorts to the bottom, so a
            // freshly added card would otherwise vanish under the triaged ones on a sorted board.
            priority.DefaultValue = "Normal";

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

        // ------------------------------------------------------------------ descriptors

        public KanbanFieldDef FindFieldDef(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            foreach (var def in FieldDefs)
            {
                if (string.Equals(def.Key, key, StringComparison.Ordinal)) return def;
            }

            return null;
        }

        public KanbanFieldDef AddFieldDef(string displayName, KanbanFieldKind kind)
        {
            var def = new KanbanFieldDef
            {
                Key = UniqueFieldKey(KanbanFieldDef.MakeKey(displayName)),
                DisplayName = displayName,
                Kind = kind
            };

            FieldDefs.Add(def);
            return def;
        }

        /// <summary>
        /// Drops a descriptor and every value tasks were holding for it. The values go too on purpose:
        /// leaving them behind would silently resurrect stale data if a key were ever reused, and they
        /// would sit in the JSON forever with nothing referencing them. Undo covers the mistake case.
        /// </summary>
        public void RemoveFieldDef(KanbanFieldDef def)
        {
            if (def == null) return;

            FieldDefs.Remove(def);

            foreach (var column in Columns)
            {
                foreach (var task in column.Tasks) task.ClearField(def.Key);
            }

            if (string.Equals(Sort.Key, def.Key, StringComparison.Ordinal)) Sort.Enabled = false;
        }

        public void MoveFieldDef(int from, int to)
        {
            if (from < 0 || from >= FieldDefs.Count) return;
            if (to < 0 || to >= FieldDefs.Count) return;
            if (from == to) return;

            var def = FieldDefs[from];
            FieldDefs.RemoveAt(from);
            FieldDefs.Insert(to, def);
        }

        private string UniqueFieldKey(string baseKey)
        {
            if (FindFieldDef(baseKey) == null) return baseKey;

            var suffix = 2;
            while (FindFieldDef($"{baseKey}-{suffix}") != null) suffix++;

            return $"{baseKey}-{suffix}";
        }

        // ------------------------------------------------------------------ sorting

        /// <summary>
        /// True when columns should render through the comparator. Sorting by a descriptor that has
        /// since been deleted silently falls back to manual order rather than erroring.
        /// </summary>
        public bool SortActive => Sort is { Enabled: true } && FindFieldDef(Sort.Key) != null;

        /// <summary>
        /// The order a column's cards should be displayed in. Returns the backing list itself when
        /// sorting is off, so the common path allocates nothing.
        /// </summary>
        public IReadOnlyList<KanbanTask> OrderTasks(List<KanbanTask> tasks)
        {
            if (tasks == null) return Array.Empty<KanbanTask>();
            if (!SortActive) return tasks;

            var def = FindFieldDef(Sort.Key);

            // Decorate with the manual index and use it as the final tiebreak: List.Sort is not stable,
            // and cards of equal priority swapping places on every rebuild would look like a bug.
            var decorated = new List<KeyValuePair<KanbanTask, int>>(tasks.Count);

            for (var index = 0; index < tasks.Count; index++)
            {
                decorated.Add(new KeyValuePair<KanbanTask, int>(tasks[index], index));
            }

            decorated.Sort((left, right) =>
            {
                var comparison = CompareByField(left.Key, right.Key, def);
                return comparison != 0 ? comparison : left.Value.CompareTo(right.Value);
            });

            var ordered = new List<KanbanTask>(decorated.Count);
            foreach (var entry in decorated) ordered.Add(entry.Key);

            return ordered;
        }

        private int CompareByField(KanbanTask left, KanbanTask right, KanbanFieldDef def)
        {
            var leftValue = left.GetField(def.Key);
            var rightValue = right.GetField(def.Key);

            var leftEmpty = string.IsNullOrEmpty(leftValue);
            var rightEmpty = string.IsNullOrEmpty(rightValue);

            // Unset sinks to the bottom in BOTH directions. Flipping the arrow and having the cards
            // nobody has triaged yet jump to the top would read as a bug, not as a sort.
            if (leftEmpty || rightEmpty)
            {
                if (leftEmpty && rightEmpty) return 0;
                return leftEmpty ? 1 : -1;
            }

            var result = def.Kind switch
            {
                KanbanFieldKind.Select => def.IndexOfOption(leftValue).CompareTo(def.IndexOfOption(rightValue)),
                KanbanFieldKind.Number => CompareNumbers(leftValue, rightValue),
                _ => string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase)
            };

            return Sort.Ascending ? result : -result;
        }

        /// <summary>Unparseable numbers sort as equal to one another and after everything else.</summary>
        private static int CompareNumbers(string leftValue, string rightValue)
        {
            var leftOk = KanbanNumber.TryParse(leftValue, out var left);
            var rightOk = KanbanNumber.TryParse(rightValue, out var right);

            if (leftOk && rightOk) return left.CompareTo(right);
            if (leftOk) return -1;
            if (rightOk) return 1;

            return 0;
        }

        /// <summary>
        /// Repairs anything a hand-edited (or older) JSON file might be missing. Load always runs this,
        /// so the rest of the tool can assume non-null lists and unique, non-empty ids.
        /// </summary>
        public void EnsureValid()
        {
            Columns ??= new List<KanbanColumn>();
            Title ??= "Polar Board";

            EnsureFieldDefsValid();

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

        /// <summary>
        /// Normalises the descriptor list. Task *values* are deliberately left alone even when no def
        /// matches them: a file where the defs were hand-edited out should not lose its data as a side
        /// effect of being opened. Only an explicit <see cref="RemoveFieldDef"/> prunes values.
        /// </summary>
        private void EnsureFieldDefsValid()
        {
            FieldDefs ??= new List<KanbanFieldDef>();
            Sort ??= new KanbanSortState();
            Sort.Key ??= string.Empty;

            var claimedKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var def in FieldDefs)
            {
                def.Options ??= new List<KanbanFieldOption>();
                def.DisplayName ??= string.Empty;
                def.DefaultValue ??= string.Empty;

                if (string.IsNullOrEmpty(def.Key)) def.Key = KanbanFieldDef.MakeKey(def.DisplayName);

                // A duplicate key would make FindFieldDef return the wrong def for every task value.
                while (!claimedKeys.Add(def.Key)) def.Key += "-2";

                if (string.IsNullOrEmpty(def.DisplayName)) def.DisplayName = def.Key;

                EnsureOptionsValid(def);
            }
        }

        private static void EnsureOptionsValid(KanbanFieldDef def)
        {
            for (var index = def.Options.Count - 1; index >= 0; index--)
            {
                var option = def.Options[index];

                option.ColorHex ??= string.Empty;
                option.Value ??= string.Empty;

                // A blank option is indistinguishable from "(none)" in the picker and can never be
                // selected, so it goes. Duplicates are deliberately left alone: renaming an option in the
                // fields editor passes through duplicate states keystroke by keystroke, and pruning them
                // here would quietly delete the user's option the next time the board was loaded. A
                // duplicate is harmless anyway - lookups resolve to the first match.
                if (string.IsNullOrWhiteSpace(option.Value)) def.Options.RemoveAt(index);
            }
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

            // Stamp each descriptor's default. SetField skips empties, so a descriptor with no default
            // still costs the new task nothing in the JSON.
            foreach (var def in board.FieldDefs) task.SetField(def.Key, def.DefaultValue);

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

        /// <summary>
        /// Values for the board's <see cref="KanbanFieldDef"/>s, keyed by def key. Sparse: a task only
        /// carries entries for descriptors it actually has a value for, so an untouched card stays a
        /// three-line object in the JSON no matter how many descriptors the board defines.
        /// </summary>
        public List<KanbanField> Fields = new();

        public string GetField(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            foreach (var field in Fields)
            {
                if (string.Equals(field.Key, key, StringComparison.Ordinal)) return field.Value ?? string.Empty;
            }

            return string.Empty;
        }

        public bool HasField(string key)
        {
            return !string.IsNullOrEmpty(GetField(key));
        }

        /// <summary>Setting a value to empty removes the entry - see the sparseness note on <see cref="Fields"/>.</summary>
        public void SetField(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (string.IsNullOrEmpty(value))
            {
                ClearField(key);
                return;
            }

            foreach (var field in Fields)
            {
                if (!string.Equals(field.Key, key, StringComparison.Ordinal)) continue;

                field.Value = value;
                return;
            }

            Fields.Add(new KanbanField { Key = key, Value = value });
        }

        public void ClearField(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            for (var index = Fields.Count - 1; index >= 0; index--)
            {
                if (string.Equals(Fields[index].Key, key, StringComparison.Ordinal)) Fields.RemoveAt(index);
            }
        }
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

        // A hot-to-cold ramp rather than the column palette's spread of hues. Select options are almost
        // always a severity scale, and options are added top-down, so index order lands the hot colours
        // on the urgent end without anyone opening a picker.
        private static readonly string[] OptionHexes =
        {
            "#E8684A", "#F6BD16", "#5B8FF9", "#61DDAA", "#9270CA", "#6DC8EC"
        };

        public static string NextColumnHex(int columnIndex)
        {
            if (columnIndex < 0) columnIndex = 0;
            return ColumnHexes[columnIndex % ColumnHexes.Length];
        }

        public static string NextOptionHex(int optionIndex)
        {
            if (optionIndex < 0) optionIndex = 0;
            return OptionHexes[optionIndex % OptionHexes.Length];
        }
    }
}
