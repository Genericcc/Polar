using System;
using System.Collections.Generic;

using UnityEngine;

namespace _Scripts.Grid
{
    [CreateAssetMenu(menuName = "Create PolarGirdRingsSettings", fileName = "PolarGirdRingsSettings", order = 0)]
    [Serializable]
    public class PolarGirdRingsSettings : ScriptableObject
    {
        [SerializeField]
        public List<RingSettings> ringSettingsList;

        public int segmentsInGame;
    }

    [Serializable]
    public struct RingSettings
    {
        public int depth;
        public int fi;
        public int height;
    }
}