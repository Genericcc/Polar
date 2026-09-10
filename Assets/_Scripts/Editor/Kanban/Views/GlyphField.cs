using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// The little icon slot on columns and cards.
    ///
    /// Icons are stored as a plain string, so anything pasted in works - but Unity's editor font stack
    /// does not reliably render colour emoji (many codepoints come out as tofu boxes). The right-click
    /// palette therefore offers BMP symbols that the editor font does render, while the field itself
    /// stays free-text for anyone whose font stack handles emoji fine.
    /// </summary>
    public static class GlyphField
    {
        private static readonly string[] Palette =
        {
            "★", "☆", "✦", "✿", // star, hollow star, sparkle, floret
            "✔", "✖", "✘", "➕", // check, heavy x, ballot x, plus
            "⚑", "⚐", "⚠", "⚡", // flag, hollow flag, warning, bolt
            "⚙", "⚒", "✂", "✎", // gear, hammer+pick, scissors, pencil
            "◆", "●", "▲", "▼", // diamond, circle, up, down
            "■", "○", "⏱", "↻", // square, ring, stopwatch, refresh
            "↔", "⇅", "§", "№"  // left-right, up-down, section, numero
        };

        /// <summary>Room for a surrogate pair, so a single astral-plane emoji still fits.</summary>
        private const int MaxGlyphLength = 2;

        public static void Bind(
            TextField field,
            IKanbanHost host,
            string undoLabel,
            Func<string> read,
            Action<string> write)
        {
            field.maxLength = MaxGlyphLength;

            KanbanFieldBinder.BindText(field, host, undoLabel, read, write);

            field.RegisterCallback<ContextClickEvent>(evt =>
            {
                ShowPalette(field);
                evt.StopPropagation();
            });
        }

        private static void ShowPalette(TextField field)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("(none)"), string.IsNullOrEmpty(field.value), () => field.value = string.Empty);
            menu.AddSeparator(string.Empty);

            foreach (var glyph in Palette)
            {
                var captured = glyph;
                menu.AddItem(new GUIContent(glyph), field.value == glyph, () => field.value = captured);
            }

            menu.ShowAsContext();
        }
    }
}
