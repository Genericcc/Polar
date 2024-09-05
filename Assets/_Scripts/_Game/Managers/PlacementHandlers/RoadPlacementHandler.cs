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

                // The node is first added to the list as the Validator validates a whole list (check if it's necessary)
                anchorNodes.Add(node);

                if (!roadValidator.Validate(anchorNodes, structureData))
                {
                    //Remove the new node if the list failed to pass validation
                    anchorNodes.RemoveAt(anchorNodes.Count - 1);

                    continue;
                }

                if (anchorNodes.Count < 2)
                {
                    Debug.Log($"Waiting for inputs... Current count is: startEndNodes.Count");

                    continue;
                }
                
                var startNode = anchorNodes.First();
                var endNode = anchorNodes.Last();

                var path = _pathfinder.FindPath(startNode, endNode).ToArray();
                
                for (var i = 0; i < path.Length - 1; i++)
                {
                    var connectedNodes = new List<PolarNode> { path[i], path[i + 1] };
                
                    var newTransform = GetBuildTransform(connectedNodes, structureData);
                
                    _signalBus.Fire(
                        new RequestStructurePlacementSignal(
                            new List<PolarNode> { path[i] },
                            structureData,
                            newTransform));
                }

                var lastAnchor = path[^1];
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

            newPos.y = polarNodes[0].CentrePosition.y;

            var buildTransform = LocalTransform.FromPositionRotationScale(
                math.float3(newPos),
                Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0)),
                structureData.Scale);

            return buildTransform;
        }
    }
}