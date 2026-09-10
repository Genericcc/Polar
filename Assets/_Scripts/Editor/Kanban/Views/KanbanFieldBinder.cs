using System;

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
    }
}
