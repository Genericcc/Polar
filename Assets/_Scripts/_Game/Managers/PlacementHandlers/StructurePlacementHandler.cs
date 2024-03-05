using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public class StructurePlacementHandler : IPlacementHandler
    {
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(PolarGridManager polarGridManager)
        {
            _polarGridManager = polarGridManager;
        }
        
        public PolarNode GetNode(Vector3 mousePosition)
        {
            var polarPos = _polarGridManager.GetPolarFromWorld(mousePosition);
            var node = _polarGridManager.GetPolarNode(polarPos);
            
            if (node != null)
            {
                return node;
            }

            return default;
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

    public struct StructureTransform
    {
        public float3 Position;
        public quaternion Rotation;
        
    }
}