using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Components.Buffers
{
    public struct StructurePlacementOrder : IBufferElementData
    {
        public LocalTransform NewTransform;
        public PolarGridPosition Coords;
        public int StructureId;
    }
}