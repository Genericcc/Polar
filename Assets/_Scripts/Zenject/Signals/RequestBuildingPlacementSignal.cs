using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts.Zenject.Signals
{
    public class RequestBuildingPlacementSignal
    {
        public readonly BaseStructureData BaseStructureData;
        public readonly PolarNode OriginBuildNode;

        public RequestBuildingPlacementSignal(BaseStructureData baseStructureData, PolarNode originBuildNode)
        {
            BaseStructureData = baseStructureData;
            OriginBuildNode = originBuildNode;
        }
    }
}