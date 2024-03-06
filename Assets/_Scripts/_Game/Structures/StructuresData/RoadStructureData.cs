using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(fileName = "RoadStructureData", menuName = "Structures/StructureData/RoadStructureData", order = 1)]
    public class RoadStructureData : BaseStructureData
    {
        public override StructureType StructureType => StructureType.Road;
        
        
    }
}