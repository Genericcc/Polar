using _Scripts._Game.Structures.StructuresData;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    public class StructureButtonFactory : PlaceholderFactory<BaseStructureData, Transform, StructureSelectionButton>
    {
    }

    public class CustomStructureButtonFactory : IFactory<BaseStructureData, Transform, StructureSelectionButton>
    {
        private readonly DiContainer _container;
        private readonly StructureSelectionButton _prefab;

        public CustomStructureButtonFactory(DiContainer container, StructureSelectionButton prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public StructureSelectionButton Create(BaseStructureData structureData, Transform parent)
        {
            var button = _container.InstantiatePrefabForComponent<StructureSelectionButton>(_prefab);

            // SetParent(parent, false), not the Instantiate(parent) overload - that one keeps the
            // world position and breaks RectTransform scale/offset under a layout group
            button.transform.SetParent(parent, false);

            button.Initialise(structureData.ID, structureData.name);
            return button;
        }
    }
}
