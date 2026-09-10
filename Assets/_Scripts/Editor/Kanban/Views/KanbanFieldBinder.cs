using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// Binds a flat inline <see cref="TextField"/> to a model string.
    ///
    /// Edits apply live (so the card always shows the truth) but only the FIRST change after the field
    /// gains focus records an undo step. Typing a ten-character title therefore costs one Ctrl+Z, not ten,
    /// and simply clicking into a field without typing records nothing at all.
    /// </summary>
    public static class KanbanFieldBinder
    {
        public static void BindText(
            TextField field,
            IKanbanHost host,
            string undoLabel,
            Func<string> read,
            Action<string> write)
        {
            field.SetValueWithoutNotify(read() ?? string.Empty);

            var undoRecordedForThisEdit = false;

            field.RegisterCallback<FocusInEvent>(_ => undoRecordedForThisEdit = false);

            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == read()) return;

                if (!undoRecordedForThisEdit)
                {
                    host.RecordUndo(undoLabel);
                    undoRecordedForThisEdit = true;
                }

                write(evt.newValue);
                host.MarkDirty();
            });
        }

        /// <summary>
        /// Makes Enter finish the edit on a field that is only multiline for the sake of wrapping.
        ///
        /// A multiline TextField treats Return as "insert a newline", which is right for a description
        /// and wrong for a title - the title wraps on its own, and someone pressing Enter after typing
        /// one means "done", not "give me a second line". Intercepted on the way down so the text engine
        /// never sees the keystroke.
        /// </summary>
        public static void CommitOnEnter(TextField field)
        {
            field.RegisterCallback<KeyDownEvent>(
                evt =>
                {
                    var isEnter = evt.keyCode == KeyCode.Return
                                  || evt.keyCode == KeyCode.KeypadEnter
                                  || evt.character == '\n';

                    if (!isEnter) return;

                    field.Blur();

                    // Stopping it here on the way down is what keeps the text engine from inserting the
                    // newline - the editing happens on the inner input element, deeper in the tree, so
                    // the event never reaches it. (PreventDefault is deprecated in Unity 6 and would add
                    // nothing here.)
                    evt.StopImmediatePropagation();
                },
                TrickleDown.TrickleDown);
        }
    }
}
