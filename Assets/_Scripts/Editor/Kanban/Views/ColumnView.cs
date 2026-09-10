using System.Collections.Generic;

using _Scripts.Editor.Kanban.Model;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// One column: an editable header (glyph, title, colour) over a scrolling list of cards.
    /// Column reordering lives in the "..." menu rather than in a drag - the menu costs a fraction of
    /// the work and the drag code here generalises to columns later if it turns out to be wanted.
    /// </summary>
    public class ColumnView
    {
        private readonly IKanbanHost _host;
        private readonly VisualTreeAsset _cardTemplate;
        private readonly VisualElement _accent;
        private readonly Label _count;

        private readonly List<TaskCardView> _cards = new();

        public VisualElement Root { get; }
        public VisualElement TasksContainer { get; }
        public ScrollView Scroll { get; }
        public KanbanColumn Column { get; }

        public ColumnView(IKanbanHost host, KanbanColumn column, VisualTreeAsset columnTemplate, VisualTreeAsset cardTemplate)
        {
            _host = host;
            _cardTemplate = cardTemplate;
            Column = column;

            var instance = columnTemplate.Instantiate();
            Root = instance.Q<VisualElement>("column");
            Root.userData = this;

            _accent = Root.Q<VisualElement>("column-accent");
            _count = Root.Q<Label>("column-count");
            Scroll = Root.Q<ScrollView>("column-scroll");
            TasksContainer = Root.Q<VisualElement>("column-tasks");

            BindGlyph();
            BindTitle();
            BindColor();
            BindButtons();

            ApplyAccentColor();
            RebuildTasks();
        }

        public void RebuildTasks()
        {
            _cards.Clear();
            TasksContainer.Clear();

            foreach (var task in Column.Tasks)
            {
                var card = new TaskCardView(_host, this, task, _cardTemplate);
                _cards.Add(card);
                TasksContainer.Add(card.Root);
            }

            _count.text = Column.Tasks.Count.ToString();
        }

        private void BindGlyph()
        {
            GlyphField.Bind(
                Root.Q<TextField>("column-glyph"),
                _host,
                "Set Column Glyph",
                () => Column.Glyph,
                value => Column.Glyph = value);
        }

        private void BindTitle()
        {
            KanbanFieldBinder.BindText(
                Root.Q<TextField>("column-title"),
                _host,
                "Rename Column",
                () => Column.Title,
                value => Column.Title = value);
        }

        private void BindColor()
        {
            var field = Root.Q<ColorField>("column-color");
            field.showAlpha = false;
            field.SetValueWithoutNotify(Column.ResolveColor());

            field.RegisterValueChangedCallback(evt =>
            {
                _host.RecordUndo("Set Column Colour");
                Column.SetColor(evt.newValue);
                _host.MarkDirty();
                ApplyAccentColor();
            });
        }

        private void ApplyAccentColor()
        {
            _accent.style.backgroundColor = Column.ResolveColor();
        }

        private void BindButtons()
        {
            Root.Q<Button>("column-add").clicked += AddTask;
            Root.Q<Button>("column-menu").clicked += ShowMenu;
        }

        private void AddTask()
        {
            _host.RecordUndo("Add Task");
            Column.AddTask(_host.Board);
            _host.MarkDirty();

            RebuildTasks();
            Scroll.schedule.Execute(() => Scroll.scrollOffset = new Vector2(0f, KanbanWindow.ScrollToEnd));
        }

        private void ShowMenu()
        {
            var board = _host.Board;
            var index = board.IndexOf(Column);

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Task"), false, AddTask);
            menu.AddSeparator(string.Empty);

            AddMoveItem(menu, "Move Left", index > 0, () => MoveColumn(index, index - 1));
            AddMoveItem(menu, "Move Right", index >= 0 && index < board.Columns.Count - 1, () => MoveColumn(index, index + 1));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete Column"), false, DeleteColumn);
            menu.ShowAsContext();
        }

        private static void AddMoveItem(GenericMenu menu, string label, bool enabled, GenericMenu.MenuFunction action)
        {
            var content = new GUIContent(label);

            if (enabled) menu.AddItem(content, false, action);
            else menu.AddDisabledItem(content, false);
        }

        private void MoveColumn(int from, int to)
        {
            _host.RecordUndo("Move Column");
            _host.Board.MoveColumn(from, to);
            _host.MarkDirty();
            _host.RebuildBoard();
        }

        private void DeleteColumn()
        {
            var label = string.IsNullOrEmpty(Column.Title) ? "this column" : $"\"{Column.Title}\"";

            if (Column.Tasks.Count > 0)
            {
                var confirmed = EditorUtility.DisplayDialog(
                    "Delete Column",
                    $"Delete {label} and its {Column.Tasks.Count} task(s)?\n\nThis can be undone with Ctrl+Z.",
                    "Delete",
                    "Cancel");

                if (!confirmed) return;
            }

            _host.RecordUndo("Delete Column");
            _host.Board.Columns.Remove(Column);
            _host.MarkDirty();
            _host.RebuildBoard();
        }
    }
}
