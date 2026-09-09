using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.DOTS.Components.ComponentData;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Systems.People
{
    public partial struct ArriveAtHomeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localTransform, personHouse, shiftData, person, entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PersonHouse>, RefRW<ShiftData>, RefRW<Person>>()
                         .WithEntityAccess())
            {
                var enterWorkCheckDistance = 1f * 1f; 
                if (math.distancesq(localTransform.ValueRO.Position, personHouse.ValueRO.Position) > enterWorkCheckDistance)
                {
                    continue;
                }

                shiftData.ValueRW.ShiftFinishedForTheDay = false;
            }
        }
    }
}