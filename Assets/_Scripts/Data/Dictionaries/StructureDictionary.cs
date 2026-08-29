using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Structures.StructuresData;

using UnityEngine;

namespace _Scripts.Data.Dictionaries
{
    [CreateAssetMenu(
        menuName = PolarAssetMenu.Root + "Data/" + nameof(StructureDictionary),
        fileName = nameof(StructureDictionary),
        order = PolarAssetMenu.Order)]
    public class StructureDictionary : ScriptableObject
    {
        [SerializeField]
        public List<BaseStructureData> structures;

        public BaseStructureData Get(StructureType structureType)
        {
            return structures.FirstOrDefault(x => x.StructureType == structureType);
        }

        public BaseStructureData Get(int id)
        {
            return structures.FirstOrDefault(x => x != null && x.ID == id);
        }
    }
}