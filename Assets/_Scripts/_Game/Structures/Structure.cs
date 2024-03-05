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
        public IStructureData StructureData { get; private set; }
        public List<PolarNode> polarNodes;

        // [SerializeField]
        // private Transform pivot;
        //
        // [SerializeField]
        // private Transform centre;
        //
        // private void Awake()
        // {
        //     if (pivot == null)
        //     {
        //         pivot = transform.Find("Pivot");
        //     }
        //     
        //     if (centre == null)
        //     {
        //         centre = transform.Find("Centre");
        //     }
        // }

        public void Initialise(List<PolarNode> newPolarNodes, IStructureData newStructureData)
        {
            polarNodes = newPolarNodes;
            StructureData = newStructureData;

            //AlignTransform();
        }
        
        

        public abstract void OnBuild();
        public abstract void OnDemolish();
    }
}