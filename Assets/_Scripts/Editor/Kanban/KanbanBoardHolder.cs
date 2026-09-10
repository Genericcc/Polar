using _Scripts.Editor.Kanban.Model;

using UnityEngine;

namespace _Scripts.Editor.Kanban
{
    /// <summary>
    /// In-memory ScriptableObject wrapper around <see cref="KanbanBoard"/>. The board itself is a
    /// plain object graph persisted as JSON, but Unity's Undo system only records UnityEngine.Objects -
    /// so every mutation records this holder and Ctrl+Z works natively instead of via a hand-rolled
    /// snapshot stack.
    ///
    /// Never written to disk (HideFlags.DontSave). A domain reload therefore drops the undo history;
    /// the window reloads the board from disk in OnEnable, so nothing is lost but the history itself.
    /// </summary>
    public class KanbanBoardHolder : ScriptableObject
    {
        public KanbanBoard Board = new();

        public static KanbanBoardHolder Create(KanbanBoard board)
        {
            var holder = CreateInstance<KanbanBoardHolder>();
            holder.hideFlags = HideFlags.DontSave;
            holder.name = "Kanban Board (session)";
            holder.Board = board;
            return holder;
        }
    }
}
