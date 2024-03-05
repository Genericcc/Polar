using System.Collections;
using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;
using _Scripts._Game.UIs;
using _Scripts.Zenject.Signals;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public class StructurePlacementHandler : IPlacementHandler
    {
        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;
        private MouseWorld _mouseWorld;

        [Inject]
        public void Construct(SignalBus signalBus,
            PolarGridManager polarGridManager,
            MouseWorld mouseWorld)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _mouseWorld = mouseWorld;
        }

        private bool IsMouseOverUI()
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (var rayResult in results)
            {
                if (rayResult.gameObject.GetComponent<UIMarker>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator _WaitForInput(
            InputReader inputReader, IStructureData structureData, IPlacementValidator placementValidator)
        {
            while (true)
            {
                if (inputReader.WasCancelPressed)
                {
                    break;
                }

                var isOverUI = IsMouseOverUI();

                if (isOverUI)
                {
                    Debug.Log("Over UI");
                    yield return 0f;
                    continue;
                }
                else
                {
                    Debug.Log("Not Over UI");
                }

                if (!inputReader.WasMouseClicked)
                {
                    yield return 0f;
                    continue;
                }
                
                var node = _polarGridManager.GetPolarNode(_mouseWorld.MousePos);
                if (node == null)
                {
                    yield return 0f;
                    continue;
                }

                if (!_polarGridManager.TryGetNodesForStructure(node, structureData.StructureSizeType, out var nodesToBuildOn))
                {
                    yield return 0f;
                    continue;
                }

                if (!placementValidator.Validate(nodesToBuildOn, structureData))
                {
                    yield return 0f;
                    continue;
                }

                var newTransform = GetBuildTransform(nodesToBuildOn, structureData);
            
                _signalBus.Fire(new RequestStructurePlacementSignal(nodesToBuildOn, structureData, newTransform));
            }
        }

        public LocalTransform GetBuildTransform(List<PolarNode> polarNodes, IStructureData structureData)
        {
            var newPos = new Vector3();

            foreach (var polarNode in polarNodes)
            {
                newPos.x += polarNode.CentrePosition.x;
                newPos.z += polarNode.CentrePosition.z;
            }

            //newPos = new Vector3(newPos.x / polarNodes.Count, newPos.y / polarNodes.Count, newPos.z / polarNodes.Count);
            newPos *= 1f / polarNodes.Count;

            newPos.y = polarNodes[0].CentrePosition.y;

            var buildTransform = LocalTransform.FromPositionRotationScale
                (math.float3(newPos),
                Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0)),
                structureData.Scale);

            return buildTransform;
        }
    }
}