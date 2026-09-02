using _Scripts._Game.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.DOTS.Components.ComponentData
{
    public struct HomeLocationData : IComponentData
    {
        public float3 Position;
        public PolarGridPosition HomeCoords;
    }
    
    public struct JobData : IComponentData
    {
        public float3 Position;
        public PolarGridPosition WorkCoords;
        public float ShiftDuration;
    }

    public struct WorkData : IComponentData
    {
        public float CurrentShiftTime;
        public float MaxShiftTime;
        public bool WorkFinishedForTheDay;
    }
    
    public struct WorkplaceData : IComponentData
    {
        public float3 Position;
        public PolarGridPosition WorkCoords;
        public float ShiftDuration;
    }

    public struct IsAtWork : IComponentData, IEnableableComponent { }
    public struct IsAtHome : IComponentData, IEnableableComponent { }
    public struct IsInTransit : IComponentData, IEnableableComponent { }    
}