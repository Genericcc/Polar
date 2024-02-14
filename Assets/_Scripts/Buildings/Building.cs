using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

using UnityEngine;

namespace _Scripts.Buildings
{
    public abstract class Building : MonoBehaviour
    {
        public BuildingData buildingData;
        public PolarNode polarNode;

        public string Name => buildingData.ToString();
        
        public void Initialise(PolarNode newPolarNode, BuildingData newBuildingData)
        {
            polarNode = newPolarNode;
            buildingData = newBuildingData;

            polarNode.PlaceBuilding(this);
        }
        
        public abstract void OnBuild();
    }
}