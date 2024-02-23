using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Structures.StructuresData;

using UnityEngine;

namespace _Scripts.Data.Dictionaries
{
    [CreateAssetMenu(menuName = "Data/Dictionaries/StructureDictionary", fileName = "StructureDictionary", order = 0)]
    public class StructureDictionary : ScriptableObject
    {
        [SerializeField]
        public List<BaseStructureData> structures;

        public BaseStructureData Get(StructureType structureType)
        {
            return structures.FirstOrDefault(x => x.structureType == structureType);
        }
    }
}