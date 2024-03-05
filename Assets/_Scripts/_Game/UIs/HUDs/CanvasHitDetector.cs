using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts._Game.UIs.HUDs
{
    public class CanvasHitDetector : MonoBehaviour 
    {
        private GraphicRaycaster _graphicRaycaster;

        private void Start()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        public bool IsPointerOverUI()
        {
            var mousePosition = Mouse.current.position.ReadValue();

            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = mousePosition;

            // Use the GraphicRaycaster instance to determine how many UI items
            // the pointer event hits.  If this value is greater-than zero, skip
            // further processing.
            var results = new List<RaycastResult>();
            _graphicRaycaster.Raycast(pointerEventData, results);
            return results.Count > 0;
        }
    }
}