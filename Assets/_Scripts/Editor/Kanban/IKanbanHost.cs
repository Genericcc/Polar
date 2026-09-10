using System.Collections.Generic;

using _Scripts.Editor.Kanban.Model;
using _Scripts.Editor.Kanban.Views;

using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// What the column/card views are allowed to ask of the window. Keeping this narrow means the views
    /// never touch Undo, disk IO or the EditorWindow lifecycle directly - they mutate the model and say
    /// what changed.
    /// </summary>
    public interface IKanbanHost
    {
        KanbanBoard Board { get; }

        /// <summary>Snapshot the board for Ctrl+Z. Call immediately BEFORE mutating.</summary>
        void RecordUndo(string label);

        /// <summary>Board changed - schedule a debounced write to disk.</summary>
        void MarkDirty();

        void RebuildBoard();

        /// <summary>Rebuilds one column's cards, leaving the rest of the board (and its focus) alone.</summary>
        void RebuildColumn(KanbanColumn column);

        bool IsExpanded(string taskId);
        void SetExpanded(string taskId, bool expanded);

        IReadOnlyList<ColumnView> ColumnViews { get; }

        /// <summary>Overlay the drag ghost is parented to, so it can cross column boundaries unclipped.</summary>
        VisualElement DragLayer { get; }

        /// <summary>The horizontal board scroller, for edge auto-scrolling during a drag.</summary>
        ScrollView BoardScroll { get; }
    }
}
