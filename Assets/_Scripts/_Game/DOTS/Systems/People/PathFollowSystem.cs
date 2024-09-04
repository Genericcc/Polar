using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Grid;

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
        
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (speedRO, transformRW, currentPathNodeIndexRW, waypoints) 
                     in SystemAPI.Query<RefRO<Speed>, RefRW<LocalTransform>, RefRW<CurrentPathNodeIndex>, DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>())
            {
                ref readonly var speed = ref speedRO.ValueRO.Value;
                ref LocalTransform transform = ref transformRW.ValueRW;
                ref var currentPathNodeIndex = ref currentPathNodeIndexRW.ValueRW.Index;
                
                if (currentPathNodeIndex < 0)
                {
                    continue;
                }
                
                var targetWaypointPosition = waypoints[currentPathNodeIndex].Position;
                
                var dir2 = math.normalizesafe(targetWaypointPosition - transform.Position);
                transform.Position += dir2 * SystemAPI.Time.DeltaTime * speed;
                transform.Rotation = quaternion.LookRotationSafe(dir2, new float3(0f,1f,0f));
                
                if (math.distance(transform.Position, targetWaypointPosition) < 0.1f)
                {
                    currentPathNodeIndex--;
                }
                
                //
                // if (waypoints.Length == 0)
                // {
                //     continue;
                // }
                //
                //
                // if (currentPathNodeIndex > waypoints.Length - 1)
                // {
                //     continue;
                // }
                //
                // if (math.distance(transform.Position, waypoints[currentPathNodeIndex].Position) < 0.1f)
                // {
                //     currentPathNodeIndex += 1;
                //     continue;
                // }
                //
                // var dir = math.normalize(waypoints[currentPathNodeIndex].Position - transform.Position);
                // transform.Position += dir * SystemAPI.Time.DeltaTime * speed;
                // transform.Rotation = quaternion.LookRotationSafe(dir, new float3(0f,1f,0f));
                //
                // //Almost the same
                // // transform.Rotation = TransformHelpers.LookAtRotation(
                // //     transform.Position,
                // //     waypoints[nextPathIndex].Position,
                // //     new float3(0f,1f,0f));
            }
        }
    }
}