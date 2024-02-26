using System;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;

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

        private Vector3 _mousePos;
        private PolarNode _currentNode;

        private void Awake()
        {
            instance = this;

            mouseMarker = transform.GetChild(0);
        }

        private void OnDrawGizmos()
        {
            if (_mousePos == Vector3.zero || _currentNode == null)
            {
                return;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_mousePos, _currentNode.transform.position);
            
        }

        private void LateUpdate()
        {
            if (!follow || !polarGridManager.Initalised)
            {
                return;
            }
            
            _mousePos = GetPosition();
            mouseMarker.position = _mousePos;
            
            var purePolar = polarGridManager.GetPurePolarFromWorld(_mousePos);
            
            //Debug.Log($"Mouse pos: {_mousePos}, Polar: {purePolar}");
            
            var polarPos = polarGridManager.GetPolarFromWorld(_mousePos);
            var node = polarGridManager.GetPolarNode(polarPos);
            if (node == null || _currentNode == node)
            {
                return;
            }

            _currentNode = node;
            
            //Debug.Log($"Node: {node}, NodePolar: {node.PolarGridPosition}");
        }

        public static Vector3 GetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var raycastHit, float.MaxValue, instance.mousePlaneLayerMask))
            {
                return raycastHit.point;
            }

            return new Vector3();
        }
    }
}