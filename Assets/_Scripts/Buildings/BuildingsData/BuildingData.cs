using System.Collections.Generic;

using _Scripts.GameResources;

using UnityEngine;

namespace _Scripts.Buildings.BuildingsData
{
    public abstract class BuildingData : ScriptableObject
    {
        public List<ResourceAmount> resourceCost;
        
        public BuildingNodesOccupationType buildingNodesOccupationType;
    }

    public enum BuildingNodesOccupationType
    {
        Space2X2,
        Space2X3,
        Space3X2,
        Space3X3,
        
    }
}