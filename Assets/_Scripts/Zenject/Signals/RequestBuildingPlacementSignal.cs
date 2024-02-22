using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts.Zenject.Signals
{
    public class RequestBuildingPlacementSignal
    {
        public readonly StructureData StructureData;
        public readonly PolarNode OriginBuildNode;

        public RequestBuildingPlacementSignal(StructureData structureData, PolarNode originBuildNode)
        {
            StructureData = structureData;
            OriginBuildNode = originBuildNode;
        }
    }
}