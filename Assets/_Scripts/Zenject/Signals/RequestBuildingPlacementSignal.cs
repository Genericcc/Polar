using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

namespace _Scripts.Zenject.Signals
{
    public class RequestBuildingPlacementSignal
    {
        public readonly BuildingData BuildingData;
        public readonly List<PolarNode> PolarNodes;

        public RequestBuildingPlacementSignal(BuildingData buildingData, List<PolarNode> polarNodes)
        {
            BuildingData = buildingData;
            PolarNodes = polarNodes;
        }
    }
}