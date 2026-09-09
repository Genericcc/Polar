using System.Collections.Generic;

using _Scripts._Game.Managers;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace _Scripts._Game.Grid.Pathfinders
{
    public class Pathfinder
    {
        private readonly PolarGridManager _gridManager;
        
        public Pathfinder(PolarGridManager gridManager)
        {
            _gridManager = gridManager;
        }
        
        public List<PolarNode> FindPath(PolarNode startNode, PolarNode endNode)
        {
            //1.Convert PolarNodes to int2 coordinates to calculate a path on one ring
            var startPos = CalculateEntityNodePosition(startNode, startNode.ParentRing.RingSettings.fi);
            var endPos = CalculateEntityNodePosition(endNode, startNode.ParentRing.RingSettings.fi);
            var ringGridSize = new int2(startNode.ParentRing.RingSettings.depth, 360 / startNode.ParentRing.RingSettings.fi);
            var pathNodes = new NativeList<int2>(Allocator.TempJob);
            
            //2.Calculate path 
            var findPathJob = new FindPathJob
            {
                StartPosition = startPos,
                EndPosition = endPos,
                GridSize = ringGridSize,
                PathNodes = pathNodes,
            };
            
            // var jobHandle = findPathJob.Schedule();
            // jobHandle.Complete();
            findPathJob.Run();

            //3.Convert path positions to PolarNodes
            var result = new List<PolarNode>();
            
            foreach (var pathPosition in pathNodes)
            {
                var polarNode = CalculatePolarNode(pathPosition, startNode.ParentRing);
                result.Add(polarNode);
            }

            pathNodes.Dispose();
            return result;
        }

        private int2 CalculateEntityNodePosition(PolarNode startNode, int segmentFi)
        {
            var x = startNode.PolarGridPosition.D;
            var y = startNode.PolarGridPosition.Fi / segmentFi;
            
            return new int2(x, y);
        }

        private PolarNode CalculatePolarNode(int2 position, Ring parentRing)
        {
            var polarGridPosition = new PolarGridPosition(
                parentRing.RingIndex,
                position.x,
                position.y * parentRing.RingSettings.fi,
                parentRing.RingSettings.height);

            return _gridManager.GetPolarNode(polarGridPosition);
        }
    }

    public struct PathNode
    {
        public int Depth;
        public int FiSegment;

        public int Index;

        public int GCost;
        public int HCost;
        public int FCost;

        public bool IsWalkable;

        public int CameFromNodeIndex;

        public void CalculateFCost()
        {
            FCost = GCost + HCost;
        }
    }
}