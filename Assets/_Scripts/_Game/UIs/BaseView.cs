using UnityEngine;

namespace _Scripts._Game.UIs
{
    [RequireComponent(typeof(UIMarker))]
    public class BaseView : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}