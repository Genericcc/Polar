using _Scripts._Game.DOTS.Components.ComponentData;
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

                AddComponent<Person>(entity);
                AddComponent<HomeData>(entity);
                AddComponent<WorkData>(entity);
            }
        }
    }
}