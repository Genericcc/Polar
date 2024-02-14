using System;

using _Scripts.Zenject.Signals;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace _Scripts.Grid
{
    public class PolarNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private PolarGridPosition _polarGridPosition;
        private PolarGridSystem _polarGridSystem;
        
        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        private SignalBus _signalBus;
        private Vector3 _worldPosition;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialise(PolarGridSystem polarGridSystem, PolarGridPosition polarGridPosition)
        {
            _polarGridSystem = polarGridSystem;
            _polarGridPosition = polarGridPosition;

            _worldPosition = _polarGridSystem.GetWorldPosition(_polarGridPosition);
        }

        private void Start()
        {
            //textMeshPro.text = _polarNode?.ToString();
        }

        public Vector3 GetWorldPosition()
        {
            return _worldPosition;
        }

        public override string ToString()
        {
            return _polarGridPosition.ToString();
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
    }
}