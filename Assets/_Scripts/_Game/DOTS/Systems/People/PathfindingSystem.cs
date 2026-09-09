using _Scripts._Game.DOTS.Authoring.People;
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
            
            state.RequireForUpdate<TheGridEntity>();
            
            _world = state.WorldUnmanaged;
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gridEntity = SystemAPI.GetSingleton<TheGridEntity>();
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (pathfindingParams, currentPathNodeIndexRW,  waypoints, entity) 
                     in SystemAPI.Query<RefRO<PathfindingParams>, RefRW<CurrentPathNodeIndex>, DynamicBuffer<Waypoint>>()
                                 .WithEntityAccess())
            {
                ref var currentTargetPathNodeIndex = ref currentPathNodeIndexRW.ValueRW.Index;

                //If the Person is going somewhere or doesn't have a target, he doesn't need to find a new path
                if (currentTargetPathNodeIndex != -1)
                {
                    continue;
                }           
                
                //Extract ring data
                var ringFound = false;
                RingData ringData = default;
                foreach (var gridRing in gridEntity.Grid.Rings)
                {
                    if (gridRing.Index == pathfindingParams.ValueRO.StartCoords.ParentRingIndex)
                    {
                        ringData = gridRing;
                        ringFound = true;
                    }
                }
                if (!ringFound)
                {
                    continue;
                }

                var startPosition = CalculateEntityNodePosition(pathfindingParams.ValueRO.StartCoords, ringData.FiStep);
                var endPosition = CalculateEntityNodePosition(pathfindingParams.ValueRO.EndCoords, ringData.FiStep);
                
                var path = new NativeList<int2>(Allocator.TempJob);
                var findPathJob = new FindPathJob
                {
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    GridSize = ringData.GridSize,
                    PathNodes = path
                };
                
                var jobHandle = findPathJob.Schedule();
                jobHandle.Complete();
                
                foreach (var pathNode in path)
                {
                    var polarGridPosition = new PolarGridPosition
                    {
                        D = pathNode.x,
                        Fi = pathNode.y * ringData.FiStep,
                        H = ringData.WorldOrigin.y, 
                        ParentRingIndex = pathfindingParams.ValueRO.StartCoords.ParentRingIndex
                    };
                    var nodePosition = gridEntity.Grid.GetWorldFromPolar(polarGridPosition);
                    waypoints.Add(new Waypoint { Position = nodePosition } );
                }
                
                currentTargetPathNodeIndex = path.Length - 1;
                
                path.Dispose();
                ecb.SetComponentEnabled<PathfindingParams>(entity, false);
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