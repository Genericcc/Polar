using System;
using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts._Game.UIs;

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

        private readonly List<PolarNode> _currentNodes = new();
        
        private InputReader _inputReader;
        private PolarGridManager _polarGridManager;
        private IPointerOverUiProbe _pointerOverUiProbe;

        public Vector3 MousePos { get; private set; }

        [Inject]
        public void Construct(InputReader inputReader, PolarGridManager polarGridManager, IPointerOverUiProbe pointerOverUiProbe)
        {
            _inputReader = inputReader;
            _polarGridManager = polarGridManager;
            _pointerOverUiProbe = pointerOverUiProbe;
        }

        private void Awake()
        {
            mouseMarker = transform.GetChild(0);
        }

        public bool IsMouseOverUI()
        {
            return _pointerOverUiProbe.IsPointerOverUI(_inputReader.PointerPosition);
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