using System.Collections.Generic;
using System.Linq;

using _Scripts.Grid;
using _Scripts.Structures.StructuresData;

using UnityEngine;

namespace _Scripts.Structures
{
    public abstract class Building : MonoBehaviour, IBuilding
    {
        public StructureData StructureData { get; private set; }
        public List<PolarNode> polarNodes;

        public string Name => StructureData.ToString();
        
        public void Initialise(List<PolarNode> newPolarNodes, StructureData newStructureData)
        {
            polarNodes = newPolarNodes;
            StructureData = newStructureData;

            AlignTransform();
        }
        
        //-----------Move all below to BuildingManager or BuildingFactory----------
        private void AlignTransform()
        {
            if (polarNodes == null || !polarNodes.Any())
            {
                Debug.LogError($"Cannot align building: {this}");
                return;
            }
            
            var buildingTransform = transform;
            var newPos = new Vector3();

            newPos = GetAveragePosition(newPos);

            buildingTransform.position = newPos;
            buildingTransform.rotation = Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0));;
        }

        private Vector3 GetAveragePosition(Vector3 newPos)
        {
            foreach (var polarNode in polarNodes)
            {
                newPos.x += polarNode.WorldPosition.x;
                newPos.z += polarNode.WorldPosition.z;
            }

            //newPos = new Vector3(newPos.x / polarNodes.Count, newPos.y / polarNodes.Count, newPos.z / polarNodes.Count);
            newPos *= 1f / polarNodes.Count;

            newPos.y = polarNodes[0].WorldPosition.y;

            return newPos;
        }

        public abstract void OnBuild();
        public abstract void OnDemolish();
    }
}