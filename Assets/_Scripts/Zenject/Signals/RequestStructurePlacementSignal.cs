using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Unity.Transforms;

namespace _Scripts.Zenject.Signals
{
    public class RequestStructurePlacementSignal
    {
        public List<PolarNode> Nodes { get; }
        public IStructureData StructureData { get; }
        public LocalTransform LocalTransform { get; }

        public RequestStructurePlacementSignal(List<PolarNode> nodes, IStructureData structureData, LocalTransform localTransform)
        {
            Nodes = nodes;
            StructureData = structureData;
            LocalTransform = localTransform;
        }
    }
}