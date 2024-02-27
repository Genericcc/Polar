using _Scripts._Game.Managers;
using _Scripts._Game.Structures;

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

        public Structure Structure { get; private set; }

        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        public bool IsFree => Structure == null;

        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(PolarGridManager polarGridManager)
        {
            _polarGridManager = polarGridManager;
            
            //Chujowo ustawiane, lepiej byłoby mieć pogrupowane Ringami? 
            transform.SetParent(polarGridManager.transform);
        }

        public void Initialise(PolarGridPosition polarGridPosition, Ring ring)
        {
            ParentRing = ring;
            PolarGridPosition = polarGridPosition;
            
            var newPosition = _polarGridManager.GetWorldFromPolar(PolarGridPosition);
            var newRotation = Quaternion.LookRotation(newPosition - new Vector3(0, newPosition.y, 0));

            var nodeTransform = transform;
            nodeTransform.position = newPosition;
            nodeTransform.rotation = newRotation;

            WorldPosition = nodeTransform.position;
                
            textMeshPro.text = ToString();
        }

        public void SetBuilding(Structure structure)
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