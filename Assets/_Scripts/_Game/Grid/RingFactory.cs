using _Scripts._Game.Managers;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Grid
{
    public class RingFactory : PlaceholderFactory<int, RingSettings, Ring>
    {
    }

    public class CustomRingFactory : IFactory<int, RingSettings, Ring>
    {
        private readonly DiContainer _container;
        private readonly PolarGridManager _polarGridManager;
        private readonly Ring _ringPrefab;

        public CustomRingFactory(DiContainer container,
            PolarGridManager polarGridManager,
            Ring ringPrefab)
        {
            _container = container;
            _polarGridManager = polarGridManager;
            _ringPrefab = ringPrefab;
        }

        public Ring Create(int ringIndex, RingSettings ringSettings)
        {
            var ring = _container.InstantiatePrefabForComponent<Ring>(_ringPrefab);

            ring.name = "Ring_" + ringIndex;
            ring.transform.SetParent(_polarGridManager.transform, true);
            ring.transform.position += new Vector3(0, ringSettings.height, 0);

            ring.Initialise(ringIndex, ringSettings);

            return ring;
        }
    }
}