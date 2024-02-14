using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using UnityEngine;

namespace _Scripts.Buildings
{
    public class BuildingFactory
    {
        public Building CreateBuilding(PolarNode targetNode, BuildingData buildingData, Building buildingPrefab)
        {
            var data = Object.Instantiate(buildingData);
            
            var buildingGameObject = GameObject.Instantiate(buildingPrefab, targetNode.WorldPosition, Quaternion.identity);
            //buildingGameObject.Initialise();

            return buildingGameObject;
        }
    }
}