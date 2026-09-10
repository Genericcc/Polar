using System;
using System.Collections.Generic;

using _Scripts.Editor.Kanban.Model;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// The descriptor half of a task card: a compact chip row that is always visible, and a full set of
    /// editor rows that live inside the expanded area next to the description.
    ///
    /// Chips render only descriptors the task actually has a value for. That is what keeps a board with
    /// five descriptors from turning every card into a form - an untriaged card shows nothing extra, and
    /// the moment it gets a priority the chip appears.
    /// </summary>
    public class TaskFieldsView
    {
        private readonly IKanbanHost _host;
        private readonly KanbanTask _task;
        private readonly VisualElement _chips;
        private readonly VisualElement _editors;

        /// <summary>Raised when a change may have altered this card's position under the active sort.</summary>
        private readonly Action _onOrderAffected;

        /// <summary>Asks the card to reveal (and focus) a descriptor that has no inline chip editor.</summary>
        private readonly Action<KanbanFieldDef> _onEditRequested;

        /// <summary>Text/Number editors by descriptor key, so a chip click can land the caret in one.</summary>
        private readonly Dictionary<string, TextField> _inputsByKey = new(StringComparer.Ordinal);

        public TaskFieldsView(
            IKanbanHost host,
            KanbanTask task,
            VisualElement chips,
            VisualElement editors,
            Action onOrderAffected,
            Action<KanbanFieldDef> onEditRequested)
        {
            _host = host;
            _task = task;
            _chips = chips;
            _editors = editors;
            _onOrderAffected = onOrderAffected;
            _onEditRequested = onEditRequested;
        }

        public void Rebuild()
        {
            RebuildChips();
            RebuildEditors();
        }

        // ------------------------------------------------------------------ chips

        public void RebuildChips()
        {
            _chips.Clear();

            var board = _host.Board;

            if (board == null) return;

            var any = false;

            foreach (var def in board.FieldDefs)
            {
                if (!_task.HasField(def.Key)) continue;

                _chips.Add(BuildChip(board, def));
                any = true;
            }

            // No values means no row at all, not an empty row - an 8px gap under every untriaged card
            // adds up fast down a long column.
            _chips.EnableInClassList("card__chips--empty", !any);
        }

        private VisualElement BuildChip(KanbanBoard board, KanbanFieldDef def)
        {
            var value = _task.GetField(def.Key);

            var chip = new Button { tooltip = def.Label };
            chip.AddToClassList("chip");
            chip.clicked += () => OnChipClicked(def);

            var option = def.Kind == KanbanFieldKind.Select ? def.FindOption(value) : null;

            if (option != null)
            {
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("chip__dot");
                dot.style.backgroundColor = option.ResolveColor();
                chip.Add(dot);
            }

            var label = new Label(value) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("chip__label");
            chip.Add(label);

            // Marking the chip the board is ordered by answers "why is this card here?" without the
            // user having to look back up at the toolbar.
            if (board.SortActive && string.Equals(board.Sort.Key, def.Key, StringComparison.Ordinal))
            {
                chip.AddToClassList("chip--sort-key");
            }

            return chip;
        }

        private void OnChipClicked(KanbanFieldDef def)
        {
            // A Select has a closed set of values, so the click can resolve it in place. Text and Number
            // need a keyboard, which means revealing the real editor.
            if (def.Kind == KanbanFieldKind.Select) ShowSelectMenu(def);
            else _onEditRequested?.Invoke(def);
        }

        // ------------------------------------------------------------------ editors

        public void RebuildEditors()
        {
            _editors.Clear();
            _inputsByKey.Clear();

            var board = _host.Board;

            if (board == null) return;

            foreach (var def in board.FieldDefs) _editors.Add(BuildEditorRow(def));

            _editors.EnableInClassList("card__fields--empty", board.FieldDefs.Count == 0);
        }

        /// <summary>
        /// Puts the caret in a descriptor's editor. Deferred a frame: the card has usually just been
        /// expanded, and focusing an element the layout has not placed yet does not stick.
        /// </summary>
        public void FocusEditor(KanbanFieldDef def)
        {
            if (!_inputsByKey.TryGetValue(def.Key, out var input)) return;

            input.schedule.Execute(() =>
            {
                input.Focus();
                input.SelectAll();
            });
        }

        private VisualElement BuildEditorRow(KanbanFieldDef def)
        {
            var row = new VisualElement();
            row.AddToClassList("field-row");

            var label = new Label(def.Label);
            label.AddToClassList("field-row__label");
            row.Add(label);

            row.Add(def.Kind == KanbanFieldKind.Select ? BuildSelectButton(def) : BuildTextInput(def));

            return row;
        }

        private VisualElement BuildSelectButton(KanbanFieldDef def)
        {
            var value = _task.GetField(def.Key);

            var button = new Button { text = string.IsNullOrEmpty(value) ? "—" : value };
            button.AddToClassList("field-row__select");
            button.clicked += () => ShowSelectMenu(def);

            return button;
        }

        private VisualElement BuildTextInput(KanbanFieldDef def)
        {
            var field = new TextField();
            field.AddToClassList("field-row__input");

            KanbanFieldBinder.BindText(
                field,
                _host,
                $"Set {def.Label}",
                () => _task.GetField(def.Key),
                value => _task.SetField(def.Key, value));

            // Chips can be refreshed mid-edit safely - they live in a different container than the field
            // being typed into, so nothing under the caret is destroyed.
            field.RegisterValueChangedCallback(_ => RebuildChips());

            // Re-sorting on every keystroke would make the card scroll away under the cursor, so the
            // reorder waits for the edit to finish.
            field.RegisterCallback<FocusOutEvent>(_ => NotifyIfSortedBy(def));

            _inputsByKey[def.Key] = field;

            return field;
        }

        private void ShowSelectMenu(KanbanFieldDef def)
        {
            var current = _task.GetField(def.Key);
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("(none)"), string.IsNullOrEmpty(current), () => Apply(def, string.Empty));

            if (def.Options.Count > 0) menu.AddSeparator(string.Empty);

            foreach (var option in def.Options)
            {
                var captured = option.Value;

                // '/' would open a submenu in GenericMenu; swap in the division slash, which looks the
                // same. Same trick as the card's Move To menu.
                var content = new GUIContent(captured.Replace('/', '∕'));

                menu.AddItem(
                    content,
                    string.Equals(captured, current, StringComparison.OrdinalIgnoreCase),
                    () => Apply(def, captured));
            }

            menu.ShowAsContext();
        }

        private void Apply(KanbanFieldDef def, string value)
        {
            if (string.Equals(_task.GetField(def.Key), value, StringComparison.Ordinal)) return;

            _host.RecordUndo($"Set {def.Label}");
            _task.SetField(def.Key, value);
            _host.MarkDirty();

            Rebuild();
            NotifyIfSortedBy(def);
        }

        private void NotifyIfSortedBy(KanbanFieldDef def)
        {
            var board = _host.Board;

            if (board == null || !board.SortActive) return;
            if (!string.Equals(board.Sort.Key, def.Key, StringComparison.Ordinal)) return;

            _onOrderAffected?.Invoke();
        }
    }
}
