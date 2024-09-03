using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using UnityEngine;

namespace _Scripts._Game.Structures
{
    public abstract class Structure : MonoBehaviour, IStructure
    {
        public float scale;
        
        public IStructureData StructureData { get; private set; }
        public List<PolarNode> polarNodes;
        
        public void Initialise(List<PolarNode> newPolarNodes, IStructureData newStructureData)
        {
            polarNodes = newPolarNodes;
            StructureData = newStructureData;
        }
        
        public abstract void OnBuild();
        public abstract void OnDemolish();        
        private void OnValidate()
        {
            #if UNITY_EDITOR
            if (transform.localScale != new Vector3(scale, scale, scale))
            {
                transform.localScale = new Vector3(scale, scale, scale);
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }
    }
}