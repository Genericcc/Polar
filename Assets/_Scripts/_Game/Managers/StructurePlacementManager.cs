using System;
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
    public class StructurePlacementManager : MonoBehaviour
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
            _input.MouseClicked += OnMouseClicked;
            _input.EnablePlayerActions();

            mainCamera = mainCamera == null ? Camera.main : mainCamera;
        }

        private void OnMouseClicked()
        {
            var node = _placementHandler.GetNode(_mouseWorld.MousePos);
            if (node == null)
            {
                return;
            }

            if (!_polarGridManager.TryGetNodesForStructure(node, _structureData.StructureSizeType, out var nodesToBuildOn))
            {
                return; 
            }

            if (!_placementValidator.Validate(nodesToBuildOn, _structureData))
            {
                return;
            }

            var newTransform = _placementHandler.GetBuildTransform(nodesToBuildOn, _structureData);
            
            _signalBus.Fire(new RequestStructurePlacementSignal(nodesToBuildOn, _structureData, newTransform));
        }

        public void OnSelectStructureSignal(SelectStructureSignal selectStructureSignal)
        {
            SelectPlacementHandler(selectStructureSignal);
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

            _structureData = selectStructureSignal.StructureData;
        }

        private void OnDisable()
        {
            _input.MouseClicked -= OnMouseClicked;
            _input.DisablePlayerActions();
        }
    }
}