using _Scripts.Buildings;
using _Scripts.Managers;
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
        public Vector3 WorldPosition { get; private set; }
        
        [SerializeField]
        public Building Building { get; private set; }

        [SerializeField]
        public RingSettings ParentRingSettings { get; private set; }
        
        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        public bool IsFree => Building == null;

        private SignalBus _signalBus;
        private PolarGridSystem _polarGridSystem;
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            
            //Chujowo ustawiane
            transform.SetParent(polarGridManager.transform);
        }

        public void Initialise(PolarGridSystem polarGridSystem, PolarGridPosition polarGridPosition, RingSettings ringSettings)
        {
            _polarGridSystem = polarGridSystem;
            ParentRingSettings = ringSettings;
            
            PolarGridPosition = polarGridPosition;

            WorldPosition = _polarGridSystem.PolarToWorld(PolarGridPosition);
            
            textMeshPro.text = ToString();
        }

        public void SetBuilding(Building building)
        {
            Building = building;
        }

        public void ClearBuilding()
        {
            Building = null;
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