using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

using UnityEngine;

namespace _Scripts.Buildings
{
    public abstract class Building : MonoBehaviour
    {
        public BuildingData buildingData;
        public List<PolarNode> polarNodes;

        public string Name => buildingData.ToString();
        
        public void Initialise(List<PolarNode> newPolarNodes, BuildingData newBuildingData)
        {
            polarNodes = newPolarNodes;
            buildingData = newBuildingData;

            AlignTransform();
        }
        
        private void AlignTransform()
        {
            var buildingTransform = transform;
            var newPos = new Vector3();

            foreach (var polarNode in polarNodes)
            {
                newPos.x += polarNode.WorldPosition.x;
                newPos.z += polarNode.WorldPosition.z;
            }

            //newPos = new Vector3(newPos.x / polarNodes.Count, newPos.y / polarNodes.Count, newPos.z / polarNodes.Count);
            newPos *= 1f / polarNodes.Count;
            
            buildingTransform.position = newPos;
            buildingTransform.rotation = Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0));;
        }
        
        public abstract void OnBuild();
    }
}