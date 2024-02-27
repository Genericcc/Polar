using _Scripts._Game.Managers;

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
        private readonly PolarGridManager _polarGridManager;
        private readonly Ring _ringPrefab;

        public CustomRingFactory(DiContainer container,
            PolarGridRingsSettings polarGridRingsSettings,
            PolarGridManager polarGridManager,
            Ring ringPrefab)
        {
            _container = container;
            _polarGridRingsSettings = polarGridRingsSettings;
            _polarGridManager = polarGridManager;
            _ringPrefab = ringPrefab;
        }
        
        public Ring Create(int ringIndex)
        {
            var ring = _container.InstantiatePrefabForComponent<Ring>(_ringPrefab);
            var ringSettings = _polarGridRingsSettings.ringSettingsList[ringIndex];

            ring.name = "Ring_" + ringIndex;
            ring.transform.SetParent(_polarGridManager.transform, true);
            ring.transform.position += new Vector3(0, ringSettings.height, 0);

            ring.Initialise(ringIndex, ringSettings);

            return ring;
        }
    }
}