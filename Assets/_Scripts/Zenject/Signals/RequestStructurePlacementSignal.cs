using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts.Zenject.Signals
{
    public class RequestStructurePlacementSignal
    {
        public readonly PolarNode OriginBuildNode;

        public RequestStructurePlacementSignal(PolarNode originBuildNode)
        {
            OriginBuildNode = originBuildNode;
        }
    }
}