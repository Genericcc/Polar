using _Scripts._Game.GameResources;

using UnityEngine;

namespace _Scripts._Game.Structures.StructuresData
{
    [CreateAssetMenu(
        fileName = nameof(WorkStructureData),
        menuName = PolarAssetMenu.Root + "Structures/StructureData/" + nameof(WorkStructureData),
        order = PolarAssetMenu.Order)]
    public class WorkStructureData : BaseStructureData
    {
        public override StructureType StructureType => StructureType.Work;
        
        [SerializeField]
        public ResourceAmount ResourceAmount;

        [SerializeField]
        public float ProductionInterval;
    }
}