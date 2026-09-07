using System.Collections;

using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;

namespace _Scripts._Game.Managers.PlacementHandlers
{
    public interface IPlacementHandler
    {
        IEnumerator TryPlace(
            InputReader inputReader,
            IStructureData structureData,
            IPlacementValidator placementValidator);
    }
}
