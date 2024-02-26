using System;
using System.Collections.Generic;

using UnityEngine;

namespace _Scripts._Game.Grid
{
    [CreateAssetMenu(menuName = "Create PolarGridRingsSettings", fileName = "PolarGridRingsSettings", order = 0)]
    [Serializable]
    public class PolarGridRingsSettings : ScriptableObject
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
        public Color color;
    }
}