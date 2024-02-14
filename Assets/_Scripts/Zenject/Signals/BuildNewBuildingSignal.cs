using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

namespace _Scripts.Zenject.Signals
{
    public class BuildNewBuildingSignal
    {
        public readonly BuildingData BuildingData;
        public readonly PolarNode PolarNode;

        public BuildNewBuildingSignal(BuildingData buildingData, PolarNode polarNode)
        {
            BuildingData = buildingData;
            PolarNode = polarNode;
        }
    }
}