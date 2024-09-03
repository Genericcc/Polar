using System.Collections.Generic;

using _Scripts._Game.Structures.StructuresData;

using Sirenix.OdinInspector;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.Structures
{
    public class StructureRegister : MonoBehaviour
    {
        [SerializeField]
        private List<BaseStructureData> structures;

        class Baker : Baker<StructureRegister>
        {
            public override void Bake(StructureRegister authoring)
            {
                var registerEntity = GetEntity(TransformUsageFlags.Dynamic);
                var structureBuffer = AddBuffer<AvailableStructure>(registerEntity);
                
                foreach(var structure in authoring.structures)
                {
                    structureBuffer.Add(new AvailableStructure
                    {
                        Prefab = GetEntity(structure.Prefab, TransformUsageFlags.Dynamic),
                        StructureId = structure.ID,
                    });
                }
            }
        }
    }
}