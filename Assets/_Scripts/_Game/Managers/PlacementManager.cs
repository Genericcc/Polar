using System;
using System.Collections;
using System.Collections.Generic;

using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.Grid;
using _Scripts._Game.Managers.PlacementHandlers;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures.StructuresData;
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
        [SerializeField]
        private Camera mainCamera;

        private InputReader _input;
        private SignalBus _signalBus;

        public int buildingIndex;

        private MouseWorld _mouseWorld;
        private IPlacementHandler _placementHandler;
        private StructureManager _structureManager;
        private PolarGridManager _polarGridManager;
        
        private IStructureData _structureData;
        private IPlacementValidator _placementValidator;
        
        private StructurePlacementValidator _structurePlacementValidator;
        private RoadPlacementHandler _roadPlacementHandler;
        private StructurePlacementHandler _structurePlacementHandler;
        
        private Coroutine _coroutine;

        [Inject]
        public void Construct(
            SignalBus signalBus,
            InputReader inputReader,
            MouseWorld mouseWorld,
            StructureManager structureManager,
            PolarGridManager polarGridManager)
        {
            _signalBus = signalBus;
            _input = inputReader;
            _mouseWorld = mouseWorld;
            _structureManager = structureManager;
            _polarGridManager = polarGridManager;
        }

        [Inject]
        public void InjectInterfaces(
            StructurePlacementHandler structurePlacementHandler,
            RoadPlacementHandler roadPlacementHandler,
            StructurePlacementValidator structurePlacementValidator)
        {
            _structurePlacementHandler = structurePlacementHandler;
            _roadPlacementHandler = roadPlacementHandler;
            _structurePlacementValidator = structurePlacementValidator;
        }

        private void Start()
        {
            if (_input == null)
            {
                _input = FindObjectOfType<InputReader>();
            }
        }

        private void OnEnable()
        {
            _input.EnablePlayerActions();

            mainCamera = mainCamera == null ? Camera.main : mainCamera;
        }

        public void OnSelectStructureSignal(SelectStructureSignal selectStructureSignal)
        {
            if (selectStructureSignal.StructureData == null)
            {
                throw new Exception("No structureData when tried to select");
            }

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
            
            _structureData = selectStructureSignal.StructureData;
            
            SelectPlacementHandler(selectStructureSignal);
            
            _coroutine = StartCoroutine(_placementHandler._WaitForInput(_input, _structureData, _placementValidator));
        }

        private void SelectPlacementHandler(SelectStructureSignal selectStructureSignal)
        {
            _placementHandler = selectStructureSignal.StructureData.StructureType switch
            {
                StructureType.Structure => _structurePlacementHandler,
                StructureType.Road => _roadPlacementHandler,
                
                
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _placementValidator = selectStructureSignal.StructureData.StructureType switch
            {
                StructureType.Structure => _structurePlacementValidator,
                
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void OnDisable()
        {
            _input.DisablePlayerActions();
        }
    }
}