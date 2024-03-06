using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Configs;
using _Scripts._Game.DOTS.Components.Tags;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace _Scripts._Game.DOTS.Systems.People
{
    partial struct PathFindingSystem : ISystem
    {
        private uint _updateCounter;
        
        private WorldUnmanaged _world;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PeopleSpawnerConfig>();
            
            state.RequireForUpdate<StructureManagerTag>();
            //state.RequireForUpdate<SomethingBuiltTag>();
            
            state.RequireForUpdate<Waypoint>();
            
            _world = state.WorldUnmanaged;
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //for testing before roads
            var structureWaypoints = SystemAPI.GetSingletonBuffer<StructureWaypointBuffer>();
            if (structureWaypoints.Length <= 0)
            {
                return;
            }
            
            foreach (var (homeDataRO, workDataRO, waypoints) 
                     in SystemAPI.Query<RefRO<HomeData>, RefRO<WorkData>, DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>())
            {
                var random = Random.CreateFromIndex(_updateCounter++);
                var posBuffer = new NativeArray<Waypoint>(4, Allocator.Temp);

                posBuffer[0] = new Waypoint { Position = homeDataRO.ValueRO.Position };
                posBuffer[^1] = new Waypoint { Position = workDataRO.ValueRO.Position };
                
                for (var i = 1; i < posBuffer.Length - 1; i++)
                {
                    var randomIndex = random.NextInt(0, structureWaypoints.Length);
                    var pos = structureWaypoints[randomIndex];
                    posBuffer[i] = new Waypoint 
                    { 
                        Position = pos.Position
                    };
                }
                
                waypoints.AddRange(posBuffer);
            }
        }
    }
}