using System;
using System.Collections.Generic;

using _Scripts._Game.GameResources;
using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;

using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public class BuildBarController : IHudController
    {
        private readonly StructureDictionary _structureDictionary;
        private readonly StructureButtonFactory _buttonFactory;
        private readonly InputReader _inputReader;
        private readonly ResourceStore _resourceStore;

        private readonly Dictionary<int, (Button Button, IStructureData Data)> _buttons = new();

        private VisualElement _buildBar;

        public event Action<IStructureData> StructureHovered;
        public event Action StructureUnhovered;

        public BuildBarController(
            StructureDictionary structureDictionary,
            StructureButtonFactory buttonFactory,
            InputReader inputReader,
            ResourceStore resourceStore)
        {
            _structureDictionary = structureDictionary;
            _buttonFactory = buttonFactory;
            _inputReader = inputReader;
            _resourceStore = resourceStore;
        }

        public void Bind(VisualElement root)
        {
            _buildBar = root.Q("build-bar");
            var itemsContainer = root.Q("build-bar-items");

            itemsContainer.Clear();
            _buttons.Clear();

            foreach (var structure in _structureDictionary.structures)
            {
                if (structure == null)
                {
                    Debug.LogWarning("BuildBarController: StructureDictionary holds an empty entry, skipping it");
                    continue;
                }

                var button = _buttonFactory.Create(structure, itemsContainer);
                _buttons[structure.ID] = (button, structure);

                var capturedStructure = structure;
                button.RegisterCallback<PointerEnterEvent>(_ => StructureHovered?.Invoke(capturedStructure));
                button.RegisterCallback<PointerLeaveEvent>(_ => StructureUnhovered?.Invoke());
            }

            RefreshAffordability();

            _resourceStore.Changed += RefreshAffordability;
            _inputReader.StructuresMenu += ToggleVisibility;
        }

        private void RefreshAffordability()
        {
            foreach (var entry in _buttons.Values)
            {
                entry.Button.SetEnabled(_resourceStore.CanAfford(entry.Data.Cost));
            }
        }

        private void ToggleVisibility()
        {
            _buildBar.ToggleInClassList("is-hidden");

            _buildBar.SetEnabled(!_buildBar.ClassListContains("is-hidden"));
        }

        public void Dispose()
        {
            _resourceStore.Changed -= RefreshAffordability;
            _inputReader.StructuresMenu -= ToggleVisibility;
        }
    }
}
