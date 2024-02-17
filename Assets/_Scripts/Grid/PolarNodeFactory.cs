using UnityEngine;

using Zenject;

namespace _Scripts.Grid
{
    public class PolarNodeFactory : PlaceholderFactory<PolarGridSystem, PolarGridPosition, RingSettings, PolarNode>
    {
    }

    public class CustomPolarNodeFactory : IFactory<PolarGridSystem, PolarGridPosition, RingSettings, PolarNode>
    {
        private readonly DiContainer _container;
        private readonly PolarNode _prefab;

        public CustomPolarNodeFactory(DiContainer container, PolarNode prefab)
        {
            _container = container;
            _prefab = prefab;
        }
        
        public PolarNode Create(PolarGridSystem polarGridSystem, 
            PolarGridPosition polarGridPosition, 
            RingSettings ringSettings)
        {
            var node = _container.InstantiatePrefabForComponent<PolarNode>(_prefab);
            
            var newPosition = polarGridSystem.PolarToWorld(polarGridPosition);
            var newRotation = Quaternion.LookRotation(newPosition - new Vector3(0, newPosition.y, 0));

            node.transform.position = newPosition;
            node.transform.rotation = newRotation;
            
            node.Initialise(polarGridSystem, polarGridPosition, ringSettings);
            
            return node;
        }
    }
}