using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace _Scripts.Grid
{
    [CreateAssetMenu(menuName = "Create PolarGridRingsSettings", fileName = "PolarGridRingsSettings", order = 0)]
    [Serializable]
    public class PolarGridRingsSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        public List<RingSettings> ringSettingsList;

        public int segmentsInGame;
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // if (ringSettingsList == null)
            // {
            //     return;
            // }
            //
            // for (var i = 0; i < ringSettingsList.Count; i++)
            // {
            //     RingSettings currentRingSettings = ringSettingsList[i];
            //     
            //     if (currentRingSettings.fi == 0 || currentRingSettings.fi == 360)
            //     {
            //         currentRingSettings.fi = 60;
            //         ringSettingsList[i] = currentRingSettings;
            //     }
            // }
        }
    }

    [Serializable]
    public struct RingSettings
    {
        public int depth;
        public int fi;
        public int height;
    }
}