using System.Linq;

using _Scripts._Game.DOTS.Authoring.Structures;
using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Managers;
using _Scripts._Game.Structures;

using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Systems.Structures
{
    public partial struct StructureSpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            state.RequireForUpdate<StructureManagerTag>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buildOrders = SystemAPI.GetSingletonBuffer<StructurePlacementOrder>();

             if (buildOrders.Length <= 0)
             {
                 return;
             }
            
            var availableStructures = SystemAPI.GetSingletonBuffer<AvailableStructure>();
            var ecbBSG = SystemAPI
                         .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                         .CreateCommandBuffer(state.WorldUnmanaged);

            // foreach (var (availableStructures, placementOrders) 
            //          in SystemAPI.Query<DynamicBuffer<AvailableStructure>, DynamicBuffer<StructurePlacementOrder>>())
            // {
            //     if (placementOrders.Length <= 0)
            //     {
            //         return;
            //     }
            //     
            //     var e = ecbBSG.Instantiate(availableStructures[])
            // }

            for (var i = 0; i < buildOrders.Length; i++)
            {
                var structure = availableStructures[buildOrders[i].StructureIndex].Prefab;
                var e = ecbBSG.Instantiate(structure);
                ecbBSG.SetComponent(e, 
                    LocalTransform.FromPositionRotationScale(
                        buildOrders[i].NewPosition, 
                        buildOrders[i].NewRotation,
                        0.25f));
            }
            
            buildOrders.Clear();
        }
    }
}