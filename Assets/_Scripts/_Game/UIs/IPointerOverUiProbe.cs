using UnityEngine;

namespace _Scripts._Game.UIs
{
    public interface IPointerOverUiProbe
    {
        bool IsPointerOverUI(Vector2 screenPosition);
    }
}
