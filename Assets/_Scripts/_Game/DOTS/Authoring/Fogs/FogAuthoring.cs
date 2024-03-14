using _Scripts._Game.DOTS.Components.Tags;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.Fogs
{
    public class FogAuthoring : MonoBehaviour
    {
        class Baker : Baker<FogAuthoring>
        {
            public override void Bake(FogAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<Fog>(entity);
            }
        }
    }
}