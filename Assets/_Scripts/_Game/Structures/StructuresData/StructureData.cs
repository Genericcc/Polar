using System;
using System.Collections.Generic;

using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    public abstract class StructureData : ScriptableObject
    {
        public List<ResourceAmount> resourceCost;

        public StructureType structureType;
        public StructureSizeType structureSizeType;
    }

    [Serializable]
    public enum StructureType
    {
        House,
        Road,
        Wall
    }

    [Serializable]
    public enum StructureSizeType
    {
        Size2X2,
        Size2X3,
        Size3X2,
        Size3X3,
    }
}