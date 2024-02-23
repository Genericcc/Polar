using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(fileName = "HouseStructureData", menuName = "Structures/StructureData/HouseBuildingData", order = 1)]
    public class HouseStructureData : BaseStructureData
    {
        [SerializeField]
        public ResourceAmount workersAmount;
        
        
    }
}