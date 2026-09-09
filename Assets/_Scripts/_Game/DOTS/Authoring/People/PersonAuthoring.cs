using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.ComponentData.Pathfinding;
using _Scripts._Game.DOTS.Components.Tags;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.People
{
    public class PersonAuthoring : MonoBehaviour
    {
        class Baker : Baker<PersonAuthoring>
        {
            public override void Bake(PersonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Person
                {
                    PersonState = PersonState.Idle
                });
                AddComponent<PersonHouse>(entity);
                AddComponent<PersonWork>(entity);
                
                AddComponent<HasWork>(entity);
                SetComponentEnabled<HasWork>(entity, false);
                AddComponent<IsAtWork>(entity);
                SetComponentEnabled<IsAtWork>(entity, false);
                
                AddComponent<PathfindingParams>(entity);
            }
        }
    }
    
    public struct Person : IComponentData
    {
        public PersonState PersonState;
    }    
    
    public enum PersonState : byte
    {
        Idle,
        Walking,
        Working,
        Resting,
    }
}