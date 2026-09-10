using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Works
{
    [UpdateAfter(typeof(ArriveAtWorkSystem))]
    public partial struct WorkShiftSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {            
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (shiftData, personWork, personHouse, person, entity) 
                     in SystemAPI.Query<RefRW<ShiftData>, RefRO<PersonWork>, RefRO<PersonHouse>, RefRW<Person>>()
                         .WithEntityAccess())
            {
                if (shiftData.ValueRO.ShiftFinishedForTheDay || person.ValueRW.PersonState != PersonState.Working)
                {
                    continue;
                }
                
                //Is working
                
                shiftData.ValueRW.CurrentShiftTime += SystemAPI.Time.DeltaTime;
                if (shiftData.ValueRO.CurrentShiftTime < shiftData.ValueRO.MaxShiftTime)
                {
                    continue;
                }
                
                //Finished working
                
                shiftData.ValueRW.ShiftFinishedForTheDay = true;
                person.ValueRW.PersonState = PersonState.Idle;
                
                ecb.SetComponent(entity, new PathfindingParams
                {
                    StartCoords = personWork.ValueRO.WorkCoords,
                    EndCoords = personHouse.ValueRO.HomeCoords,
                });
                ecb.SetComponentEnabled<PathfindingParams>(entity, true);
                
                //TODO when event system is added, trigger the event
                // workData.ValueRW.OnWorkFinished.IsTriggered = true;
            }
        }
    }
}