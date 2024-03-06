using _Scripts._Game.DOTS.Components.Configs;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.People
{
    public class PeopleSpawnerAuthoring : MonoBehaviour
    {
        public GameObject personPrefab;  
        public float minSpeed;
        public float maxSpeed;

        class Baker : Baker<PeopleSpawnerAuthoring>
        {
            public override void Bake(PeopleSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PeopleSpawnerConfig
                {
                    PersonPrefab = GetEntity(authoring.personPrefab, TransformUsageFlags.Dynamic),
                    MinSpeed = authoring.minSpeed,
                    MaxSpeed = authoring.maxSpeed
                });
            }
        }
    }
}