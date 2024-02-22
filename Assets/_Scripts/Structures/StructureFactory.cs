using System.Collections.Generic;

using _Scripts.Grid;
using _Scripts.Structures.StructuresData;

using Zenject;

namespace _Scripts.Structures
{
    public class StructureFactory : PlaceholderFactory<List<PolarNode>, StructureData, Structure>
    {
    }

    public class CustomStructureFactory : IFactory<List<PolarNode>, StructureData, Structure>
    {
        private readonly DiContainer _container;
        private readonly Structure _structurePrefab;

        public CustomStructureFactory(DiContainer container,
            Structure structurePrefab)
        {
            _container = container;
            _structurePrefab = structurePrefab;
        }

        public Structure Create(List<PolarNode> polarNodes, StructureData structureData)
        {
            var result = _container.InstantiatePrefabForComponent<Structure>(_structurePrefab);

            result.Initialise(polarNodes, structureData);
            
            return result;
        }
    }
}