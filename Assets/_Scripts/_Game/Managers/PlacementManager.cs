using System;
using System.Collections;
using System.Collections.Generic;

using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.Grid;
using _Scripts._Game.Managers.PlacementHandlers;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;
using _Scripts.Zenject.Installers;
using _Scripts.Zenject.Signals;

using Unity.Entities;
using Unity.Physics;

using UnityEngine;
using UnityEngine.InputSystem;

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

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            var handler = GetPlacementHandler(structureData);
            var validator = GetPlacementValidator(structureData);
            
            //TODO Inject _input and validator into handler? 
            _coroutine = StartCoroutine(handler._WaitForInput(_input, structureData, validator));
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
            _input.DisablePlayerActions();
        }
    }
}