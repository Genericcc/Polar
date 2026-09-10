using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using UnityEngine;

namespace _Scripts.Editor.Kanban.Model
{
    /// <summary>
    /// What kind of editor a descriptor gets, and how the sort comparator reads its values.
    /// Serialized by JsonUtility as the underlying int, so the explicit numbers are load-bearing -
    /// never renumber them, only append.
    /// </summary>
    public enum KanbanFieldKind
    {
        Text = 0,
        Number = 1,
        Select = 2
    }

    /// <summary>
    /// One allowed value of a <see cref="KanbanFieldKind.Select"/> field.
    ///
    /// The position of an option inside <see cref="KanbanFieldDef.Options"/> *is* its sort rank. That is
    /// the whole reason Select exists as a separate kind: "Critical" has to outrank "High" even though
    /// it sorts before it alphabetically, and asking the user to number their own priorities would be
    /// busywork. Reordering the list in the fields editor reorders the board.
    /// </summary>
    [Serializable]
    public class KanbanFieldOption
    {
        public string Value = string.Empty;

        // "#RRGGBB", same convention as KanbanColumn.ColorHex - readable and hand-diffable.
        public string ColorHex = string.Empty;

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

    /// <summary>
    /// A descriptor that every task on the board may carry a value for - priority, status, assignee.
    ///
    /// Adding one is data, not code: the card renders its chip row and its editor rows straight from
    /// this list, so a new descriptor costs an entry here and nothing else.
    /// </summary>
    [Serializable]
    public class KanbanFieldDef
    {
        /// <summary>Stable identity, matched against <see cref="KanbanField.Key"/>. Never shown to the user.</summary>
        public string Key = string.Empty;

        public string DisplayName = string.Empty;

        public KanbanFieldKind Kind = KanbanFieldKind.Text;

        /// <summary>Select only. Order is sort order - see <see cref="KanbanFieldOption"/>.</summary>
        public List<KanbanFieldOption> Options = new();

        /// <summary>
        /// Value stamped onto every newly created task. Empty means "leave it unset".
        ///
        /// A per-descriptor property rather than a rule about one well-known descriptor: the whole point
        /// of the field system is that adding "Priority" is data, and a default that only worked for a
        /// key literally spelled "priority" would put a hole in that.
        ///
        /// It applies at creation only. Changing the default later deliberately does NOT retro-fit
        /// existing tasks - that would silently rewrite triage decisions someone already made.
        /// </summary>
        public string DefaultValue = string.Empty;

        public string Label => string.IsNullOrEmpty(DisplayName) ? Key : DisplayName;

        /// <summary>
        /// Sort rank of a value. Anything not in the option list ranks after every known option rather
        /// than before it - a value left over from a renamed option should sink, not lead.
        /// </summary>
        public int IndexOfOption(string value)
        {
            for (var index = 0; index < Options.Count; index++)
            {
                if (string.Equals(Options[index].Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return Options.Count;
        }

        public KanbanFieldOption FindOption(string value)
        {
            foreach (var option in Options)
            {
                if (string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase)) return option;
            }

            return null;
        }

        public KanbanFieldOption AddOption(string value)
        {
            var option = new KanbanFieldOption { Value = value };
            option.ColorHex = KanbanPalette.NextOptionHex(Options.Count);

            Options.Add(option);
            return option;
        }

        public void MoveOption(int from, int to)
        {
            if (from < 0 || from >= Options.Count) return;
            if (to < 0 || to >= Options.Count) return;
            if (from == to) return;

            var option = Options[from];
            Options.RemoveAt(from);
            Options.Insert(to, option);
        }

        /// <summary>
        /// Derives a key from a display name: lowercase, non-alphanumerics collapsed to '-'. Keys are
        /// internal, so this only has to be stable and readable in the JSON, not pretty.
        /// </summary>
        public static string MakeKey(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "field";

            var builder = new StringBuilder(displayName.Length);
            var lastWasSeparator = false;

            foreach (var character in displayName.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    lastWasSeparator = false;
                }
                else if (!lastWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                    lastWasSeparator = true;
                }
            }

            var key = builder.ToString().Trim('-');
            return key.Length == 0 ? "field" : key;
        }
    }

    /// <summary>
    /// Which descriptor the board is currently ordered by, if any.
    ///
    /// Sorting is a *view* transform - <see cref="KanbanColumn.Tasks"/> keeps its manual order at all
    /// times, so switching sorting off restores exactly the arrangement the user dragged into place.
    /// </summary>
    [Serializable]
    public class KanbanSortState
    {
        public bool Enabled;
        public string Key = string.Empty;
        public bool Ascending = true;
    }

    /// <summary>
    /// Parses a Number field value. Invariant culture on purpose: the JSON is shared through git, and a
    /// board written on a machine with a comma decimal separator has to read back the same everywhere.
    /// </summary>
    public static class KanbanNumber
    {
        public static bool TryParse(string value, out double number)
        {
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number);
        }
    }
}
