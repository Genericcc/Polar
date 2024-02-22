using System;

namespace _Scripts._Game.GameResources
{
    [Serializable]
    public class ResourceAmount
    {
        public ResourceType resourceType;
        public int amount;
    }

    public enum ResourceType
    {
        Worker,
        Food,
        Wood,
        Stone
    }
}