using _Scripts.Editor.Kanban.Model;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// One task card: glyph, title, and a description folded away behind the arrow. Expanded/collapsed
    /// is view state and is never written to the board file - it lives in the window's session state,
    /// so toggling an arrow does not produce a git diff.
    /// </summary>
    public class TaskCardView
    {
        private const string CollapsedArrow = "▸"; // black right-pointing small triangle
        private const string ExpandedArrow = "▾";  // black down-pointing small triangle

        private readonly IKanbanHost _host;
        private readonly VisualElement _descriptionHolder;
        private readonly Button _toggle;
        private readonly TaskFieldsView _fields;

        public VisualElement Root { get; }
        public VisualElement Grip { get; }
        public KanbanTask Task { get; }
        public ColumnView Owner { get; }

        public TaskCardView(IKanbanHost host, ColumnView owner, KanbanTask task, VisualTreeAsset template)
        {
            _host = host;
            Owner = owner;
            Task = task;

            var instance = template.Instantiate();
            Root = instance.Q<VisualElement>("task-card");
            Root.userData = this;

            Grip = Root.Q<VisualElement>("card-grip");
            _toggle = Root.Q<Button>("card-toggle");
            _descriptionHolder = Root.Q<VisualElement>("card-description-holder");

            _fields = new TaskFieldsView(
                host,
                task,
                Root.Q<VisualElement>("card-chips"),
                Root.Q<VisualElement>("card-fields"),
                ReorderAfterFieldChange,
                RevealField);

            BindGlyph();
            BindTitle();
            BindDescription();
            BindToggle();
            BindMenu();

            _fields.Rebuild();

            Root.AddManipulator(new TaskDragManipulator(host, this));
        }

        /// <summary>
        /// A descriptor with no inline editor (Text, Number) was clicked on its chip - open the card so
        /// the real field is reachable.
        /// </summary>
        private void RevealField(KanbanFieldDef def)
        {
            if (!_host.IsExpanded(Task.Id))
            {
                _host.SetExpanded(Task.Id, true);
                ApplyExpanded(true);
            }

            _fields.FocusEditor(def);
        }

        /// <summary>
        /// The edited value feeds the active sort, so this card may belong somewhere else now. Rebuilding
        /// destroys this very view, so it waits for the current event to finish dispatching - the same
        /// reason <see cref="TaskDragManipulator"/> defers its rebuild.
        /// </summary>
        private void ReorderAfterFieldChange()
        {
            var column = Owner.Column;
            _host.DragLayer.schedule.Execute(() => _host.RebuildColumn(column));
        }

        private void BindGlyph()
        {
            GlyphField.Bind(
                Root.Q<TextField>("card-glyph"),
                _host,
                "Set Task Glyph",
                () => Task.Glyph,
                value => Task.Glyph = value);
        }

        private void BindTitle()
        {
            var field = Root.Q<TextField>("card-title");

            KanbanFieldBinder.BindText(
                field,
                _host,
                "Rename Task",
                () => Task.Title,
                value => Task.Title = value);

            // The field is multiline only so long titles wrap - Enter should still mean "done".
            KanbanFieldBinder.CommitOnEnter(field);
        }

        private void BindDescription()
        {
            KanbanFieldBinder.BindText(
                Root.Q<TextField>("card-description"),
                _host,
                "Edit Description",
                () => Task.Description,
                value => Task.Description = value);
        }

        private void BindToggle()
        {
            ApplyExpanded(_host.IsExpanded(Task.Id));

            _toggle.clicked += () =>
            {
                var expanded = !_host.IsExpanded(Task.Id);
                _host.SetExpanded(Task.Id, expanded);
                ApplyExpanded(expanded);
            };
        }

        private void ApplyExpanded(bool expanded)
        {
            _toggle.text = expanded ? ExpandedArrow : CollapsedArrow;
            _descriptionHolder.EnableInClassList("card__desc-holder--expanded", expanded);
        }

        private void BindMenu()
        {
            var menuButton = Root.Q<Button>("card-menu");
            menuButton.clicked += ShowMenu;
        }

        private void ShowMenu()
        {
            var menu = new GenericMenu();
            var board = _host.Board;

            foreach (var column in board.Columns)
            {
                var target = column;
                var label = string.IsNullOrEmpty(column.Title) ? "(untitled)" : column.Title;

                // '/' opens a submenu in GenericMenu, so a column called "A/B" would nest instead of
                // showing up as one entry. Swap it for the division slash, which looks the same.
                var content = new GUIContent($"Move To/{label.Replace('/', '∕')}");

                if (target == Owner.Column)
                {
                    menu.AddDisabledItem(content, true);
                }
                else
                {
                    menu.AddItem(content, false, () => MoveTo(target));
                }
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete Task"), false, Delete);
            menu.ShowAsContext();
        }

        private void MoveTo(KanbanColumn target)
        {
            _host.RecordUndo("Move Task");
            Owner.Column.Tasks.Remove(Task);
            target.Tasks.Add(Task);
            _host.MarkDirty();

            _host.RebuildColumn(Owner.Column);
            _host.RebuildColumn(target);
        }

        private void Delete()
        {
            _host.RecordUndo("Delete Task");
            Owner.Column.Tasks.Remove(Task);
            _host.MarkDirty();
            _host.RebuildColumn(Owner.Column);
        }
    }
}
