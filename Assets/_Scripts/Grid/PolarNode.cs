using System;

using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
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
        public PolarGridPosition PolarGridPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Building Building { get; private set; }

        private RingSettings _parentRingSettings;
        
        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        private SignalBus _signalBus;
        private PolarGridSystem _polarGridSystem;
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
        }

        public void Initialise(
            PolarGridSystem polarGridSystem, PolarGridPosition polarGridPosition, RingSettings ringSettings)
        {
            _polarGridSystem = polarGridSystem;
            _parentRingSettings = ringSettings;
            
            PolarGridPosition = polarGridPosition;

            WorldPosition = _polarGridSystem.GetWorldPosition(PolarGridPosition);
        }

        private void Start()
        {
            //textMeshPro.text = _polarNode?.ToString();
        }

        public void SetBuilding(Building building)
        {
            Building = building;
        }

        public void ClearBuilding()
        {
            Building = null;
        }

        public bool HasBuilding()
        {
            return Building != null;
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
        
        //Test, potem usunąć
        public void OnPointerClick(PointerEventData eventData)
        {
            //zmienić na buildingData
            var nodesToBuildOn = _polarGridManager.GetNeighbours(this, BuildingNodesOccupationType.Space2X2);
            
            _signalBus.Fire(new RequestBuildingPlacementSignal(null, nodesToBuildOn));
        }

        // public PolarGridPosition GetHorizontalNeighbourPosition(bool isToTheRight)
        // {
        //     var nextPos = PolarGridPosition + new PolarGridPosition(0, 0, _parentRingSettings.fi, 0);
        //
        //     
        // }
    }
}