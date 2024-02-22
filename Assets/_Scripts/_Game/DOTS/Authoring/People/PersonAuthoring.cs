using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.People
{
    public class PersonAuthoring : MonoBehaviour
    {
        public float speed;
        
        class Baker : Baker<PersonAuthoring>
        {
            public override void Bake(PersonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<Person>(entity);
                AddComponent(entity, new PersonDestination
                {
                    Destination = new float3(),
                    Speed = authoring.speed
                });
            }
        }
    }

    public struct Person : IComponentData
    {
        
    }
    
    public struct PersonDestination : IComponentData
    {
        public float3 Destination;
        public float Speed;
    }
}