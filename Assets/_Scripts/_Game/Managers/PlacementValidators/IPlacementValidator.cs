using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts._Game.Managers.PlacementValidators
{
    public interface IPlacementValidator
    {
        bool Validate(List<PolarNode> polarNodes, IStructureData structureData);
    }
}