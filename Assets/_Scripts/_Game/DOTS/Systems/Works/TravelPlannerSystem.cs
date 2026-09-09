using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using _Scripts._Game.DOTS.Systems.People;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Works
{
    [UpdateBefore(typeof(PathFindingSystem))]
    public partial struct TravelPlannerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {        
            foreach (var (personWork, personHouse, pathfindingParams, entity) 
                     in SystemAPI.Query<RefRO<PersonWork>, RefRO<PersonHouse>, RefRW<PathfindingParams>>()
                         .WithDisabled<PathfindingParams>()
                         .WithEntityAccess())
            {
                pathfindingParams.ValueRW.StartCoords = personHouse.ValueRO.HomeCoords;
                pathfindingParams.ValueRW.EndCoords = personWork.ValueRO.WorkCoords;
                
                SystemAPI.SetComponentEnabled<PathfindingParams>(entity, true);
            }
        }
    }
}