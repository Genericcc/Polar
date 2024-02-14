using System;

namespace _Scripts.GameResources
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