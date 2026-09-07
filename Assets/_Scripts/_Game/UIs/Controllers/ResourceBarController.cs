using System;
using System.Collections.Generic;

using _Scripts._Game.GameResources;

using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public class ResourceBarController : IHudController
    {
        private readonly ResourceStore _resourceStore;
        private readonly UIRoot _uiRoot;

        private readonly Dictionary<ResourceType, Label> _valueLabels = new();

        public ResourceBarController(ResourceStore resourceStore, UIRoot uiRoot)
        {
            _resourceStore = resourceStore;
            _uiRoot = uiRoot;
        }

        public void Bind(VisualElement root)
        {
            var container = root.Q("resource-bar");

            container.Clear();
            _valueLabels.Clear();

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                var instance = _uiRoot.ResourceChipTemplate.Instantiate();
                var chip = instance.Q("resource-chip");
                container.Add(chip);

                chip.Q<Label>("resource-chip-label").text = type.ToString();

                var valueLabel = chip.Q<Label>("resource-chip-value");
                valueLabel.text = _resourceStore.Get(type).ToString();

                _valueLabels[type] = valueLabel;
            }

            _resourceStore.Changed += Refresh;
        }

        private void Refresh()
        {
            foreach (var entry in _valueLabels)
            {
                entry.Value.text = _resourceStore.Get(entry.Key).ToString();
            }
        }

        public void Dispose()
        {
            _resourceStore.Changed -= Refresh;
        }
    }
}
