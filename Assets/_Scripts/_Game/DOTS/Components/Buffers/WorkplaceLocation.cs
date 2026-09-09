using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.Buffers
{
    public struct WorkplaceLocation : IBufferElementData
    {
        public float3 Position;
        public PolarGridPosition StructureCoordinates;
        public float ShiftDuration;
    }    
}