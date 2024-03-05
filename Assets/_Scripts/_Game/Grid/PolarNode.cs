using System;

using _Scripts._Game.Managers;
using _Scripts._Game.Structures;
using _Scripts._Game.Structures.StructuresData;

using TMPro;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Grid
{
    public class PolarNode : MonoBehaviour
    {
        public PolarGridPosition PolarGridPosition { get; private set; }

        public Ring ParentRing { get; set; }
        
        public Vector3 WorldPosition { get; private set; }

        public IStructureData Structure { get; private set; }

        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        public bool IsFree => Structure == null;
        
        public Vector3 CentrePosition { get; set; }
        
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(PolarGridManager polarGridManager)
        {
            _polarGridManager = polarGridManager;
        }

        public void Initialise(PolarGridPosition polarGridPosition, Ring ring)
        {
            ParentRing = ring;
            PolarGridPosition = polarGridPosition;
            
            var newPosition = _polarGridManager.GetWorldFromPolar(PolarGridPosition);
            var newRotation = Quaternion.LookRotation(newPosition - new Vector3(0, newPosition.y, 0));

            var nodeTransform = transform;
            nodeTransform.position = new Vector3(newPosition.x, 0, newPosition.z);
            nodeTransform.rotation = newRotation;

            WorldPosition = nodeTransform.position;

            var purePolar = _polarGridManager.GetPurePolarFromWorld(WorldPosition);
            var purePolarShifted = new PurePolarCoords(
                purePolar.Radius + _polarGridManager.ColumnHeight / 2f, 
                purePolar.Fi + ring.RingSettings.fi / 2f, 
                ring.RingSettings.height);

            var worldPos = _polarGridManager.GetWorldFromPurePolar(purePolarShifted);

            CentrePosition = worldPos;

            textMeshPro.text = ToString();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(CentrePosition, 0.3f);
        }

        public void SetBuilding(IStructureData structure)
        {
            Structure = structure;
        }

        public void ClearBuilding()
        {
            Structure = null;
        }

        public override string ToString()
        {
            return PolarGridPosition.ToString();
        }
    }
}