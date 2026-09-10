using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// The board's text size setting.
    ///
    /// Stored in <see cref="EditorPrefs"/> rather than in board.json: how large one person likes their
    /// text is a personal preference, not a property of the board, and writing it to the shared file
    /// would put a git diff behind every adjustment. Same reasoning as the expand-state set, except this
    /// one has to outlive the session, so EditorPrefs rather than SessionState.
    ///
    /// Applying a size swaps a single class on the window root. That class redefines a group of USS
    /// variables - text sizes, but also control heights, the glyph slot and the column width - because
    /// several controls have fixed pixel sizes that would clip their own text if only the font grew.
    /// </summary>
    public static class KanbanFontScale
    {
        private const string PrefKey = "Polar.Kanban.FontSize";

        /// <summary>Comfortably above Unity's ~12px editor default, which reads small on a dense board.</summary>
        public const int DefaultSize = 14;

        /// <summary>
        /// The offered sizes. A short discrete list rather than a free slider: every step needs a matching
        /// block of USS variables, and a handful of well-proportioned steps beats arbitrary values.
        /// </summary>
        public static readonly int[] Sizes = { 11, 12, 13, 14, 16, 18, 20 };

        /// <summary>Raised when the size changes, so every open Kanban window can restyle itself.</summary>
        public static event Action Changed;

        public static int Current
        {
            get => Nearest(EditorPrefs.GetInt(PrefKey, DefaultSize));
            set
            {
                var size = Nearest(value);

                if (size == Current) return;

                EditorPrefs.SetInt(PrefKey, size);
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Puts the current size's class on a window root, clearing any previous one. Safe to call
        /// repeatedly - it is how a window picks up a change made from another window.
        /// </summary>
        public static void Apply(VisualElement root)
        {
            if (root == null) return;

            foreach (var size in Sizes) root.RemoveFromClassList(ClassFor(size));

            root.AddToClassList(ClassFor(Current));
        }

        public static string ClassFor(int size)
        {
            return "kanban-scale--" + size;
        }

        /// <summary>
        /// Snaps to the closest offered size. A hand-edited pref, or a value from a build that offered a
        /// size this one does not, still resolves to something with a matching USS block.
        /// </summary>
        private static int Nearest(int size)
        {
            var best = DefaultSize;
            var bestDistance = int.MaxValue;

            foreach (var candidate in Sizes)
            {
                var distance = Mathf.Abs(candidate - size);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                best = candidate;
            }

            return best;
        }
    }
}
