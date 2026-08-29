using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(
        fileName = nameof(WallStructureData),
        menuName = PolarAssetMenu.Root + "Structures/StructureData/" + nameof(WallStructureData),
        order = PolarAssetMenu.Order)]
    public class WallStructureData : BaseStructureData
    {
        public override StructureType StructureType => StructureType.Wall;
    }
}