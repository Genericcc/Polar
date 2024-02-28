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
            state.RequireForUpdate<SomethingBuiltTag>();
            
            state.RequireForUpdate<Waypoint>();
            
            _world = state.WorldUnmanaged;
        }
        
        public void OnUpdate(ref SystemState state)
        {
            //For init test, change to check for AtHomeEnableComponent or something like that, then find new path
            state.Enabled = false;
            
            var buildings = SystemAPI.GetSingletonEntity<StructureManagerTag>();
            var structureWaypointBuffer = _world.EntityManager.GetBuffer<StructureWaypointBuffer>(buildings);
            
            // var peopleQuery = SystemAPI.QueryBuilder().WithAll<Person>().Build();
            // if (!peopleQuery.IsEmpty)
            // {
            //     return;
            // }
            
            //var peopleAtHomeQuery = SystemAPI.QueryBuilder()..WithAll<Person>().Build();
            
            if (structureWaypointBuffer.Length <= 0)
            {
                return;
            }
            
            foreach (var waypoints 
                     in SystemAPI.Query<DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>())
            {
                var random = Random.CreateFromIndex(_updateCounter++);
                var posBuffer = new NativeArray<Waypoint>(4, Allocator.Temp);
                
                for (var i = 0; i <= 3; i++)
                {
                    var randomIndex = random.NextInt(0, structureWaypointBuffer.Length);
                    var pos = structureWaypointBuffer[randomIndex];
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