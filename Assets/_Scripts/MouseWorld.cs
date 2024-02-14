using UnityEngine;

namespace _Scripts
{
    public class MouseWorld : MonoBehaviour
    {

        private static MouseWorld instance;


        [SerializeField] private LayerMask mousePlaneLayerMask;

        private void Awake()
        {
            instance = this;
        }

        public static Vector3 GetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var raycastHit, float.MaxValue, instance.mousePlaneLayerMask);
            return raycastHit.point;
        }

    }
}