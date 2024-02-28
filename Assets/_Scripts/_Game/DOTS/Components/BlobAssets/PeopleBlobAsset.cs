using _Scripts._Game.DOTS.Authoring.People;
using _Scripts._Game.DOTS.Components.Configs;

using Unity.Entities;

namespace _Scripts._Game.DOTS.Components.BlobAssets
{
    public struct PeopleBlobAsset : IComponentData
    {
        public BlobAssetReference<PeopleSpawnerConfig> Value;
    }
}