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

        [SerializeField]
        private Transform pivot;
        
        [SerializeField]
        private Transform centre;

        private void Awake()
        {
            if (pivot == null)
            {
                pivot = transform.Find("Pivot");
            }
            
            if (centre == null)
            {
                centre = transform.Find("Centre");
            }
        }

        public void Initialise(List<PolarNode> newPolarNodes, IStructureData newStructureData)
        {
            polarNodes = newPolarNodes;
            StructureData = newStructureData;

            AlignTransform();
        }
        
        private void AlignTransform()
        {
            if (polarNodes == null || !polarNodes.Any())
            {
                Debug.LogError($"Cannot align building: {this}");
                return;
            }
            
            var newPos = GetAveragePosition();

            var buildingTransform = transform;
            buildingTransform.position = newPos;
            buildingTransform.rotation = Quaternion.LookRotation(newPos - new Vector3(0, newPos.y, 0));;
        }

        private Vector3 GetAveragePosition()
        {
            var newPos = new Vector3();
            
            foreach (var polarNode in polarNodes)
            {
                newPos.x += polarNode.WorldPosition.x;
                newPos.z += polarNode.WorldPosition.z;
            }

            //newPos = new Vector3(newPos.x / polarNodes.Count, newPos.y / polarNodes.Count, newPos.z / polarNodes.Count);
            newPos *= 1f / polarNodes.Count;

            newPos.y = polarNodes[0].WorldPosition.y;

            return newPos;
        }

        public abstract void OnBuild();
        public abstract void OnDemolish();
    }
}