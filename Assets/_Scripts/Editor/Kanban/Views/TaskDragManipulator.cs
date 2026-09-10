using _Scripts.Editor.Kanban.Model;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor.Kanban.Views
{
    /// <summary>
    /// Drags a task card between (and within) columns using pointer capture.
    ///
    /// Deliberately not the IMGUI DragAndDrop API - that one exists for asset drags across windows and
    /// brings a whole generic-data protocol with it. Everything here happens inside one window, where
    /// pointer capture gives exact control over the ghost and the insertion marker.
    ///
    /// While a drag is running the original card is detached from the hierarchy and a placeholder holds
    /// its slot. That keeps every element the drag measures against present in layout - a card hidden
    /// with display:none has no world bounds to hit-test.
    ///
    /// Because of that detach, the pointer is captured on the DRAG LAYER, not on the card. A visual
    /// element loses pointer capture the moment it leaves the panel, so capturing on the card made the
    /// drag cancel itself the instant it began: BeginDrag detaches the card, capture is released,
    /// PointerCaptureOutEvent fires, and the handler reads that as "something interrupted the drag".
    /// The drag layer is the window root and never moves, so capture there survives the whole gesture.
    /// The move/up/capture-out callbacks therefore live on the drag layer too, for the duration of the
    /// gesture only - captured pointer events are routed to the capturing element, not to the card.
    /// </summary>
    public class TaskDragManipulator : PointerManipulator
    {
        private const float DragThreshold = 4f;
        private const float AutoScrollMargin = 40f;
        private const float AutoScrollSpeed = 12f;

        private readonly IKanbanHost _host;
        private readonly TaskCardView _card;

        private bool _pointerDown;
        private bool _dragging;
        private bool _finishing;
        private int _pointerId;
        private Vector2 _pointerDownPosition;
        private Vector2 _grabOffset;

        private VisualElement _ghost;
        private VisualElement _placeholder;
        private ColumnView _sourceColumn;
        private ColumnView _targetColumn;

        /// <summary>The drag layer while a gesture is in flight; null the rest of the time.</summary>
        private VisualElement _capture;

        /// <summary>
        /// Captured once at drag start rather than read per-frame: the whole gesture has to obey one set
        /// of rules, and a sort toggled from the toolbar mid-drag would otherwise change what the drop
        /// means halfway through.
        /// </summary>
        private bool _sorted;

        public TaskDragManipulator(IKanbanHost host, TaskCardView card)
        {
            _host = host;
            _card = card;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            // Only the press lives on the card. Everything after it is routed through the drag layer.
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);

            // A card removed mid-gesture must not leave its handlers behind on the shared drag layer.
            ReleaseCapture();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            if (!IsDragHandle(evt.target as VisualElement)) return;

            var layer = _host.DragLayer;

            if (layer == null) return;

            _pointerDown = true;
            _pointerId = evt.pointerId;
            _pointerDownPosition = evt.position;
            _grabOffset = _pointerDownPosition - target.worldBound.position;

            _capture = layer;
            _capture.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _capture.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _capture.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            _capture.CapturePointer(_pointerId);

            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_pointerDown) return;
            if (evt.pointerId != _pointerId) return;

            Vector2 position = evt.position;

            if (!_dragging)
            {
                if (Vector2.Distance(position, _pointerDownPosition) < DragThreshold) return;
                BeginDrag();
            }

            UpdateDrag(position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_pointerDown) return;
            if (evt.pointerId != _pointerId) return;

            var wasDragging = _dragging;

            // Release before committing: the commit rebuilds the column and destroys this card, and
            // releasing capture from a detached element is not something to rely on. _finishing keeps
            // the capture-out handler from reading the release as a cancellation.
            _finishing = true;

            if (_capture != null && _capture.HasPointerCapture(_pointerId)) _capture.ReleasePointer(_pointerId);

            _finishing = false;

            EndDrag(true);

            if (wasDragging) evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            // Fires when capture is lost for any reason the drag did not initiate (window focus change,
            // alt-tab). Only a genuine pointer-up commits; anything else puts the card back.
            if (_finishing) return;
            if (_dragging || _pointerDown) EndDrag(false);
        }

        /// <summary>Unhooks this gesture's handlers from the shared drag layer. Safe to call twice.</summary>
        private void ReleaseCapture()
        {
            if (_capture == null) return;

            _capture.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _capture.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _capture.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

            if (_capture.HasPointerCapture(_pointerId))
            {
                _finishing = true;
                _capture.ReleasePointer(_pointerId);
                _finishing = false;
            }

            _capture = null;
        }

        /// <summary>
        /// Text fields and buttons on the card keep their own click behaviour; everything else - the
        /// grip, the padding, the row background - is grabbable.
        /// </summary>
        private bool IsDragHandle(VisualElement element)
        {
            while (element != null)
            {
                if (element is TextField || element is Button) return false;
                if (element == target) return true;

                element = element.parent;
            }

            return false;
        }

        private void BeginDrag()
        {
            _dragging = true;
            _sorted = _host.Board is { SortActive: true };

            _sourceColumn = _card.Owner;
            _targetColumn = _sourceColumn;
            _targetColumn.Root.AddToClassList("column--drop-target");

            var sourceIndex = target.parent.IndexOf(target);
            var size = target.layout.size;

            _placeholder = new VisualElement();
            _placeholder.AddToClassList("card-placeholder");
            _placeholder.style.height = size.y;

            var container = target.parent;
            target.RemoveFromHierarchy();
            container.Insert(Mathf.Clamp(sourceIndex, 0, container.childCount), _placeholder);

            _ghost = BuildGhost(size.x);
            _host.DragLayer.Add(_ghost);
        }

        private VisualElement BuildGhost(float width)
        {
            var ghost = new VisualElement { pickingMode = PickingMode.Ignore };
            ghost.AddToClassList("card");
            ghost.AddToClassList("card--ghost");
            ghost.style.width = width;

            var glyph = _card.Task.Glyph;

            if (!string.IsNullOrEmpty(glyph))
            {
                var glyphLabel = new Label(glyph) { pickingMode = PickingMode.Ignore };
                glyphLabel.AddToClassList("glyph-field");
                ghost.Add(glyphLabel);
            }

            var titleLabel = new Label(_card.Task.Title) { pickingMode = PickingMode.Ignore };
            titleLabel.AddToClassList("card__title");
            ghost.Add(titleLabel);

            return ghost;
        }

        private void UpdateDrag(Vector2 position)
        {
            _ghost.style.left = position.x - _grabOffset.x;
            _ghost.style.top = position.y - _grabOffset.y;

            AutoScroll(position);

            var column = ResolveColumnUnder(position);

            if (column != null && column != _targetColumn)
            {
                _targetColumn?.Root.RemoveFromClassList("column--drop-target");
                _targetColumn = column;
                _targetColumn.Root.AddToClassList("column--drop-target");
            }

            if (_targetColumn == null) return;

            var container = _targetColumn.TasksContainer;

            // Under a sort the drop position is not the user's to choose - the comparator decides where
            // the card lands. Parking the placeholder at the end says "it joins this column" without
            // promising a slot the drop cannot honour.
            var insertIndex = _sorted
                ? container.childCount
                : ResolveInsertIndex(container, position.y);

            _placeholder.RemoveFromHierarchy();
            container.Insert(Mathf.Clamp(insertIndex, 0, container.childCount), _placeholder);
        }

        private ColumnView ResolveColumnUnder(Vector2 position)
        {
            foreach (var column in _host.ColumnViews)
            {
                if (column.Root.worldBound.Contains(position)) return column;
            }

            // Nothing under the pointer (the gap between columns, or off the board) - keep the last
            // valid target so a wobble mid-drag does not drop the card somewhere unexpected.
            return null;
        }

        private int ResolveInsertIndex(VisualElement container, float pointerY)
        {
            var index = 0;

            foreach (var child in container.Children())
            {
                if (child == _placeholder) continue;
                if (pointerY < child.worldBound.center.y) return index;

                index++;
            }

            return index;
        }

        private void AutoScroll(Vector2 position)
        {
            var boardScroll = _host.BoardScroll;
            var boardBounds = boardScroll.worldBound;
            var boardOffset = boardScroll.scrollOffset;

            if (position.x > boardBounds.xMax - AutoScrollMargin) boardOffset.x += AutoScrollSpeed;
            else if (position.x < boardBounds.xMin + AutoScrollMargin) boardOffset.x -= AutoScrollSpeed;

            boardScroll.scrollOffset = boardOffset;

            if (_targetColumn == null) return;

            var columnScroll = _targetColumn.Scroll;
            var columnBounds = columnScroll.worldBound;
            var columnOffset = columnScroll.scrollOffset;

            if (position.y > columnBounds.yMax - AutoScrollMargin) columnOffset.y += AutoScrollSpeed;
            else if (position.y < columnBounds.yMin + AutoScrollMargin) columnOffset.y -= AutoScrollSpeed;

            columnScroll.scrollOffset = columnOffset;
        }

        private void EndDrag(bool commit)
        {
            _pointerDown = false;

            // Every exit runs through here - a committed drop, a cancellation, or a press that never
            // passed the threshold - so this is the one place the drag layer has to be cleaned up.
            ReleaseCapture();

            if (!_dragging)
            {
                return;
            }

            _dragging = false;

            var dropIndex = _placeholder.parent != null ? _placeholder.parent.IndexOf(_placeholder) : -1;

            _ghost?.RemoveFromHierarchy();
            _placeholder?.RemoveFromHierarchy();
            _targetColumn?.Root.RemoveFromClassList("column--drop-target");

            _ghost = null;
            _placeholder = null;

            var source = _sourceColumn;
            var destination = _targetColumn;

            _sourceColumn = null;
            _targetColumn = null;

            if (!commit || destination == null || dropIndex < 0)
            {
                // Cancelled: the card was detached from the hierarchy, so rebuild to put it back.
                if (source != null) ScheduleRebuild(source.Column, null);
                return;
            }

            MoveTask(source, destination, dropIndex);
        }

        private void MoveTask(ColumnView source, ColumnView destination, int dropIndex)
        {
            var task = _card.Task;

            if (_sorted)
            {
                // Reordering within a column is meaningless while the comparator owns the order, so a
                // same-column drop is a no-op rather than a silent nothing-happened. Moving a card to a
                // different column still means something - it re-homes the task - so that stays allowed.
                if (destination == source)
                {
                    ScheduleRebuild(source.Column, null);
                    return;
                }

                _host.RecordUndo("Move Task");
                source.Column.Tasks.Remove(task);

                // Appended, not inserted: the manual order is what the board falls back to when sorting
                // is switched off, and the end of the list is the honest answer for "arrived just now".
                destination.Column.Tasks.Add(task);

                _host.MarkDirty();
                ScheduleRebuild(source.Column, destination.Column);
                return;
            }

            _host.RecordUndo("Move Task");

            source.Column.Tasks.Remove(task);

            // The dragged card left the hierarchy when the drag began, so the placeholder's index was
            // already measured against a list without it - which is exactly the list shape the model is
            // in after the Remove above. That holds for same-column reorders and cross-column drops alike.
            var clamped = Mathf.Clamp(dropIndex, 0, destination.Column.Tasks.Count);
            destination.Column.Tasks.Insert(clamped, task);

            _host.MarkDirty();

            ScheduleRebuild(source.Column, destination.Column);
        }

        /// <summary>
        /// Rebuilding destroys the very card this manipulator is attached to, so it waits until the
        /// current event has finished dispatching rather than pulling the element out mid-flight.
        /// </summary>
        private void ScheduleRebuild(KanbanColumn first, KanbanColumn second)
        {
            _host.DragLayer.schedule.Execute(() =>
            {
                if (first != null) _host.RebuildColumn(first);
                if (second != null && second != first) _host.RebuildColumn(second);
            });
        }
    }
}
