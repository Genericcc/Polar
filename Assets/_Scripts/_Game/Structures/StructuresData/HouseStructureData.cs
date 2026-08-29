using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(
        fileName = nameof(HouseStructureData),
        menuName = PolarAssetMenu.Root + "Structures/StructureData/" + nameof(HouseStructureData),
        order = PolarAssetMenu.Order)]
    public class HouseStructureData : BaseStructureData
    {
        public override StructureType StructureType => StructureType.Structure;
        
        [SerializeField]
        public ResourceAmount workersAmount;

        
    }
}