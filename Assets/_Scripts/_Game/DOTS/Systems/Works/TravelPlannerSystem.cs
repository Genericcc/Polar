using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Works
{
    public partial struct TravelPlannerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {        
            foreach (var (personWork, personHouse, pathfindingParams) 
                     in SystemAPI.Query<RefRO<PersonWork>, RefRO<PersonHouse>, RefRW<PathfindingParams>>()
                         .WithDisabled<IsAtWork>()
                         .WithAll<HasWork>())
            {
                if (pathfindingParams.ValueRO.IsAssigned)
                {
                    continue;
                }
                pathfindingParams.ValueRW.IsAssigned = true;
                
                pathfindingParams.ValueRW.StartCoords = personHouse.ValueRO.HomeCoords;
                pathfindingParams.ValueRW.EndCoords = personWork.ValueRO.WorkCoords;
            }
        }
    }
}