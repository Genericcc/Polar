using System.Collections.Generic;

using _Scripts.GameResources;

using UnityEngine;

namespace _Scripts.Buildings.BuildingsData
{
    public abstract class BuildingData : ScriptableObject
    {
        public List<ResourceAmount> resourceCost;
        
        public BuildingSizeType buildingSizeType;
    }

    public enum BuildingSizeType
    {
        Size2X2,
        Size2X3,
        Size3X2,
        Size3X3,
    }
}