using UnityEngine;

namespace _Scripts._Game.UIs
{
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