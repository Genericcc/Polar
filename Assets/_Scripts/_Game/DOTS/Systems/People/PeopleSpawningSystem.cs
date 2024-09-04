using _Scripts._Game.DOTS.Authoring.Structures;
using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
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
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            
            state.RequireForUpdate<StructureManagerTag>();
            //state.RequireForUpdate<SomethingBuiltTag>();
            state.RequireForUpdate<PeopleSpawnerConfig>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spawnOrders = SystemAPI.GetSingletonBuffer<PeopleSpawnOrder>();
            
            if (spawnOrders.Length <= 0)
            {
                return;
            }
            
            var spawnerConfig = SystemAPI.GetSingleton<PeopleSpawnerConfig>();
            var structureWaypoints = SystemAPI.GetSingletonBuffer<StructureWaypointBuffer>();
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged);
            
            var random = Random.CreateFromIndex(_updateCounter++);

            for (var i = 0; i < spawnOrders.Length; i++)
            {
                for (var j = 0; j < 1 /*spawnOrders[i].PeopleAmount*/; j++)
                {
                    var entity = ecb.Instantiate(spawnerConfig.PersonPrefab);
                    
                    ecb.SetComponent(entity, spawnOrders[i].SpawnTransform);
                    
                    ecb.SetComponent(entity, new Speed
                    {
                        Value = random.NextFloat(spawnerConfig.MinSpeed, spawnerConfig.MaxSpeed)
                    });
                    
                    ecb.SetComponent(entity, new HomeData
                    {
                        Position = spawnOrders[i].SpawnTransform.Position
                    });
                    
                    //TODO add Tag WorkplaceStructure and then add work from there
                    var index = random.NextInt(0, structureWaypoints.Length);
                    ecb.SetComponent(entity, new WorkData
                    {
                        Position = structureWaypoints[index].Position,
                    });
                    
                    ecb.SetComponent(entity, new PathfindingParams
                    {
                        StartPosition = spawnOrders[i].SpawnTransform.Position,
                        EndPosition = structureWaypoints[index].Position,
                    });
                }
            }
                
            spawnOrders.Clear();
        }
    }
}