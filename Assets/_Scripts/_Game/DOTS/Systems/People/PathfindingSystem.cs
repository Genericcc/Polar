using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using _Scripts._Game.DOTS.Components.Configs;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Grid;
using _Scripts._Game.Grid.Pathfinders;
using _Scripts._Game.Grid.PolarGridUnmanageds;
using _Scripts._Game.Managers.PlacementHandlers;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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
            
            state.RequireForUpdate<TheGridEntity>();
            
            _world = state.WorldUnmanaged;
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //for testing before roads
            var structureWaypoints = SystemAPI.GetSingletonBuffer<StructureWaypointBuffer>();
            if (structureWaypoints.Length <= 1)
            {
                return;
            }

            var gridEntity = SystemAPI.GetSingleton<TheGridEntity>();
            foreach (var gridRing in gridEntity.Grid.Rings)
            {
                foreach (var polarNodeData in gridRing.Nodes)
                {
                    Debug.Log(polarNodeData.Coords.ParentRingIndex + ", " + polarNodeData.Coords.D + ", " + polarNodeData.Coords.Fi);
                }
            }
            
            
            return;
            
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (pathfindingParams, currentPathNodeIndexRW,  waypoints, entity) 
                     in SystemAPI.Query<RefRO<PathfindingParams>, RefRW<CurrentPathNodeIndex>, DynamicBuffer<Waypoint>>()
                                 .WithAll<Person>()
                                 .WithEntityAccess())
            {
                ref var currentTargetPathNodeIndex = ref currentPathNodeIndexRW.ValueRW.Index;
                var random = Random.CreateFromIndex(_updateCounter++);

                //If the Person is going somewhere, he doesn't need to find path
                if (currentTargetPathNodeIndex > 0)
                {
                    continue;
                }           
                
                //Extract ring data
                var dFi = -1;
                int2 gridSize = new(0, 0);
                foreach (var gridRing in gridEntity.Grid.Rings)
                {
                    if (gridRing.Index == pathfindingParams.ValueRO.StartCoords.ParentRingIndex)
                    {
                        dFi = gridRing.Fi;
                        gridSize = gridRing.GridSize;
                    }
                }
                if (dFi == -1)
                {
                    continue;
                }

                var pathNodes = new NativeList<int2>(Allocator.TempJob);
                var findPathJob = new FindPathJob
                {
                    StartPosition = CalculateEntityNodePosition(pathfindingParams.ValueRO.StartCoords, dFi),
                    EndPosition = CalculateEntityNodePosition(pathfindingParams.ValueRO.EndCoords, dFi),
                    GridSize = gridSize,
                    PathNodes = pathNodes
                };
                
                //TODO verify this CTRL C + V
                var jobHandle = findPathJob.Schedule();
                jobHandle.Complete();
                
                //copying path nodes from list into waypoints buffer
                //TODO verify this line
                currentTargetPathNodeIndex = pathNodes.Length - 1;
                
                foreach (var pathNode in pathNodes)
                {
                    //TODO calculate Position from Ring data? 
                   // waypoints.Add(new Waypoint {Position = pathNode} );
                }
                
                //Clear pathfinding
                //TODO SetDisabled instead of modifying the Entity by Removal
                ecb.RemoveComponent<PathfindingParams>(entity);

                
                //----------                
                //Old approach
                var posBuffer = new NativeArray<Waypoint>(40, Allocator.Temp);
                
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

                currentTargetPathNodeIndex = posBuffer.Length - 1;
                waypoints.AddRange(posBuffer);
                
                ecb.RemoveComponent<PathfindingParams>(entity);
            }
        }        
        
        private int2 CalculateEntityNodePosition(PolarGridPosition position, int segmentFi)
        {
            var x = position.D;
            var y = position.Fi / segmentFi;
            
            return new int2(x, y);
        }
    }
}