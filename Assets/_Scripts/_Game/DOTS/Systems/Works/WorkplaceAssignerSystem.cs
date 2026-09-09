using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Works
{
    public partial struct WorkplaceAssignerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {            
            var workplaceBuffer = SystemAPI.GetSingletonBuffer<WorkplaceLocation>();
            if (workplaceBuffer.Length <= 1)
            {
                return;
            }

            foreach (var (personWork, entity) 
                     in SystemAPI.Query<RefRW<PersonWork>>()
                         .WithDisabled<IsAtWork>()
                         .WithDisabled<HasWork>()
                         .WithEntityAccess())
            {
                //Idle, no work and not at work
                personWork.ValueRW.Position = workplaceBuffer[0].Position;
                personWork.ValueRW.WorkCoords = workplaceBuffer[0].StructureCoordinates;
                personWork.ValueRW.ShiftDuration = workplaceBuffer[0].ShiftDuration;

                SystemAPI.SetComponentEnabled<HasWork>(entity, true);
            }
        }
    }
}