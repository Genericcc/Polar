using System;
using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts._Game.UIs;

using UnityEngine;
using UnityEngine.EventSystems;

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
        private PolarGridManager _polarGridManager;
        private List<PolarNode> _currentNodes;

        public Vector3 MousePos { get; private set; }

        [Inject]
        public void Construct(InputReader inputReader, PolarGridManager polarGridManager)
        {
            _inputReader = inputReader;
            _polarGridManager = polarGridManager;
        }

        private void Awake()
        {
            mouseMarker = transform.GetChild(0);
        }

        public bool IsMouseOverUI()
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (var rayResult in results)
            {
                if (rayResult.gameObject.GetComponent<UIMarker>() is not null)
                {
                    return true;
                }
            }

            return false;
        }

        // Old debugging that connected mouseMarker to node under the mouse
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

            var node = _polarGridManager.GetPolarNode(MousePos);
            if (node is null)
            {
                _currentNodes.ForEach(x => x.Highlight(false));
                _currentNodes.Clear();
                return;
            }
            
            _polarGridManager.TryGetNodesForStructure(node, StructureSizeType.Size2X2, out var buildNodes);
            if (buildNodes.Count > 0)
            {
                _currentNodes.ForEach(x => x.Highlight(false));
                _currentNodes.Clear();
                _currentNodes.AddRange(buildNodes);
                _currentNodes.ForEach(x => x.Highlight(true));
            }
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