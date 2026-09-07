using _Scripts._Game.Structures.StructuresData;

using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public class StructureInfoController : IHudController
    {
        private readonly BuildBarController _buildBarController;
        private readonly UIRoot _uiRoot;

        private VisualElement _panel;
        private Image _icon;
        private Label _name;
        private Label _size;
        private Label _inhabitants;
        private VisualElement _costContainer;

        public StructureInfoController(BuildBarController buildBarController, UIRoot uiRoot)
        {
            _buildBarController = buildBarController;
            _uiRoot = uiRoot;
        }

        public void Bind(VisualElement root)
        {
            _panel = root.Q("structure-info");
            _icon = root.Q<Image>("info-icon");
            _name = root.Q<Label>("info-name");
            _size = root.Q<Label>("info-size");
            _inhabitants = root.Q<Label>("info-inhabitants");
            _costContainer = root.Q("info-cost");

            _panel.pickingMode = PickingMode.Ignore;

            _buildBarController.StructureHovered += OnStructureHovered;
            _buildBarController.StructureUnhovered += OnStructureUnhovered;
        }

        private void OnStructureHovered(IStructureData structureData)
        {
            _icon.sprite = structureData.Icon;
            _icon.style.display = structureData.Icon != null ? DisplayStyle.Flex : DisplayStyle.None;
            _name.text = structureData.DisplayName;
            _size.text = $"Size: {structureData.StructureSizeType}";
            _inhabitants.text = $"Inhabitants: {structureData.Inhabitants}";

            _costContainer.Clear();
            if (structureData.Cost != null)
            {
                foreach (var cost in structureData.Cost)
                {
                    var instance = _uiRoot.CostRowTemplate.Instantiate();
                    var row = instance.Q("cost-row");
                    row.Q<Label>("cost-row-label").text = cost.ResourceType.ToString();
                    row.Q<Label>("cost-row-value").text = cost.Amount.ToString();
                    _costContainer.Add(row);
                }
            }

            _panel.RemoveFromClassList("is-hidden");
        }

        private void OnStructureUnhovered()
        {
            _panel.AddToClassList("is-hidden");
        }

        public void Dispose()
        {
            _buildBarController.StructureHovered -= OnStructureHovered;
            _buildBarController.StructureUnhovered -= OnStructureUnhovered;
        }
    }
}
