using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Zenject;

namespace _Scripts._Game.Managers.PlacementValidators
{
    public class RoadPlacementValidator : IPlacementValidator
    {
        [Inject]
        private PolarGridManager _polarGridManager;
        
        public bool Validate(List<PolarNode> nodes, IStructureData structureData)
        {
            return CanConnectNodes(nodes);
        }

        private bool CanConnectNodes(IEnumerable<PolarNode> nodes)
        {
            return true;
        }
    }
}