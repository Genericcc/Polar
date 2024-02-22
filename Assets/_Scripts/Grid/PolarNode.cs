using _Scripts.Managers;
using _Scripts.Structures;
using _Scripts.Zenject.Signals;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace _Scripts.Grid
{
    public class PolarNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        public PolarGridPosition PolarGridPosition { get; private set; }

        [SerializeField]
        public Ring ParentRing { get; set; }
        
        public Vector3 WorldPosition { get; private set; }

        [SerializeField]
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

        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            //Chujowo ustawiane
            transform.SetParent(polarGridManager.transform);
        }

        public void Initialise(PolarGridPosition polarGridPosition, Ring ring)
        {
            ParentRing = ring;
            PolarGridPosition = polarGridPosition;
        }

        public void Set()
        {
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
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            meshRenderer.materials = highlightMaterials;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            meshRenderer.materials = defaultMaterial;
        }
        
        //Test, potem robić z BuildingManagera?
        public void OnPointerClick(PointerEventData eventData)
        {
            _signalBus.Fire(new RequestBuildingPlacementSignal(null, this));
        }

        // public PolarGridPosition GetHorizontalNeighbourPosition(bool isToTheRight)
        // {
        //     var nextPos = PolarGridPosition + new PolarGridPosition(0, 0, _parentRingSettings.fi, 0);
        //
        //     
        // }
    }
}