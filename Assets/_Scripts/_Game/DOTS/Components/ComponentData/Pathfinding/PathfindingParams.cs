using _Scripts._Game.Grid;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Components.ComponentData.Pathfinding
{
    //TODO Change into IEnableableComponent
    public struct PathfindingParams : IComponentData, IEnableableComponent
    {
        public PolarGridPosition StartCoords;
        public PolarGridPosition EndCoords;
    }
}