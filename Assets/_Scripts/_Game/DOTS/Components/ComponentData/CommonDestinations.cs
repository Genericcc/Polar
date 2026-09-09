using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.ComponentData
{
    public struct PersonHouse : IComponentData
    {
        public float3 Position;
        public PolarGridPosition HomeCoords;
    }
    
    public struct PersonWork : IComponentData, IEnableableComponent
    {
        public float3 Position;
        public PolarGridPosition WorkCoords;
        public float ShiftDuration;
    }

    public struct ShiftData : IComponentData
    {
        public float CurrentShiftTime;
        public float MaxShiftTime;
        public bool ShiftFinishedForTheDay;
    }
}