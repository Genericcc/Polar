using UnityEngine;

using Zenject;

namespace _Scripts.Grid
{
    public class PolarNodeFactory : PlaceholderFactory<PolarGridSystem, PolarGridPosition, PolarNode>
    {
    }

    public class CustomPolarNodeFactory : IFactory<PolarGridSystem, PolarGridPosition, PolarNode>
    {
        private readonly DiContainer _container;
        private readonly PolarNode _prefab;

        public CustomPolarNodeFactory(DiContainer container, PolarNode prefab)
        {
            _container = container;
            _prefab = prefab;
        }
        
        public PolarNode Create(PolarGridSystem polarGridSystem, PolarGridPosition polarGridPosition)
        {
            var result = _container.InstantiatePrefabForComponent<PolarNode>(_prefab);
            
            var newPosition = polarGridSystem.GetWorldPosition(polarGridPosition);
            var newRotation = Quaternion.LookRotation(newPosition - new Vector3(0, newPosition.y, 0));

            result.transform.position = newPosition;
            result.transform.rotation = newRotation;
            
            result.Initialise(polarGridSystem, polarGridPosition);
            
            return result;
        }
    }
}