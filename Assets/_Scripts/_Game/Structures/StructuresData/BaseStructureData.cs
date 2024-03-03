using System;
using System.Collections.Generic;

using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    public abstract class BaseStructureData : ScriptableObject, IStructureData
    {
        [SerializeField]
        private List<ResourceAmount> resourceCost;
        public List<ResourceAmount> Cost => resourceCost;
        
        [SerializeField]
        private StructureSizeType structureSizeType;
        public StructureSizeType StructureSizeType => structureSizeType;
        
        [SerializeField]
        private float scale = 1f;
        public float Scale => scale;

        public abstract StructureType StructureType { get; }
    }

    public interface IStructureData
    {
        List<ResourceAmount> Cost { get; }
        StructureType StructureType { get; }
        StructureSizeType StructureSizeType { get; }
        float Scale { get; }
    }

    public enum StructureType
    {
        Structure,
        Road,
        Wall,
    }

    [Serializable]
    public enum StructureSizeType
    {
        Size1X1,
        Size1X2,
        Size2X1,
        Size2X2,
        Size2X3,
        Size3X2,
        Size3X3,
    }
}