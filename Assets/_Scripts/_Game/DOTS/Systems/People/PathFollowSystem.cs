using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Tags;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Systems.People
{
    public partial struct PathFollowSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Person>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (speedRO, transformRW, nextPathIndexRW, waypoints) 
                     in SystemAPI.Query<RefRO<Speed>, RefRW<LocalTransform>, RefRW<NextPathIndex>, DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>())
            {
                ref readonly var speed = ref speedRO.ValueRO.Value;
                ref LocalTransform transform = ref transformRW.ValueRW;
                ref var nextPathIndex = ref nextPathIndexRW.ValueRW.Value;

                if (math.distance(transform.Position, waypoints[nextPathIndex].Position) < 0.1f)
                {
                    nextPathIndex = (nextPathIndex + 1) % waypoints.Length;
                }
                
                var dir = math.normalize(waypoints[nextPathIndex].Position - transform.Position);
                transform.Position += dir * SystemAPI.Time.DeltaTime * speed;
                transform.Rotation = quaternion.LookRotationSafe(dir, new float3(0f,1f,0f));
                //Almost the same
                // transform.Rotation = TransformHelpers.LookAtRotation(
                //     transform.Position,
                //     waypoints[nextPathIndex].Position,
                //     new float3(0f,1f,0f));
            }
        }
    }
}