using Unity.Entities;

namespace _Scripts._Game.DOTS.Components.Configs
{
    public struct PeopleSpawnerConfig : IComponentData
    {
        public Entity PersonPrefab;     
        public float MinSpeed;
        public float MaxSpeed;
    }
}