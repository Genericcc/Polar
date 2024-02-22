using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(fileName = "HouseBuildingData", menuName = "Data/BuildingData/HouseBuildingData", order = 1)]
    public class HouseStructureData : StructureData
    {
        [SerializeField]
        public ResourceAmount workersAmount;
        
        
    }
}