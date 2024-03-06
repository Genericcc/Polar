using Unity.Entities;
using Unity.Transforms;

namespace _Scripts._Game.DOTS.Components.Buffers
{
    public struct PeopleSpawnOrder : IBufferElementData
    {
        public int PeopleAmount;
        public LocalTransform SpawnTransform;
    }
}