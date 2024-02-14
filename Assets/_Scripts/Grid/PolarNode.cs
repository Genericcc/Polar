using System;

using _Scripts.Buildings;
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

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialise(PolarGridSystem polarGridSystem, PolarGridPosition polarGridPosition)
        {
            _polarGridSystem = polarGridSystem;
            PolarGridPosition = polarGridPosition;

            WorldPosition = _polarGridSystem.GetWorldPosition(PolarGridPosition);
        }

        private void Start()
        {
            //textMeshPro.text = _polarNode?.ToString();
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
            _signalBus.Fire(new BuildNewBuildingSignal(null, this));
        }

        public void PlaceBuilding(Building building)
        {
            building.transform.position = WorldPosition;
            building.transform.rotation = transform.rotation;
        }
    }
}