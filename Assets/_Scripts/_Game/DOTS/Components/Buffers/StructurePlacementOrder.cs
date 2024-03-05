using _Scripts._Game.Grid;

using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.Buffers
{
    public struct StructurePlacementOrder : IBufferElementData
    {
        public float3 NewPosition;
        public quaternion NewRotation;
        public int StructureIndex;
    }
}