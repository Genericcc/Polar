using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Editor.Kanban.Model;
using _Scripts.Editor.Kanban.Views;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// A Kanban board that lives in the Editor, so planning sits next to the code instead of in a
    /// browser tab. Columns hold task cards; cards drag between columns; everything persists as JSON
    /// in ProjectSettings (see <see cref="KanbanStore"/>).
    ///
    /// Built on UI Toolkit rather than IMGUI: the board is a flexbox row of scrolling lanes with inline
    /// text fields and a floating drag ghost, all of which IMGUI would turn into manual Rect arithmetic.
    /// </summary>
    public class KanbanWindow : EditorWindow, IKanbanHost
    {
        private const string ExpandedStateKey = "Polar.Kanban.ExpandedTasks";
        private const long SaveDebounceMs = 750;

        /// <summary>Larger than any realistic content size; ScrollView clamps it to the true extent.</summary>
        internal const float ScrollToEnd = 100000f;

        private readonly List<ColumnView> _columnViews = new();
        private readonly HashSet<string> _expandedTaskIds = new();

        private KanbanBoardHolder _holder;
        private VisualTreeAsset _columnTemplate;
        private VisualTreeAsset _cardTemplate;

        private VisualElement _columnsContainer;
        private ScrollView _boardScroll;
        private Label _dirtyIndicator;
        private VisualElement _conflictBar;

        private IVisualElementScheduledItem _saveScheduler;
        private bool _dirty;
        private DateTime _lastKnownWriteUtc;

        [MenuItem(PolarAssetMenu.Root + "Kanban Board")]
        private static void Open()
        {
            GetWindow<KanbanWindow>("Kanban");
        }

        // ------------------------------------------------------------------ IKanbanHost

        public KanbanBoard Board => _holder != null ? _holder.Board : null;

        public IReadOnlyList<ColumnView> ColumnViews => _columnViews;

        public VisualElement DragLayer => rootVisualElement;

        public ScrollView BoardScroll => _boardScroll;

        public void RecordUndo(string label)
        {
            if (_holder == null) return;

            // RegisterCompleteObjectUndo rather than RecordObject: list reordering is a structural change
            // to the serialized graph, which the diffing variant does not always capture.
            Undo.RegisterCompleteObjectUndo(_holder, label);
        }

        public void MarkDirty()
        {
            _dirty = true;
            UpdateDirtyIndicator();
            _saveScheduler?.ExecuteLater(SaveDebounceMs);
        }

        public void RebuildBoard()
        {
            if (_columnsContainer == null || Board == null) return;

            _columnViews.Clear();
            _columnsContainer.Clear();

            foreach (var column in Board.Columns)
            {
                var view = new ColumnView(this, column, _columnTemplate, _cardTemplate);
                _columnViews.Add(view);
                _columnsContainer.Add(view.Root);
            }
        }

        public void RebuildColumn(KanbanColumn column)
        {
            var view = _columnViews.FirstOrDefault(candidate => candidate.Column == column);

            if (view == null)
            {
                RebuildBoard();
                return;
            }

            view.RebuildTasks();
        }

        public bool IsExpanded(string taskId)
        {
            return !string.IsNullOrEmpty(taskId) && _expandedTaskIds.Contains(taskId);
        }

        public void SetExpanded(string taskId, bool expanded)
        {
            if (string.IsNullOrEmpty(taskId)) return;

            var changed = expanded ? _expandedTaskIds.Add(taskId) : _expandedTaskIds.Remove(taskId);

            if (changed) SaveExpandedState();
        }

        // ------------------------------------------------------------------ lifecycle

        private void OnEnable()
        {
            minSize = new Vector2(420f, 260f);

            LoadExpandedState();
            LoadBoard();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            FlushSave();

            if (_holder != null)
            {
                // Drop the undo entries first - they reference the holder, and leaving them pointing at
                // a destroyed object makes the undo stack complain later.
                Undo.ClearUndo(_holder);
                DestroyImmediate(_holder);
                _holder = null;
            }
        }

        private void OnFocus()
        {
            CheckForExternalChanges();
        }

        private void CreateGUI()
        {
            var windowTemplate = KanbanPaths.LoadUiAsset<VisualTreeAsset>(this, "KanbanWindow.uxml");
            var styleSheet = KanbanPaths.LoadUiAsset<StyleSheet>(this, "Kanban.uss");

            _columnTemplate = KanbanPaths.LoadUiAsset<VisualTreeAsset>(this, "ColumnView.uxml");
            _cardTemplate = KanbanPaths.LoadUiAsset<VisualTreeAsset>(this, "TaskCard.uxml");

            if (windowTemplate == null || _columnTemplate == null || _cardTemplate == null) return;

            if (styleSheet != null) rootVisualElement.styleSheets.Add(styleSheet);

            windowTemplate.CloneTree(rootVisualElement);

            _boardScroll = rootVisualElement.Q<ScrollView>("columns-scroll");
            _boardScroll.mode = ScrollViewMode.Horizontal;
            _columnsContainer = rootVisualElement.Q<VisualElement>("columns");
            _dirtyIndicator = rootVisualElement.Q<Label>("dirty-indicator");
            _conflictBar = rootVisualElement.Q<VisualElement>("conflict-bar");

            BindToolbar();
            BindConflictBar();

            _saveScheduler = rootVisualElement.schedule.Execute(FlushSave);

            UpdateDirtyIndicator();
            RebuildBoard();
        }

        private void BindToolbar()
        {
            KanbanFieldBinder.BindText(
                rootVisualElement.Q<TextField>("board-title"),
                this,
                "Rename Board",
                () => Board?.Title,
                value =>
                {
                    if (Board != null) Board.Title = value;
                });

            rootVisualElement.Q<Button>("add-column").clicked += AddColumn;
            rootVisualElement.Q<Button>("reload").clicked += () => ReloadFromDisk(true);
            rootVisualElement.Q<Button>("reveal").clicked += () => EditorUtility.RevealInFinder(KanbanStore.AbsoluteBoardPath);
        }

        private void BindConflictBar()
        {
            rootVisualElement.Q<Button>("conflict-reload").clicked += () =>
            {
                ReloadFromDisk(false);
                ShowConflictBar(false);
            };

            rootVisualElement.Q<Button>("conflict-keep").clicked += () =>
            {
                // Accept the on-disk timestamp so the banner stops firing, then mark dirty so this
                // window's version is the one that ends up written.
                _lastKnownWriteUtc = KanbanStore.LastWriteUtc();
                ShowConflictBar(false);
                MarkDirty();
            };
        }

        // ------------------------------------------------------------------ board state

        private void LoadBoard()
        {
            var board = KanbanStore.Load();

            if (_holder == null) _holder = KanbanBoardHolder.Create(board);
            else _holder.Board = board;

            if (!KanbanStore.Exists())
            {
                // First run - put the default board on disk straight away so the file exists and can be
                // committed, rather than appearing only after the first edit.
                KanbanStore.Save(board);
            }

            _dirty = false;
            _lastKnownWriteUtc = KanbanStore.LastWriteUtc();
        }

        private void ReloadFromDisk(bool confirmWhenDirty)
        {
            if (confirmWhenDirty && _dirty)
            {
                var discard = EditorUtility.DisplayDialog(
                    "Reload Board",
                    "This board has unsaved changes. Reloading discards them.",
                    "Reload",
                    "Cancel");

                if (!discard) return;
            }

            LoadBoard();
            UpdateDirtyIndicator();
            RebuildBoard();

            var titleField = rootVisualElement.Q<TextField>("board-title");
            titleField?.SetValueWithoutNotify(Board?.Title ?? string.Empty);
        }

        private void AddColumn()
        {
            if (Board == null) return;

            RecordUndo("Add Column");
            Board.AddColumn("New Column");
            MarkDirty();
            RebuildBoard();

            // Scroll to the new column once layout has caught up with it. ScrollView clamps the offset,
            // so any value past the content width lands on the far right.
            _boardScroll.schedule.Execute(() => _boardScroll.scrollOffset = new Vector2(ScrollToEnd, 0f));
        }

        private void OnUndoRedoPerformed()
        {
            if (_holder == null) return;

            // Undo restored an older serialized state of the holder, so every view is now bound to stale
            // model objects - rebuild wholesale, then persist the reverted board.
            RebuildBoard();

            var titleField = rootVisualElement?.Q<TextField>("board-title");
            titleField?.SetValueWithoutNotify(Board?.Title ?? string.Empty);

            MarkDirty();
        }

        // ------------------------------------------------------------------ persistence

        private void FlushSave()
        {
            if (!_dirty || Board == null) return;

            if (!KanbanStore.Save(Board)) return;

            _dirty = false;
            _lastKnownWriteUtc = KanbanStore.LastWriteUtc();
            UpdateDirtyIndicator();
        }

        private void CheckForExternalChanges()
        {
            if (_conflictBar == null || Board == null) return;

            var diskWriteUtc = KanbanStore.LastWriteUtc();

            if (diskWriteUtc <= _lastKnownWriteUtc) return;

            if (_dirty)
            {
                ShowConflictBar(true);
                return;
            }

            // Nothing local to lose (a git pull, or an edit in another editor) - just take the new file.
            ReloadFromDisk(false);
        }

        private void ShowConflictBar(bool visible)
        {
            _conflictBar?.EnableInClassList("conflict-bar--visible", visible);
        }

        private void UpdateDirtyIndicator()
        {
            _dirtyIndicator?.EnableInClassList("kanban-toolbar__dirty--hidden", !_dirty);
        }

        private void LoadExpandedState()
        {
            _expandedTaskIds.Clear();

            var raw = SessionState.GetString(ExpandedStateKey, string.Empty);

            if (string.IsNullOrEmpty(raw)) return;

            foreach (var id in raw.Split(';'))
            {
                if (!string.IsNullOrEmpty(id)) _expandedTaskIds.Add(id);
            }
        }

        private void SaveExpandedState()
        {
            // SessionState, not the board file: which descriptions are open is view state, and writing it
            // to JSON would put a git diff behind every arrow click.
            SessionState.SetString(ExpandedStateKey, string.Join(";", _expandedTaskIds));
        }
    }
}
