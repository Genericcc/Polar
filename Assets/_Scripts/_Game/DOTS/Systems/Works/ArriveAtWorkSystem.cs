using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Systems.Works
{
    public partial struct ArriveAtWorkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localTransform, workData, workLocationData, entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<WorkData>, RefRO<JobData>>()
                         .WithAll<Person>()
                         .WithDisabled<IsAtWork>()
                         .WithEntityAccess())
            {
                if (workData.ValueRO.WorkFinishedForTheDay)
                {
                    continue;
                }
                
                var enterWorkCheckDistance = 1f * 1f; 
                if (math.distancesq(localTransform.ValueRO.Position, workLocationData.ValueRO.Position) > enterWorkCheckDistance)
                {
                    continue;
                }

                workData.ValueRW.CurrentShiftTime = 0f;
                workData.ValueRW.MaxShiftTime = workLocationData.ValueRO.ShiftDuration;
                SystemAPI.SetComponentEnabled<IsAtWork>(entity, true);
            }
        }
    }
}