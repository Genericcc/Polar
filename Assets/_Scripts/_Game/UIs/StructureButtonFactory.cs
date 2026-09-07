using _Scripts._Game.Structures.StructuresData;
using _Scripts.Zenject.Installers;

using UnityEngine.UIElements;

using Zenject;

namespace _Scripts._Game.UIs
{
    public class StructureButtonFactory
    {
        private readonly UIRoot _uiRoot;
        private readonly SignalBus _signalBus;

        public StructureButtonFactory(UIRoot uiRoot, SignalBus signalBus)
        {
            _uiRoot = uiRoot;
            _signalBus = signalBus;
        }

        public Button Create(IStructureData structureData, VisualElement parent)
        {
            var instance = _uiRoot.StructureButtonTemplate.Instantiate();
            var button = instance.Q<Button>("structure-button");
            parent.Add(button);

            var icon = button.Q<Image>("structure-button-icon");
            icon.sprite = structureData.Icon;
            icon.style.display = structureData.Icon != null ? DisplayStyle.Flex : DisplayStyle.None;

            button.Q<Label>("structure-button-label").text = structureData.DisplayName;

            var structureId = structureData.ID;
            button.clicked += () => _signalBus.Fire(new StructureSelectedSignal(structureId));

            return button;
        }
    }
}
