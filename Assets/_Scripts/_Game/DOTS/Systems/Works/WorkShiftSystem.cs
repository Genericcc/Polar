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
            
            foreach (var (jobData, workLocationData, homeLocationData, entity) 
                     in SystemAPI.Query<RefRW<WorkData>, RefRO<JobData>, RefRO<HomeLocationData>>()
                         .WithAll<IsAtWork>() // <-- the equivalent of WithEnabled
                         .WithEntityAccess())
            {
                if (jobData.ValueRO.WorkFinishedForTheDay)
                {
                    throw new Exception($"{nameof(IsAtWork)} was supposed to be Disabled but Query still processed it");
                }
                
                jobData.ValueRW.CurrentShiftTime += SystemAPI.Time.DeltaTime;
                if (jobData.ValueRO.CurrentShiftTime < jobData.ValueRO.MaxShiftTime)
                {
                    continue;
                }
                jobData.ValueRW.WorkFinishedForTheDay = true;
                SystemAPI.SetComponentEnabled<IsAtWork>(entity, false);
                ecb.AddComponent(entity, new PathfindingParams
                {
                    StartCoords = workLocationData.ValueRO.WorkCoords,
                    EndCoords = homeLocationData.ValueRO.HomeCoords,
                });
                
                //TODO when event system is added, trigger the event
                // workData.ValueRW.OnWorkFinished.IsTriggered = true;
            }
        }
    }
}