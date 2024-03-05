using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public class RoadPlacementHandler : IPlacementHandler
    {
        public PolarNode GetNode(Vector3 mousePosition)
        {
            throw new System.NotImplementedException();
        }

        public LocalTransform GetBuildTransform(List<PolarNode> polarNodes, IStructureData structureData)
        {
            throw new System.NotImplementedException();
        }
    }
}