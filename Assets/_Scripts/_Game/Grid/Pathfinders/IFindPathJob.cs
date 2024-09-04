using Unity.Burst;
using Unity.Collections;
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

        public void Execute()
        {
            var pathNodeArray = new NativeArray<PathNode>(GridSize.x * GridSize.y, Allocator.Temp);
            var frontierList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);
            var neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0);
            neighbourOffsetArray[0] = new int2(+1, 0);
            neighbourOffsetArray[0] = new int2(0, +1);
            neighbourOffsetArray[0] = new int2(0, -1);

            for (var r = 0; r < GridSize.x; r++)
            {
                for (var fi = 0; fi < GridSize.y; fi++)
                {
                    var pathNode = new PathNode();

                    pathNode.R = r;
                    pathNode.Fi = fi;
                    pathNode.Index = CalculateIndex(r, fi, GridSize.x);

                    pathNode.GCost = int.MaxValue;
                    pathNode.HCost = CalculateDistanceCost(new int2(r, fi), EndPosition);
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
                        currentFrontierNode.R + neighbourOffset.x,
                        currentFrontierNode.Fi + neighbourOffset.y);

                    if (!IsPositionInsideGrid(GridSize, neighbourPosition))
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

                    var frontierNodePosition = new int2(currentFrontierNode.R, currentFrontierNode.Fi);
                    var tentativeGCost = currentFrontierNode.GCost +
                                         CalculateDistanceCost(frontierNodePosition, neighbourPosition);

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
                //Debug.Log("Failed to find path");
            }
            else
            {
                var path = CalculatePath(pathNodeArray, endNode);
                
                //reverse path using for a for loop
                for (var i = 0; i < path.Length / 2; i++)
                {
                    (path[i], path[path.Length - 1 - i]) = (path[path.Length - 1 - i], path[i]);
                }
                
                path.Dispose();
            }

            pathNodeArray.Dispose();
            frontierList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
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
                path.Add(new int2(endNode.R, endNode.Fi));

                var currentNode = endNode;

                while (currentNode.CameFromNodeIndex != -1)
                {
                    var cameFromNode = pathNodeArray[currentNode.CameFromNodeIndex];
                    path.Add(new int2(cameFromNode.R, cameFromNode.Fi));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private static bool IsPositionInsideGrid(int2 gridSize, int2 neighbourNodePosition)
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

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            var rDistance = math.abs(aPosition.x - bPosition.x);
            var fiDistance = math.abs(aPosition.y - bPosition.y);
            var remaining = math.abs(rDistance - fiDistance);

            return MoveStraightCost * remaining; //dodać ewentualnie + MoveDiagonalCost * math.min(rDistance, fiDistance);
        }

        private int CalculateIndex(int r, int fi, int gridSize)
        {
            return r + fi * gridSize;
        }
    }
}