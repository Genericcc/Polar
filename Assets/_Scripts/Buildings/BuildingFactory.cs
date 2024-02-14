using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

using Zenject;

namespace _Scripts.Buildings
{
    public class BuildingFactory : PlaceholderFactory<PolarNode, BuildingData, Building>
    {
    }

    public class CustomBuildingFactory : IFactory<PolarNode, BuildingData, Building>
    {
        private readonly DiContainer _container;
        private readonly Building _buildingPrefab;

        public CustomBuildingFactory(DiContainer container,
            Building buildingPrefab)
        {
            _container = container;
            _buildingPrefab = buildingPrefab;
        }

        public Building Create(PolarNode polarNode, BuildingData buildingData)
        {
            var result = _container.InstantiatePrefabForComponent<Building>(_buildingPrefab);

            result.Initialise(polarNode, buildingData);
            
            return result;
        }
    }
}