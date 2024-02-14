using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

using Zenject;

namespace _Scripts.Buildings
{
    public class BuildingFactory : PlaceholderFactory<List<PolarNode>, BuildingData, Building>
    {
    }

    public class CustomBuildingFactory : IFactory<List<PolarNode>, BuildingData, Building>
    {
        private readonly DiContainer _container;
        private readonly Building _buildingPrefab;

        public CustomBuildingFactory(DiContainer container,
            Building buildingPrefab)
        {
            _container = container;
            _buildingPrefab = buildingPrefab;
        }

        public Building Create(List<PolarNode> polarNodes, BuildingData buildingData)
        {
            var result = _container.InstantiatePrefabForComponent<Building>(_buildingPrefab);

            result.Initialise(polarNodes, buildingData);
            
            return result;
        }
    }
}