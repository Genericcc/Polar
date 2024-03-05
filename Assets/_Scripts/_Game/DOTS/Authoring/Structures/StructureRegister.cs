using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.Structures
{
    public class StructureRegister : MonoBehaviour
    {
        public List<GameObject> structures;

        class Baker : Baker<StructureRegister>
        {
            public override void Bake(StructureRegister authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<Structure>(entity);
                
                foreach(var structure in authoring.structures)
                {
                    buffer.Add(new Structure
                    {
                        Prefab = GetEntity(structure, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
    
    public struct Structure : IBufferElementData
    {
        public Entity Prefab;
    }
}