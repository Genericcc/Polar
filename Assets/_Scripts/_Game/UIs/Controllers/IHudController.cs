using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public interface IHudController
    {
        void Bind(VisualElement root);
        void Dispose();
    }
}
