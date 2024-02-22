using System.Collections.Generic;

using _Scripts.Grid;
using _Scripts.Structures.StructuresData;

using Zenject;

namespace _Scripts.Structures
{
    public class BuildingFactory : PlaceholderFactory<List<PolarNode>, StructureData, Building>
    {
    }

    public class CustomBuildingFactory : IFactory<List<PolarNode>, StructureData, Building>
    {
        private readonly DiContainer _container;
        private readonly Building _buildingPrefab;

        public CustomBuildingFactory(DiContainer container,
            Building buildingPrefab)
        {
            _container = container;
            _buildingPrefab = buildingPrefab;
        }

        public Building Create(List<PolarNode> polarNodes, StructureData structureData)
        {
            var result = _container.InstantiatePrefabForComponent<Building>(_buildingPrefab);

            result.Initialise(polarNodes, structureData);
            
            return result;
        }
    }
}