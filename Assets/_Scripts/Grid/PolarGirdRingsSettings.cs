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
    }

    [Serializable]
    public struct RingSettings
    {
        public int maxFields;
        public int height;
    }
}