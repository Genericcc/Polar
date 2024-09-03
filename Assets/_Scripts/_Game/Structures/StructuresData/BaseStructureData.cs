using System;
using System.Collections.Generic;

using _Scripts._Game.GameResources;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    public abstract class BaseStructureData : ScriptableObject, IStructureData
    {
        public int id;
        public int ID => id;
        
        [SerializeField]
        private string pathToPrefab;
        
        [SerializeField]
        private Structure structurePrefab;
        public GameObject Prefab => structurePrefab.gameObject;
        
        [SerializeField]
        private StructureSizeType structureSizeType;
        public StructureSizeType StructureSizeType => structureSizeType;
        
        public float Scale => structurePrefab.scale;
        
        [SerializeField]
        private int inhabitants = 0;
        public int Inhabitants => inhabitants;
        
        [SerializeField]
        private List<ResourceAmount> resourceCost;
        public List<ResourceAmount> Cost => resourceCost;
        
        public abstract StructureType StructureType { get; }
        
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            #if UNITY_EDITOR
            if (structurePrefab == null)
            {
                //load prefab from path
                structurePrefab = AssetDatabase.LoadAssetAtPath<Structure>(pathToPrefab);
                EditorUtility.SetDirty(this);
            }
            #endif
        }
    }

    public interface IStructureData
    {
        int ID { get; }
        GameObject Prefab { get; }
        int Inhabitants { get; }
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