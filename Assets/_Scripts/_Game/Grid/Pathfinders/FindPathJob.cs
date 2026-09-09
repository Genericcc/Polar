using _Scripts._Game.DOTS.Components.Buffers;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace _Scripts._Game.Grid.Pathfinders
{
    [BurstCompile]
    public struct FindPathJob : IJob
    {
        private const int MoveStraightCost = 10;
        private const int MoveDiagonalCost = 14;
        
        public int2 StartPosition;
        public int2 EndPosition;
        public int2 GridSize;
        
        //public DynamicBuffer<Waypoint> PathPositionBuffer;
        public NativeList<int2> PathNodes;

        public void Execute()
        {
            var pathNodeArray = new NativeArray<PathNode>(GridSize.x * GridSize.y, Allocator.Temp);
            var frontierList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);
            
            var neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0);
            neighbourOffsetArray[1] = new int2(+1, 0);
            neighbourOffsetArray[2] = new int2(0, +1);
            neighbourOffsetArray[3] = new int2(0, -1);            
            
            for (var x = 0; x < GridSize.x; x++)
            {
                for (var y = 0; y < GridSize.y; y++)
                {
                    var pathNode = new PathNode();

                    pathNode.Depth = x;
                    pathNode.FiSegment = y;
                    
                    pathNode.Index = CalculateIndex(x, y, GridSize.x);

                    pathNode.GCost = int.MaxValue;
                    pathNode.HCost = CalculateDistanceCost(new int2(x, y), EndPosition, GridSize);
                    pathNode.CalculateFCost();

                    pathNode.IsWalkable = true;
                    pathNode.CameFromNodeIndex = -1;

                    pathNodeArray[pathNode.Index] = pathNode;
                }
            }

            var endNodeIndex = CalculateIndex(EndPosition.x, EndPosition.y, GridSize.x);
            
            var startNode = pathNodeArray[CalculateIndex(StartPosition.x, StartPosition.y, GridSize.x)];
            startNode.GCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.Index] = startNode;

            frontierList.Add(startNode.Index);

            while (frontierList.Length > 0)
            {
                var currentFrontierNodeIndex = GetLowestFCostNodeIndex(frontierList, pathNodeArray);
                var currentFrontierNode = pathNodeArray[currentFrontierNodeIndex];

                if (currentFrontierNodeIndex == endNodeIndex)
                {
                    //done
                    break;
                }

                //Remove currentFrontierNode from frontier list (removes copies too)
                for (var i = 0; i < frontierList.Length; i++)
                {
                    if (frontierList[i] == currentFrontierNodeIndex)
                    {
                        frontierList.RemoveAtSwapBack(i);

                        break;
                    }
                }

                closedList.Add(currentFrontierNodeIndex);

                for (var i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    var neighbourOffset = neighbourOffsetArray[i];
                    var neighbourPosition = new int2(
                        currentFrontierNode.Depth + neighbourOffset.x,
                        WrapFiSegment(currentFrontierNode.FiSegment + neighbourOffset.y, GridSize.y));

                    if (!IsPositionInsideGrid(neighbourPosition, GridSize))
                    {
                        continue;
                    }

                    var neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, GridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        continue;
                    }

                    var neighbourNode = pathNodeArray[neighbourNodeIndex];

                    if (!neighbourNode.IsWalkable)
                    {
                        continue;
                    }

                    //Sąsiad jest zawsze o jeden krok w bok albo w głąb
                    var tentativeGCost = currentFrontierNode.GCost + MoveStraightCost;

                    if (tentativeGCost >= neighbourNode.GCost)
                    {
                        continue;
                    }

                    neighbourNode.CameFromNodeIndex = currentFrontierNodeIndex;
                    neighbourNode.GCost = tentativeGCost;
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (!frontierList.Contains(neighbourNode.Index))
                    {
                        frontierList.Add(neighbourNode.Index);
                    }
                }
            }

            var endNode = pathNodeArray[endNodeIndex];

            if (endNode.CameFromNodeIndex == -1)
            {
            }
            else
            {
                CalculatePath(pathNodeArray, endNode, PathNodes);
            }

            pathNodeArray.Dispose();
            frontierList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
        }
        
        // private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<Waypoint> pathPositionBuffer)
        // {
        //     if (endNode.CameFromNodeIndex == -1)
        //     {
        //     }
        //     else
        //     {
        //         //TODO tu jest najebane na pewno 
        //         pathPositionBuffer.Add(new Waypoint { Position = new float3(endNode.Depth, endNode.FiSegment, 0) });
        //
        //         var currentNode = endNode;
        //
        //         while (currentNode.CameFromNodeIndex != -1)
        //         {
        //             var cameFromNode = pathNodeArray[currentNode.CameFromNodeIndex];
        //             pathPositionBuffer.Add(new Waypoint { Position = new float3(cameFromNode.Depth, cameFromNode.FiSegment, 0) });
        //             currentNode = cameFromNode;
        //         }
        //     }
        // }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, NativeList<int2> pathNodes)
        {
            if (endNode.CameFromNodeIndex == -1)
            {
            }
            else
            {
                pathNodes.Add(new int2(endNode.Depth, endNode.FiSegment));

                var currentNode = endNode;

                while (currentNode.CameFromNodeIndex != -1)
                {
                    var cameFromNode = pathNodeArray[currentNode.CameFromNodeIndex];
                    pathNodes.Add(new int2(cameFromNode.Depth, cameFromNode.FiSegment));
                    currentNode = cameFromNode;
                }
            }
        }

        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.CameFromNodeIndex == -1)
            {
                return new NativeList<int2>();
            }
            else
            {
                var path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.Depth, endNode.FiSegment));

                var currentNode = endNode;

                while (currentNode.CameFromNodeIndex != -1)
                {
                    var cameFromNode = pathNodeArray[currentNode.CameFromNodeIndex];
                    path.Add(new int2(cameFromNode.Depth, cameFromNode.FiSegment));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private static bool IsPositionInsideGrid(int2 neighbourNodePosition, int2 gridSize)
        {
            return neighbourNodePosition.x >= 0 &&
                   neighbourNodePosition.x < gridSize.x &&
                   neighbourNodePosition.y >= 0 &&
                   neighbourNodePosition.y < gridSize.y;
        }

        private int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            var currentLowestFCostNode = pathNodeArray[openList[0]];

            for (var i = 1; i < openList.Length; i++)
            {
                var potentialLowestFNode = pathNodeArray[openList[i]];

                if (potentialLowestFNode.FCost < currentLowestFCostNode.FCost)
                {
                    currentLowestFCostNode = potentialLowestFNode;
                }
            }

            return currentLowestFCostNode.Index;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition, int2 gridSize)
        {
            var depthDistance = math.abs(aPosition.x - bPosition.x);

            var dy = math.abs(aPosition.y - bPosition.y);
            var fiSegmentDistance = math.min(dy, gridSize.y - dy);

            return MoveStraightCost * (depthDistance + fiSegmentDistance);
        }

        // Fi jest cykliczne: ostatni segment pierścienia sąsiaduje z segmentem 0
        private static int WrapFiSegment(int fiSegment, int segmentCount)
        {
            return (fiSegment + segmentCount) % segmentCount;
        }

        private int CalculateIndex(int r, int fi, int gridSize)
        {
            return r + fi * gridSize;
        }
    }
}