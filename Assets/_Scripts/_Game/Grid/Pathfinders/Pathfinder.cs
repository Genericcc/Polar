using System.Collections.Generic;

using _Scripts._Game.Managers;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace _Scripts._Game.Grid.Pathfinders
{
    public class Pathfinder
    {
        private const int MoveStraightCost = 10;
        private const int MoveDiagonalCost = 14;
        
        private readonly PolarGridManager _polarGrid;
        
        public Pathfinder (PolarGridManager polarGrid)
        {
            _polarGrid = polarGrid;
        }
        
        public List<PolarNode> FindPath(PolarGridPosition startPosition, PolarGridPosition endPosition)
        {
            return new List<PolarNode>();
        }

        public void FindPath(int2 startPosition, int2 endPosition, int2 gridSize, int ringFi)
        {
            var pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);
            var frontierList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);
            var neighbourOffsetArray = new NativeArray<int2>(
                new int2[]
                {
                    new int2(-1, 0), //Top
                    new int2(+1, 0), //Bottom
                    new int2(0, +1), //Right
                    new int2(0, -1), //Left
                },
                Allocator.Temp);

            for (var r = 0; r < gridSize.x; r++)
            {
                for (var fi = 0; fi < gridSize.y; fi++)
                {
                    var pathNode = new PathNode();

                    pathNode.R = r;
                    pathNode.Fi = fi;
                    pathNode.Index = CalculateIndex(r, fi, gridSize.x);

                    pathNode.GCost = int.MaxValue;
                    pathNode.HCost = CalculateDistanceCost(new int2(r, fi), endPosition);
                    pathNode.CalculateFCost();

                    pathNode.IsWalkable = true;
                    pathNode.CameFromNodeIndex = -1;

                    pathNodeArray[pathNode.Index] = pathNode;
                }
            }

            var endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
            
            var startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
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

                    if (!IsPositionInsideGrid(gridSize, neighbourPosition))
                    {
                        continue;
                    }

                    var neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

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
                Debug.Log("Failed to find path");
            }
            else
            {
                var path = CalculatePath(pathNodeArray, endNode);

                foreach (var position in path)
                {
                    
                    Debug.Log($"{position}");
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

    public struct PathNode
    {
        public int R;
        public int Fi;

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