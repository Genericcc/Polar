using _Scripts.DOTS.Authoring.People;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace _Scripts.DOTS.Systems.Poeple
{
    partial struct PeopleSpawningSystem : ISystem
    {
        private uint _updateCounter;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PeopleSpawnerConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var peopleQuery = SystemAPI.QueryBuilder().WithAll<Person>().Build();

            // Only spawn cubes when no cubes currently exist.
            if (peopleQuery.IsEmpty)
            {
                var spawnerConfig = SystemAPI.GetSingleton<PeopleSpawnerConfig>();
                var prefab = spawnerConfig.PersonPrefab;
                var count = spawnerConfig.PeopleCount;

                // Instantiating an entity creates copy entities with the same component types and values.
                var instances = state.EntityManager.Instantiate(prefab, count, Allocator.Temp);

                // Unlike new Random(), CreateFromIndex() hashes the random seed
                // so that similar seeds don't produce similar results.
                var random = Random.CreateFromIndex(_updateCounter++);

                foreach (var entity in instances)
                {
                    var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                    var destination = SystemAPI.GetComponentRW<PersonDestination>(entity);
                    
                    transform.ValueRW.Position = (random.NextFloat3() - new float3(0.5f, 0, 0.5f)) * 20;
                    destination.ValueRW.Destination = (random.NextFloat3() - new float3(0.7f, 0, 0.3f)) * 100;
                }
            }
        }
    }
}