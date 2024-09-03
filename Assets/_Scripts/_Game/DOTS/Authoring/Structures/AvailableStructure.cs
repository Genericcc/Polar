using Unity.Entities;

namespace _Scripts._Game.DOTS.Authoring.Structures
{
    public struct AvailableStructure : IBufferElementData
    {
        public Entity Prefab;
        public int StructureId;
    }
}