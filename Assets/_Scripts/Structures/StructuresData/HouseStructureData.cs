using _Scripts.GameResources;

using UnityEngine;

namespace _Scripts.Structures.StructuresData
{
    [CreateAssetMenu(fileName = "HouseBuildingData", menuName = "Data/BuildingData/HouseBuildingData", order = 1)]
    public class HouseStructureData : StructureData
    {
        [SerializeField]
        public ResourceAmount workersAmount;
        
        
    }
}