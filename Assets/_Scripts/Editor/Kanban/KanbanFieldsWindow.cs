using System;

using _Scripts.Editor.Kanban.Model;
using _Scripts.Editor.Kanban.Views;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// Editor for the board's task descriptors - the list that decides which chips a card can show and
    /// what the board can be sorted by.
    ///
    /// A separate utility window rather than a panel inside the board: descriptors are defined once and
    /// then left alone for weeks, and giving that a permanent strip of the board would tax every session
    /// for a rare action.
    ///
    /// Built in C# rather than from a UXML template because the whole contents are list-driven - there is
    /// no fixed tree for a template to describe.
    /// </summary>
    public class KanbanFieldsWindow : EditorWindow
    {
        /// <summary>
        /// Serialized so the link survives a domain reload. EditorWindow references are UnityEngine.Object
        /// references, so this comes back pointing at the same board window rather than at null.
        /// </summary>
        [SerializeField] private KanbanWindow _owner;

        private VisualElement _list;

        public static void Open(KanbanWindow owner)
        {
            var window = GetWindow<KanbanFieldsWindow>(true, "Task Descriptors");
            window._owner = owner;
            window.minSize = new Vector2(360f, 240f);
            window.Rebuild();
            window.Show();
        }

        private KanbanBoard Board => _owner != null ? _owner.Board : null;

        private void OnEnable()
        {
            KanbanFontScale.Changed += ApplyFontScale;
        }

        private void OnDisable()
        {
            KanbanFontScale.Changed -= ApplyFontScale;
        }

        private void ApplyFontScale()
        {
            KanbanFontScale.Apply(rootVisualElement);
        }

        private void CreateGUI()
        {
            var styleSheet = KanbanPaths.LoadUiAsset<StyleSheet>(this, "Kanban.uss");

            if (styleSheet != null) rootVisualElement.styleSheets.Add(styleSheet);

            KanbanFontScale.Apply(rootVisualElement);

            rootVisualElement.AddToClassList("fields-window");

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1f;
            rootVisualElement.Add(scroll);

            _list = scroll.contentContainer;

            Rebuild();
        }

        private void Rebuild()
        {
            if (_list == null) return;

            _list.Clear();

            var board = Board;

            if (board == null)
            {
                // Reachable if the board window was closed while this one stayed open.
                _list.Add(new Label("The Kanban window is closed. Reopen it from Polar > Kanban Board.")
                {
                    name = "fields-hint"
                });

                return;
            }

            foreach (var def in board.FieldDefs) _list.Add(BuildDefCard(board, def));

            var add = new Button(AddField) { text = "+ Add Descriptor" };
            add.AddToClassList("fields-window__add");
            _list.Add(add);

            var hint = new Label(
                "Descriptors appear as chips on every card, and the board can be ordered by any of them " +
                "from the Sort menu. For a Select, the order of the options is the sort order.");

            hint.AddToClassList("fields-window__hint");
            _list.Add(hint);
        }

        // ------------------------------------------------------------------ descriptor rows

        private VisualElement BuildDefCard(KanbanBoard board, KanbanFieldDef def)
        {
            var card = new VisualElement();
            card.AddToClassList("field-def");

            card.Add(BuildDefHeader(board, def));

            if (def.Kind == KanbanFieldKind.Select) card.Add(BuildOptionsBlock(def));

            return card;
        }

        private VisualElement BuildDefHeader(KanbanBoard board, KanbanFieldDef def)
        {
            var header = new VisualElement();
            header.AddToClassList("field-def__header");

            var name = new TextField();
            name.AddToClassList("field-def__name");

            // Renaming edits the display name only - Key is the identity every task value is stored
            // against, so changing it here would orphan every value on the board.
            KanbanFieldBinder.BindText(
                name,
                _owner,
                "Rename Descriptor",
                () => def.DisplayName,
                value => def.DisplayName = value);

            // The rename shows up on every chip and in the sort menu, but refreshing the whole board on
            // each keystroke would rebuild every card in the project's backlog to repaint one label.
            // Waiting for the field to lose focus costs nothing and keeps typing smooth.
            name.RegisterCallback<FocusOutEvent>(_ => _owner.NotifyFieldDefsChanged());

            header.Add(name);

            var kind = new Button { text = def.Kind.ToString(), tooltip = "Descriptor type" };
            kind.AddToClassList("field-def__kind");
            kind.clicked += () => ShowKindMenu(def);
            header.Add(kind);

            var index = board.FieldDefs.IndexOf(def);

            header.Add(IconButton("▲", "Move up", index > 0, () => MoveField(index, index - 1)));
            header.Add(IconButton("▼", "Move down", index < board.FieldDefs.Count - 1, () => MoveField(index, index + 1)));
            header.Add(IconButton("✖", "Delete descriptor", true, () => DeleteField(board, def)));

            return header;
        }

        private VisualElement BuildOptionsBlock(KanbanFieldDef def)
        {
            var block = new VisualElement();
            block.AddToClassList("field-def__options");

            for (var index = 0; index < def.Options.Count; index++)
            {
                block.Add(BuildOptionRow(def, def.Options[index], index));
            }

            var add = new Button(() => AddOption(def)) { text = "+ Option" };
            add.AddToClassList("field-def__add-option");
            block.Add(add);

            return block;
        }

        private VisualElement BuildOptionRow(KanbanFieldDef def, KanbanFieldOption option, int index)
        {
            var row = new VisualElement();
            row.AddToClassList("option-row");

            var color = new ColorField { showAlpha = false, tooltip = "Chip colour" };
            color.AddToClassList("option-row__color");
            color.SetValueWithoutNotify(option.ResolveColor());

            color.RegisterValueChangedCallback(evt =>
            {
                _owner.RecordUndo("Set Option Colour");
                option.SetColor(evt.newValue);
                _owner.MarkDirty();
                _owner.NotifyFieldDefsChanged();
            });

            row.Add(color);

            var value = new TextField();
            value.AddToClassList("option-row__value");

            // Renaming an option does NOT migrate task values - they match by text, so a card holding the
            // old text simply stops matching and sorts as unknown. Delete-and-re-pick is the honest fix,
            // and pretending otherwise would need a rename-migration pass this tool does not have.
            KanbanFieldBinder.BindText(
                value,
                _owner,
                "Rename Option",
                () => option.Value,
                newValue => option.Value = newValue);

            // Deferred to focus-out for the same reason as the descriptor rename above.
            value.RegisterCallback<FocusOutEvent>(_ => _owner.NotifyFieldDefsChanged());

            row.Add(value);

            row.Add(IconButton("▲", "Move up (raises sort rank)", index > 0, () => MoveOption(def, index, index - 1)));
            row.Add(IconButton("▼", "Move down (lowers sort rank)", index < def.Options.Count - 1, () => MoveOption(def, index, index + 1)));
            row.Add(IconButton("✖", "Delete option", true, () => DeleteOption(def, option)));

            return row;
        }

        private static Button IconButton(string glyph, string tooltip, bool enabled, Action action)
        {
            var button = new Button(action) { text = glyph, tooltip = tooltip };
            button.AddToClassList("icon-button");
            button.SetEnabled(enabled);

            return button;
        }

        // ------------------------------------------------------------------ mutations

        private void ShowKindMenu(KanbanFieldDef def)
        {
            var menu = new GenericMenu();

            foreach (KanbanFieldKind kind in Enum.GetValues(typeof(KanbanFieldKind)))
            {
                var captured = kind;

                menu.AddItem(
                    new GUIContent(captured.ToString()),
                    def.Kind == captured,
                    () => SetKind(def, captured));
            }

            menu.ShowAsContext();
        }

        private void SetKind(KanbanFieldDef def, KanbanFieldKind kind)
        {
            if (def.Kind == kind) return;

            _owner.RecordUndo("Change Descriptor Type");

            // Options and existing task values are left in place when switching away from Select, so
            // switching back restores the descriptor exactly rather than silently emptying the board.
            def.Kind = kind;

            Commit();
        }

        private void AddField()
        {
            var board = Board;

            if (board == null) return;

            _owner.RecordUndo("Add Descriptor");

            var def = board.AddFieldDef("New Descriptor", KanbanFieldKind.Select);
            def.AddOption("Option 1");

            Commit();
        }

        private void DeleteField(KanbanBoard board, KanbanFieldDef def)
        {
            var affected = CountTasksWith(board, def.Key);

            if (affected > 0)
            {
                var confirmed = EditorUtility.DisplayDialog(
                    "Delete Descriptor",
                    $"Delete \"{def.Label}\"?\n\n{affected} task(s) currently hold a value for it, and those " +
                    "values will be removed too.\n\nThis can be undone with Ctrl+Z in the board window.",
                    "Delete",
                    "Cancel");

                if (!confirmed) return;
            }

            _owner.RecordUndo("Delete Descriptor");
            board.RemoveFieldDef(def);

            Commit();
        }

        private void MoveField(int from, int to)
        {
            _owner.RecordUndo("Reorder Descriptors");
            Board.MoveFieldDef(from, to);

            Commit();
        }

        private void AddOption(KanbanFieldDef def)
        {
            _owner.RecordUndo("Add Option");
            def.AddOption(UniqueOptionValue(def));

            Commit();
        }

        private void MoveOption(KanbanFieldDef def, int from, int to)
        {
            // Reordering options reorders the board itself when this descriptor drives the sort - option
            // position is the sort rank.
            _owner.RecordUndo("Reorder Options");
            def.MoveOption(from, to);

            Commit();
        }

        private void DeleteOption(KanbanFieldDef def, KanbanFieldOption option)
        {
            _owner.RecordUndo("Delete Option");
            def.Options.Remove(option);

            // Task values naming this option are deliberately left alone: the value is still meaningful
            // text, it just no longer matches an option, so it sorts as unknown and shows an uncoloured
            // chip. Re-adding the option makes every one of those cards light up again.
            Commit();
        }

        private static string UniqueOptionValue(KanbanFieldDef def)
        {
            var suffix = def.Options.Count + 1;

            while (def.FindOption($"Option {suffix}") != null) suffix++;

            return $"Option {suffix}";
        }

        private static int CountTasksWith(KanbanBoard board, string key)
        {
            var count = 0;

            foreach (var column in board.Columns)
            {
                foreach (var task in column.Tasks)
                {
                    if (task.HasField(key)) count++;
                }
            }

            return count;
        }

        /// <summary>Persist, refresh the board window, and redraw this window's list.</summary>
        private void Commit()
        {
            _owner.MarkDirty();
            _owner.NotifyFieldDefsChanged();

            Rebuild();
        }
    }
}
