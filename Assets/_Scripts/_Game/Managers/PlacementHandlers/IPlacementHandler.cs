using System.Collections;

using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public interface IPlacementHandler
    {
        IEnumerator _WaitForInput(InputReader input, IStructureData structureData, IPlacementValidator placementValidator);
    }
}