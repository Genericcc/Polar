using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(
        fileName = nameof(RoadStructureData),
        menuName = PolarAssetMenu.Root + "Structures/StructureData/" + nameof(RoadStructureData),
        order = PolarAssetMenu.Order)]
    public class RoadStructureData : BaseStructureData
    {
        public override StructureType StructureType => StructureType.Road;
        
        
    }
}