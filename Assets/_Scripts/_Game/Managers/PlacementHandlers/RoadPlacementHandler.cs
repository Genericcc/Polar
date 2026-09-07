using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Grid.Pathfinders;
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
        private Pathfinder _pathfinder;

        [Inject]
        public void Construct(SignalBus signalBus,
            PolarGridManager polarGridManager,
            MouseWorld mouseWorld,
            Pathfinder pathfinder)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _mouseWorld = mouseWorld;
            _pathfinder = pathfinder;
        }

        public IEnumerator TryPlace(
            InputReader inputReader,
            IStructureData structureData,
            IPlacementValidator roadValidator)
        {
            //End is useless now, but kept to avoid typing...
            (PolarNode start, PolarNode end) anchorNodes = new (null, null);

            while (true)
            {
                yield return 0f;

                if (inputReader.WasCancelPressed)
                {
                    if (anchorNodes.start is not null)
                    {
                        anchorNodes.start = null;
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
                if (node is null)
                {
                    continue;
                }

                if (anchorNodes.start == node)
                {
                    Debug.Log("Node already selected for road building");
                    continue;
                }
                
                if (anchorNodes.start is null)
                {
                    anchorNodes.start = node;
                    continue;
                }
                
                //If we got here, the node is the end node
                var path = _pathfinder.FindPath(anchorNodes.start, node);
                if (!roadValidator.Validate(path, structureData))
                {
                    Debug.Log($"Path not valid");
                    continue;
                }
                
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var newTransform = GetRoadTransform(path[i], path[i + 1], structureData);
                    _signalBus.Fire(new RequestStructurePlacementSignal(new List<PolarNode> { path[i] }, structureData, newTransform));
                }

                //Continue at the road end
                anchorNodes.start = node;
            }
        }

        public LocalTransform GetRoadTransform(PolarNode backNode, PolarNode frontNode, IStructureData structureData)
        {
            var newPos = new Vector3
            {
                x = backNode.WorldPosition.x + frontNode.WorldPosition.x,
                y = backNode.CentrePosition.y + frontNode.CentrePosition.y,
                z = backNode.WorldPosition.z + frontNode.WorldPosition.z
            };
            newPos *= 0.5f;

            var rotation = Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0));
            if (backNode.PolarGridPosition.D != frontNode.PolarGridPosition.D)
            {
                rotation *= Quaternion.AngleAxis(90, Vector3.up);
            }

            var buildTransform = LocalTransform.FromPositionRotationScale(
                math.float3(newPos),
                rotation,
                structureData.Scale);

            return buildTransform;
        }
    }
}