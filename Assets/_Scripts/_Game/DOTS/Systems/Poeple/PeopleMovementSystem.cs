using _Scripts._Game.DOTS.Authoring.People;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Systems.Poeple
{
    public partial struct PeopleMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Person>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (transform, destination) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PersonDestination>>()
                              .WithAll<Person>())
            {
                var position = transform.ValueRO.Position;
                var dir = destination.ValueRO.Destination - position;
                var speed = destination.ValueRO.Speed;
                
                transform.ValueRW.Position += dir * dt * speed;
                transform.ValueRW.Rotation = quaternion.LookRotation(dir, new float3(0f,1f,0f));

            }
        }
    }
}