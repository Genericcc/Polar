using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using _Scripts._Game.DOTS.Components.Configs;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Grid;
using _Scripts._Game.Grid.Pathfinders;
using _Scripts._Game.Managers.PlacementHandlers;

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
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PeopleSpawnerConfig>();
            
            state.RequireForUpdate<StructureManagerTag>();
            //state.RequireForUpdate<SomethingBuiltTag>();
            
            state.RequireForUpdate<Waypoint>();
            
            _world = state.WorldUnmanaged;
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //for testing before roads
            var structureWaypoints = SystemAPI.GetSingletonBuffer<StructureWaypointBuffer>();
            if (structureWaypoints.Length <= 1)
            {
                return;
            }
            
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (pathfindingParams, currentPathNodeIndexRW,  waypoints, entity) 
                     in SystemAPI.Query<RefRO<PathfindingParams>, RefRW<CurrentPathNodeIndex>, DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>()
                                 .WithEntityAccess())
            {
                ref var currentPathNodeIndexReference = ref currentPathNodeIndexRW.ValueRW.Index;
                var random = Random.CreateFromIndex(_updateCounter++);
                var posBuffer = new NativeArray<Waypoint>(4, Allocator.Temp);
                
                if (currentPathNodeIndexReference != -1)
                {
                    continue;
                }           
                
                // var findPathJob = new FindPathJob
                // {
                //     StartPosition = pathfindingParams.ValueRO.StartPosition,
                //     EndPosition = pathfindingParams.ValueRO.EndPosition,,
                // }
                
                
                posBuffer[^1] = new Waypoint { Position = pathfindingParams.ValueRO.StartPosition };
                posBuffer[0] = new Waypoint { Position = pathfindingParams.ValueRO.EndPosition };

                for (var i = posBuffer.Length - 2; i >= 1; i--)
                {
                    var randomIndex = random.NextInt(0, structureWaypoints.Length);
                    var pos = structureWaypoints[randomIndex];
                    
                    posBuffer[i] = new Waypoint 
                    { 
                        Position = pos.Position
                    };
                }

                currentPathNodeIndexReference = posBuffer.Length - 1;
                waypoints.AddRange(posBuffer);
                
                ecb.RemoveComponent<PathfindingParams>(entity);
            }
        }
    }
}