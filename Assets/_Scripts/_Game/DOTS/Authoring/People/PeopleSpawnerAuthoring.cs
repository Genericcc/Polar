using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.People
{
    public class PeopleSpawnerAuthoring : MonoBehaviour
    {
        public GameObject personPrefab;
        public int peopleCount;

        class Baker : Baker<PeopleSpawnerAuthoring>
        {
            public override void Bake(PeopleSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PeopleSpawnerConfig
                {
                    PersonPrefab = GetEntity(authoring.personPrefab, TransformUsageFlags.Dynamic),
                    PeopleCount = authoring.peopleCount,
                });
            }
        }
    }

    public struct PeopleSpawnerConfig : IComponentData
    {
        public Entity PersonPrefab;
        public int PeopleCount;
    }
}