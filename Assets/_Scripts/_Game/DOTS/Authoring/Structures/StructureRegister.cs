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
                var buffer = AddBuffer<AvailableStructure>(entity);
                
                foreach(var structure in authoring.structures)
                {
                    buffer.Add(new AvailableStructure
                    {
                        Prefab = GetEntity(structure, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}