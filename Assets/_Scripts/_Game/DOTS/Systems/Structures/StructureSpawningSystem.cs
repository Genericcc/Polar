using _Scripts._Game.DOTS.Authoring.Structures;
using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Structures
{
    public partial struct StructureSpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            
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
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            for (var i = 0; i < buildOrders.Length; i++)
            {
                if (TryGetStructure(availableStructures, buildOrders[i].StructureId, out var structure))
                {
                    var entity = ecb.Instantiate(structure.Prefab);
                    ecb.SetComponent(entity, buildOrders[i].NewTransform);
                }
            }

            buildOrders.Clear();
        }

        private static bool TryGetStructure(DynamicBuffer<AvailableStructure> availableStructures, 
            int requestedId, out AvailableStructure structure)
        {
            for (var i = 0; i < availableStructures.Length; i++)
            {
                if (availableStructures[i].StructureId == requestedId)
                {
                    structure = availableStructures[i];
                    return true;
                }
            }

            structure = new AvailableStructure();
            return false;
        }
    }
}