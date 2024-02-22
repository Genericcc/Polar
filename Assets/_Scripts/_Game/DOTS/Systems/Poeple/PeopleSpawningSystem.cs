using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.Managers;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace _Scripts._Game.DOTS.Systems.Poeple
{
    partial struct PeopleSpawningSystem : ISystem
    {
        private uint _updateCounter;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BuildingManagerTag>();
            state.RequireForUpdate<SomethingBuiltTag>();
            state.RequireForUpdate<PeopleSpawnerConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            
            var world = state.WorldUnmanaged;

            var buildings = SystemAPI.GetSingletonEntity<BuildingManagerTag>();
            var buffer = world.EntityManager.GetBuffer<BuildingPositionBuffer>(buildings);

            var peopleQuery = SystemAPI.QueryBuilder().WithAll<Person>().Build();

            if (peopleQuery.IsEmpty)
            {
                var spawnerConfig = SystemAPI.GetSingleton<PeopleSpawnerConfig>();
                var prefab = spawnerConfig.PersonPrefab;
                var count = spawnerConfig.PeopleCount;

                var instances = state.EntityManager.Instantiate(prefab, count, Allocator.Temp);
                
                var random = Random.CreateFromIndex(_updateCounter++);

                foreach (var entity in instances)
                {
                    var start = random.NextInt(0, buffer.Length);
                    var end = random.NextInt(0, buffer.Length);
                    
                    var startDestination = buffer[start];
                    var endDestination = buffer[end];
                    
                    var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                    var destination = SystemAPI.GetComponentRW<PersonDestination>(entity);
                    
                    transform.ValueRW.Position = startDestination.Value;
                    destination.ValueRW.Destination = endDestination.Value;
                }
            }
        }
    }
}