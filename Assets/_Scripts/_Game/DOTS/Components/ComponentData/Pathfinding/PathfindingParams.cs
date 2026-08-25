using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.ComponentData.Pathfinding
{
    public struct PathfindingParams : IComponentData
    {
        public PolarGridPosition StartCoords;
        public PolarGridPosition EndCoords;
    }
}