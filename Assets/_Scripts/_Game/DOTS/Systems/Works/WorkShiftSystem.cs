using System;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Works
{
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
            
            foreach (var (shiftData, personWork, personHouse, entity) 
                     in SystemAPI.Query<RefRW<ShiftData>, RefRO<PersonWork>, RefRO<PersonHouse>>()
                         .WithAll<IsAtWork>() // <-- the equivalent of WithEnabled
                         .WithEntityAccess())
            {
                if (shiftData.ValueRO.ShiftFinishedForTheDay)
                {
                    throw new Exception($"{nameof(IsAtWork)} was supposed to be Disabled but Query still processed it");
                }
                
                shiftData.ValueRW.CurrentShiftTime += SystemAPI.Time.DeltaTime;
                if (shiftData.ValueRO.CurrentShiftTime < shiftData.ValueRO.MaxShiftTime)
                {
                    continue;
                }
                shiftData.ValueRW.ShiftFinishedForTheDay = true;
                SystemAPI.SetComponentEnabled<IsAtWork>(entity, false);
                ecb.AddComponent(entity, new PathfindingParams
                {
                    StartCoords = personWork.ValueRO.WorkCoords,
                    EndCoords = personHouse.ValueRO.HomeCoords,
                });
                
                //TODO when event system is added, trigger the event
                // workData.ValueRW.OnWorkFinished.IsTriggered = true;
            }
        }
    }
}