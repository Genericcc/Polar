using System;
using System.Collections;

using _Scripts._Game.Managers.PlacementHandlers;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;
using _Scripts.Zenject.Installers;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers
{
    public class PlacementManager : MonoBehaviour
    {
        private InputReader _input;
        private SignalBus _signalBus;
        private StructureDictionary _structureDictionary;

        private IPlacementHandler _placementHandler;
        private IPlacementValidator _placementValidator;

        //TODO change into ValidatorFactory
        private StructurePlacementValidator _structurePlacementValidator;
        private RoadPlacementValidator _roadPlacementValidator;

        //TODO change into HandlerFactory
        private RoadPlacementHandler _roadPlacementHandler;
        private StructurePlacementHandler _structurePlacementHandler;

        private Coroutine _coroutine;

        public event Action<IStructureData> PlacementStarted;
        public event Action PlacementEnded;

        [Inject]
        public void Construct(
            SignalBus signalBus,
            InputReader inputReader,
            StructureDictionary structureDictionary)
        {
            _signalBus = signalBus;
            _input = inputReader;
            _structureDictionary = structureDictionary;
        }

        //TODO inject factories
        [Inject]
        public void InjectInterfaces(
            StructurePlacementHandler structurePlacementHandler,
            RoadPlacementHandler roadPlacementHandler,
            StructurePlacementValidator structurePlacementValidator,
            RoadPlacementValidator roadPlacementValidator)
        {
            _structurePlacementHandler = structurePlacementHandler;
            _roadPlacementHandler = roadPlacementHandler;
            _structurePlacementValidator = structurePlacementValidator;
            _roadPlacementValidator = roadPlacementValidator;
        }

        private void OnEnable()
        {
            _input.EnablePlayerActions();
        }

        public void OnStructureSelectedSignal(StructureSelectedSignal structureSelectedSignal)
        {
            var structureData = _structureDictionary.Get(structureSelectedSignal.StructureId);
            if (structureData == null)
            {
                Debug.LogError($"No structure with id {structureSelectedSignal.StructureId} in the StructureDictionary");
                return;
            }

            CancelPlacement();

            var handler = GetPlacementHandler(structureData);
            var validator = GetPlacementValidator(structureData);

            PlacementStarted?.Invoke(structureData);

            //TODO Inject _input and validator into handler?
            _coroutine = StartCoroutine(RunPlacement(handler, structureData, validator));
        }

        private IEnumerator RunPlacement(
            IPlacementHandler handler,
            IStructureData structureData,
            IPlacementValidator validator)
        {
            yield return handler.TryPlace(_input, structureData, validator);

            _coroutine = null;
            PlacementEnded?.Invoke();
        }

        public void CancelPlacement()
        {
            if (_coroutine == null)
            {
                return;
            }

            StopCoroutine(_coroutine);
            _coroutine = null;

            PlacementEnded?.Invoke();
        }

        private IPlacementHandler GetPlacementHandler(IStructureData structureData)
        {
            return structureData.StructureType switch
            {
                StructureType.Structure => _structurePlacementHandler,
                StructureType.Wall => _roadPlacementHandler,
                StructureType.Road => _roadPlacementHandler,

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IPlacementValidator GetPlacementValidator(IStructureData structureData)
        {
            return structureData.StructureType switch
            {
                StructureType.Structure => _structurePlacementValidator,
                StructureType.Wall => _roadPlacementValidator,
                StructureType.Road => _roadPlacementValidator,

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void OnDisable()
        {
            CancelPlacement();
            _input.DisablePlayerActions();
        }
    }
}
