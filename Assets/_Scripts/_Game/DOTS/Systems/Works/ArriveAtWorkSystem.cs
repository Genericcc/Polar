using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.DOTS.Components.ComponentData;
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
            foreach (var (localTransform, shiftData, personWork, entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShiftData>, RefRO<PersonWork>>()
                         .WithAll<Person>()
                         .WithDisabled<IsAtWork>()
                         .WithEntityAccess())
            {
                if (shiftData.ValueRO.ShiftFinishedForTheDay)
                {
                    continue;
                }
                
                var enterWorkCheckDistance = 1f * 1f; 
                if (math.distancesq(localTransform.ValueRO.Position, personWork.ValueRO.Position) > enterWorkCheckDistance)
                {
                    continue;
                }

                shiftData.ValueRW.CurrentShiftTime = 0f;
                shiftData.ValueRW.MaxShiftTime = personWork.ValueRO.ShiftDuration;
                SystemAPI.SetComponentEnabled<IsAtWork>(entity, true);
            }
        }
    }
}