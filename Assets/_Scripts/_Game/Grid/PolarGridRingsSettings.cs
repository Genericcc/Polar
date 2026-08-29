using System;
using System.Collections.Generic;

using UnityEngine;

namespace _Scripts._Game.Grid
{
    [CreateAssetMenu(
        menuName = PolarAssetMenu.Root + "Grid/" + nameof(PolarGridRingsSettings),
        fileName = nameof(PolarGridRingsSettings),
        order = PolarAssetMenu.Order)]
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
        public float height;
        public Material material;
    }
}