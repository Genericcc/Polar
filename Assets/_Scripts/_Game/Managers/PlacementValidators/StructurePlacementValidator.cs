using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Zenject;

namespace _Scripts._Game.Managers.PlacementValidators
{
    public class StructurePlacementValidator : IPlacementValidator
    {
        [Inject]
        private PolarGridManager _polarGridManager;
        
        public bool Validate(List<PolarNode> nodes, IStructureData structureData)
        {
            return CanBuildOnNodes(nodes);
        }

        private bool CanBuildOnNodes(IEnumerable<PolarNode> buildingNodes)
        {
            if (buildingNodes.All(x => x.IsFree))
            {
                return true;
            }

            return false;
        }
    }
}