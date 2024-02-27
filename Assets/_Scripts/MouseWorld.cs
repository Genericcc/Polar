using System;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;

using UnityEngine;

namespace _Scripts
{
    public class MouseWorld : MonoBehaviour
    {
        public static MouseWorld Instance;
        [SerializeField] private LayerMask mousePlaneLayerMask;
        
        [SerializeField]
        private bool follow;

        [SerializeField]
        private Transform mouseMarker;

        [SerializeField]
        private PolarGridManager polarGridManager;

        private Vector3 _mousePos;
        
        public PolarNode CurrentNode { get; private set; }

        private void Awake()
        {
            Instance = this;

            mouseMarker = transform.GetChild(0);
        }

        private void OnDrawGizmos()
        {
            if (_mousePos == Vector3.zero || CurrentNode == null)
            {
                return;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_mousePos, CurrentNode.transform.position);
        }

        private void LateUpdate()
        {
            if (!follow || !polarGridManager.Initalised)
            {
                return;
            }
            
            _mousePos = GetPosition();
            mouseMarker.position = _mousePos;
            
            var polarPos = polarGridManager.GetPolarFromWorld(_mousePos);
            var node = polarGridManager.GetPolarNode(polarPos);
            if (node == null || CurrentNode == node)
            {
                return;
            }

            CurrentNode = node;
        }

        public static Vector3 GetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var raycastHit, float.MaxValue, Instance.mousePlaneLayerMask))
            {
                return raycastHit.point;
            }

            return new Vector3();
        }
    }
}