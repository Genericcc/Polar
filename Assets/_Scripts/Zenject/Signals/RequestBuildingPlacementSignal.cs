using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

namespace _Scripts.Zenject.Signals
{
    public class RequestBuildingPlacementSignal
    {
        public readonly BuildingData BuildingData;
        public readonly PolarNode OriginBuildNode;

        public RequestBuildingPlacementSignal(BuildingData buildingData, PolarNode originBuildNode)
        {
            BuildingData = buildingData;
            OriginBuildNode = originBuildNode;
        }
    }
}