using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.Buffers
{
    public struct Waypoint : IBufferElementData
    {
        public float3 Position;
    }
}