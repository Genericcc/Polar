using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Configs;
using _Scripts._Game.DOTS.Components.Tags;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace _Scripts._Game.DOTS.Systems.People
{
    partial struct PeopleSpawningSystem : ISystem
    {
        private uint _updateCounter;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StructureManagerTag>();
            state.RequireForUpdate<SomethingBuiltTag>();
            state.RequireForUpdate<PeopleSpawnerConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //So it only runs once
            state.Enabled = false;
            
            var world = state.WorldUnmanaged;

            var buildings = SystemAPI.GetSingletonEntity<StructureManagerTag>();
            var structureWaypointBuffer = world.EntityManager.GetBuffer<StructureWaypointBuffer>(buildings);

            // var peopleQuery = SystemAPI.QueryBuilder().WithAll<Person>().Build();
            // if (!peopleQuery.IsEmpty)
            // {
            //     return;
            // }

            var spawnerConfig = SystemAPI.GetSingleton<PeopleSpawnerConfig>();
            var prefab = spawnerConfig.PersonPrefab;
            var count = spawnerConfig.PeopleCount;

            var instances = state.EntityManager.Instantiate(prefab, count, Allocator.Temp);
            var random = Random.CreateFromIndex(_updateCounter++);
            
            foreach (var homePos in SystemAPI.Query<RefRW<HomePosition>>().WithAll<Person>())
            {
                var newHomePosition = random.NextInt(0, structureWaypointBuffer.Length);
                homePos.ValueRW.Value = newHomePosition;
            }
            
            
            //Test
            foreach (var jobPos in SystemAPI.Query<RefRW<JobPosition>>().WithAll<Person>())
            {
                var newPos = random.NextInt(0, structureWaypointBuffer.Length);
                jobPos.ValueRW.Value = newPos;
            }
            //
            
            
            foreach (var entity in instances)
            {
                var speed = SystemAPI.GetComponentRW<Speed>(entity);
                speed.ValueRW.Value = random.NextFloat(spawnerConfig.MinSpeed, spawnerConfig.MaxSpeed);

                var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                var home = SystemAPI.GetComponentRO<HomePosition>(entity);
                    
                transform.ValueRW.Position = home.ValueRO.Value;
            }
        }
    }
}