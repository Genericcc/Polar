using System;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.Grid
{
    public class PolarGridDebugObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private PolarNode _polarNode;
        
        [SerializeField]
        private TextMeshPro textMeshPro;
        
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private Material[] defaultMaterial;
        
        [SerializeField]
        private Material[] highlightMaterials;

        public void AssignPolarNode(PolarNode polarNode)
        {
            _polarNode = polarNode;
        }

        private void Start()
        {
            // textMeshPro = GetComponentInChildren<TextMeshPro>();
            // meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            textMeshPro.text = _polarNode?.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            meshRenderer.materials = highlightMaterials;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            meshRenderer.materials = defaultMaterial;
        }
    }
}