using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;

using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public class PlacementStatusController : IHudController
    {
        private readonly PlacementManager _placementManager;

        private VisualElement _panel;
        private Label _label;
        private Button _cancelButton;

        public PlacementStatusController(PlacementManager placementManager)
        {
            _placementManager = placementManager;
        }

        public void Bind(VisualElement root)
        {
            _panel = root.Q("placement-status");
            _label = root.Q<Label>("placement-label");
            _cancelButton = root.Q<Button>("placement-cancel");

            _panel.SetEnabled(!_panel.ClassListContains("is-hidden"));

            _cancelButton.clicked += _placementManager.CancelPlacement;
            _placementManager.PlacementStarted += OnPlacementStarted;
            _placementManager.PlacementEnded += OnPlacementEnded;
        }

        private void OnPlacementStarted(IStructureData structureData)
        {
            _label.text = structureData.DisplayName;
            _panel.RemoveFromClassList("is-hidden");
            _panel.SetEnabled(true);
        }

        private void OnPlacementEnded()
        {
            _panel.AddToClassList("is-hidden");
            _panel.SetEnabled(false);
        }

        public void Dispose()
        {
            _cancelButton.clicked -= _placementManager.CancelPlacement;
            _placementManager.PlacementStarted -= OnPlacementStarted;
            _placementManager.PlacementEnded -= OnPlacementEnded;
        }
    }
}
