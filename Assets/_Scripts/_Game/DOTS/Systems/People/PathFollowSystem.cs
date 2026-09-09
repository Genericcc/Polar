using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;

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
            foreach (var (speedRO, transformRW, currentPathNodeIndexRW, waypoints, person) 
                     in SystemAPI.Query<RefRO<Speed>, RefRW<LocalTransform>, RefRW<CurrentPathNodeIndex>, DynamicBuffer<Waypoint>, RefRO<Person>>())
            {
                //TODO change this placeholder Working state or change the entire check
                if (person.ValueRO.PersonState == PersonState.Working)
                {
                    continue;
                }
                
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
                transform.Rotation = quaternion.LookRotationSafe(dir2, new float3(0f, 1f, 0f));
                
                if (math.distance(transform.Position, targetWaypointPosition) < 0.1f)
                {
                    currentPathNodeIndex--;
                }
            }
        }
    }
}