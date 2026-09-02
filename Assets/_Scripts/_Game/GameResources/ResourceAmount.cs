using System;

namespace _Scripts._Game.GameResources
{
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType ResourceType;
        public int Amount;
    }

    public enum ResourceType
    {
        Food,
        Wood,
        Stone,
    }
}