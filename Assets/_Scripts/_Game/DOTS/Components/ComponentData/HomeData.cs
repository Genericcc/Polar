using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.ComponentData
{
    public struct HomeData : IComponentData
    {
        public float3 Position;
        public PolarGridPosition HomeCoordinates;
    }
}