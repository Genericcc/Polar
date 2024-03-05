using System;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;

using UnityEngine;

using Zenject;

namespace _Scripts
{
    public class MouseWorld : MonoBehaviour
    {
        [SerializeField]
        private LayerMask mousePlaneLayerMask;
        
        [SerializeField]
        private bool follow;

        [SerializeField]
        private Transform mouseMarker;

        private InputReader _inputReader;

        public Vector3 MousePos { get; private set; }

        [Inject]
        public void Construct(InputReader inputReader)
        {
            _inputReader = inputReader;
        }

        private void Awake()
        {
            mouseMarker = transform.GetChild(0);
        }

        // private void OnDrawGizmos()
        // {
        //     if (MousePos == Vector3.zero || CurrentNode == null)
        //     {
        //         return;
        //     }
        //     
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawLine(MousePos, CurrentNode.transform.position);
        // }

        private void LateUpdate()
        {
            if (!follow)
            {
                return;
            }
            
            MousePos = GetPosition();
            mouseMarker.position = MousePos;
        }

        private Vector3 GetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(_inputReader.PointerPosition);

            if (Physics.Raycast(ray, out var raycastHit, float.MaxValue, mousePlaneLayerMask))
            {
                return raycastHit.point;
            }

            return new Vector3();
        }
    }
}