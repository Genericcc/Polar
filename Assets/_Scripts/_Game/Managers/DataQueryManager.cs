using System;
using _Scripts._Game.DOTS.Authoring.People;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace _Scripts._Game.Managers
{
    public class DataQueryManager : MonoBehaviour
    {
        private const float PollIntervalSeconds = 0.25f;

        private World _world;
        private EntityQuery _personQuery;
        private bool _queryReady;

        private float _pollTimer;
        private int _lastPopulation = -1;

        public event Action<int> PopulationChanged;

        public int CurrentPopulation => Mathf.Max(0, _lastPopulation);

        [Inject]
        public void Construct()
        {
            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void Update()
        {
            if (!_queryReady)
            {
                if (_world == null || !_world.IsCreated)
                {
                    return;
                }

                _personQuery = _world.EntityManager.CreateEntityQuery(typeof(Person));
                _queryReady = true;
            }

            _pollTimer += Time.deltaTime;
            if (_pollTimer < PollIntervalSeconds)
            {
                return;
            }

            _pollTimer = 0f;

            var population = _personQuery.CalculateEntityCount();
            if (population == _lastPopulation)
            {
                return;
            }

            _lastPopulation = population;
            PopulationChanged?.Invoke(population);
        }

        private void OnDestroy()
        {
            if (_queryReady)
            {
                _personQuery.Dispose();
            }
        }
    }
}
