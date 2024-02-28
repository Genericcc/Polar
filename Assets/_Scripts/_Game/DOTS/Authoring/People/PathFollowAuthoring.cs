using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.ComponentData;
using _Scripts._Game.DOTS.Components.Tags;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.People
{
    public class PathFollowAuthoring : MonoBehaviour
    {
        class Baker : Baker<PathFollowAuthoring>
        {
            public override void Bake(PathFollowAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddBuffer<Waypoint>(entity);
                AddComponent(entity, new Speed
                {
                    Value = 0,
                });
                AddComponent(entity, new NextPathIndex
                {
                    Value = 0,
                });
                
                Debug.Log($"{typeof(PathFollowAuthoring)} baked");
            }
        }
        
    }
}