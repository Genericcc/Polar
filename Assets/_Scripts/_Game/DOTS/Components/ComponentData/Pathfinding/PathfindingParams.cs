using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.ComponentData.Pathfinding
{
    public struct PathfindingParams : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
    }
}