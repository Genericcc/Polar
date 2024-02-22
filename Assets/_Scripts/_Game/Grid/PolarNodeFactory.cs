using _Scripts._Game.Managers;

using Zenject;

namespace _Scripts._Game.Grid
{
    public class PolarNodeFactory : PlaceholderFactory<PolarGridPosition, Ring, PolarNode>
    {
    }

    public class CustomPolarNodeFactory : IFactory<PolarGridPosition, Ring, PolarNode>
    {
        private readonly DiContainer _container;
        private readonly PolarNode _prefab;
        private readonly PolarGridManager _polarGridManager;

        public CustomPolarNodeFactory(DiContainer container, PolarNode prefab, PolarGridManager polarGridManager)
        {
            _container = container;
            _prefab = prefab;
            _polarGridManager = polarGridManager;
        }
        
        public PolarNode Create(PolarGridPosition polarGridPosition, Ring ring)
        {
            var node = _container.InstantiatePrefabForComponent<PolarNode>(_prefab);
            
            node.Initialise(polarGridPosition, ring);
            return node;
        }
    }
}