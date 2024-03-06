using System.Collections;
using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Zenject.Signals;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public class RoadPlacementHandler : IPlacementHandler
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

        public IEnumerator _WaitForInput(
            InputReader inputReader, IStructureData structureData, IPlacementValidator roadValidator)
        {
            var anchorNodes = new List<PolarNode>();

            while (true)
            {
                yield return 0f;
                
                if (inputReader.WasCancelPressed)
                {
                    if (anchorNodes.Count == 1)
                    {
                        anchorNodes.Clear();
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (!inputReader.WasMouseClicked || _mouseWorld.IsMouseOverUI())
                {
                    continue;
                }

                var node = _polarGridManager.GetPolarNode(_mouseWorld.MousePos);
                if (node == null)
                {
                    continue;
                }

                if (anchorNodes.Contains(node))
                {
                    Debug.Log("Node already selected for road building");

                    continue;
                }

                anchorNodes.Add(node);

                if (!roadValidator.Validate(anchorNodes, structureData))
                {
                    anchorNodes.RemoveAt(anchorNodes.Count - 1);

                    continue;
                }

                if (anchorNodes.Count < 2)
                {
                    Debug.Log($"Waiting for inputs... Current count is: startEndNodes.Count");

                    continue;
                }

                //TODO try to connect start and end build nodes? Pathfinding? 
                // if (!_polarGridManager.TryGetNodesForStructure(node, structureData.StructureSizeType, out var nodesToBuildOn))
                // {
                //     yield return 0f;
                //     continue;
                // }

                
                //TODO refactor to build road BETWEEN TWO nodes, not on each
                foreach (var anchor in anchorNodes)
                {
                    var singleNodeList = new List<PolarNode> { anchor };
                    
                    var newTransform = GetBuildTransform(singleNodeList, structureData);

                    _signalBus.Fire(new RequestStructurePlacementSignal(singleNodeList, structureData, newTransform));
                }

                var lastAnchor = anchorNodes[^1];
                anchorNodes.Clear();
                anchorNodes.Add(lastAnchor);
            }
        }

        public LocalTransform GetBuildTransform(List<PolarNode> polarNodes, IStructureData structureData)
        {
            var newPos = new Vector3();

            foreach (var polarNode in polarNodes)
            {
                newPos.x += polarNode.WorldPosition.x;
                newPos.z += polarNode.WorldPosition.z;
            }

            //newPos = new Vector3(newPos.x / polarNodes.Count, newPos.y / polarNodes.Count, newPos.z / polarNodes.Count);
            newPos *= 1f / polarNodes.Count;

            newPos.y = polarNodes[0].WorldPosition.y;

            var buildTransform = LocalTransform.FromPositionRotationScale(
                math.float3(newPos),
                Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0)),
                structureData.Scale);

            return buildTransform;
        }
    }
}