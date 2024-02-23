using _Scripts.Extensions;

using Unity.VisualScripting;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Grid
{
    public class RingFactory : PlaceholderFactory<int, Ring>
    {
    }
    
    public class CustomRingFactory : IFactory<int, Ring>
    {
        private readonly DiContainer _container;
        private readonly PolarGridRingsSettings _polarGridRingsSettings;
        private readonly Ring _ringPrefab;

        public CustomRingFactory(DiContainer container,
            PolarGridRingsSettings polarGridRingsSettings,
            Ring ringPrefab)
        {
            _container = container;
            _polarGridRingsSettings = polarGridRingsSettings;
            _ringPrefab = ringPrefab;
        }
        public Ring Create(int ringIndex)
        {
            var ring = _container.InstantiatePrefabForComponent<Ring>(_ringPrefab);
            var ringSettings = _polarGridRingsSettings.ringSettingsList[ringIndex];
            ring.name = "Ring_" + ringIndex;
            ring.Initialise(ringIndex, ringSettings);

            return ring;
        }
    }
}