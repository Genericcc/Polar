using System;

using _Scripts.Managers;

using UnityEngine;

namespace _Scripts
{
    public class MouseWorld : MonoBehaviour
    {
        private static MouseWorld instance;
        [SerializeField] private LayerMask mousePlaneLayerMask;
        
        [SerializeField]
        private bool follow;

        [SerializeField]
        private Transform mouseMarker;

        [SerializeField]
        private PolarGridManager polarGridManager;

        private void Awake()
        {
            instance = this;

            mouseMarker = transform.GetChild(0);
        }

        private void LateUpdate()
        {
            if (!follow || !polarGridManager.Initalised)
            {
                return;
            }
            
            mouseMarker.position = GetPosition();
            
            var worldPos = GetPosition();
            var polarPos = polarGridManager.GetPolarFromWorld(worldPos);
            
            Debug.Log(polarPos);
        }

        public static Vector3 GetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var raycastHit, float.MaxValue, instance.mousePlaneLayerMask);
            return raycastHit.point;
        }
        
        

    }
}