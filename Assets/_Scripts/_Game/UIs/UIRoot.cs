using System.Collections.Generic;

using _Scripts._Game.UIs.Controllers;

using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace _Scripts._Game.UIs
{
    [RequireComponent(typeof(UIDocument))]
    public class UIRoot : MonoBehaviour, IPointerOverUiProbe
    {
        [SerializeField]
        private VisualTreeAsset structureButtonTemplate;

        [SerializeField]
        private VisualTreeAsset resourceChipTemplate;

        [SerializeField]
        private VisualTreeAsset costRowTemplate;

        public VisualTreeAsset StructureButtonTemplate => structureButtonTemplate;
        public VisualTreeAsset ResourceChipTemplate => resourceChipTemplate;
        public VisualTreeAsset CostRowTemplate => costRowTemplate;

        private UIDocument _document;
        private List<IHudController> _controllers;
        private bool _started;

        [Inject]
        public void Construct(List<IHudController> controllers)
        {
            _controllers = controllers;
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            _started = true;
            BindAll();
        }

        private void OnEnable()
        {
            if (_started)
            {
                BindAll();
            }
        }

        private void BindAll()
        {
            var root = _document.rootVisualElement;

            StretchToFillPanel(root);
            SetIgnorePicking(root, "hud-root", "top-bar", "build-bar", "build-bar-items");

            foreach (var controller in _controllers)
            {
                controller.Bind(root);
            }
        }

        private static void StretchToFillPanel(VisualElement root)
        {
            root.style.flexGrow = 1;

            foreach (var child in root.Children())
            {
                child.style.flexGrow = 1;
            }
        }

        private static void SetIgnorePicking(VisualElement root, params string[] elementNames)
        {
            foreach (var elementName in elementNames)
            {
                var element = root.Q(elementName);
                if (element != null)
                {
                    element.pickingMode = PickingMode.Ignore;
                }
            }
        }

        private void OnDisable()
        {
            if (!_started)
            {
                return;
            }

            foreach (var controller in _controllers)
            {
                controller.Dispose();
            }
        }

        public bool IsPointerOverUI(Vector2 screenPosition)
        {
            var panel = _document.rootVisualElement?.panel;
            if (panel == null)
            {
                return false;
            }

            var panelPosition = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            return panel.Pick(panelPosition) != null;
        }
    }
}
