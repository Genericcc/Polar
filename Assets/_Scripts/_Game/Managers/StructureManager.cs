using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Grid;
using _Scripts._Game.Managers.PlacementValidators;
using _Scripts._Game.Structures;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;
using _Scripts.Zenject.Installers;
using _Scripts.Zenject.Signals;

using com.cyborgAssets.inspectorButtonPro;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers
{
    public class StructureManager : MonoBehaviour
    {
        private SignalBus _signalBus;
        private StructureFactory _structureFactory;

        private Entity _entity;
        private World _world;
        private bool _wasRun;

        //private List<Structure> _structures;
        
        [Header("Testing")]
        public int testStructuresAmount;
        public BaseStructureData testStructureData;
        
        private StructureDictionary _structureDictionary;

        private IPlacementValidator _placementValidator;

        [Inject]
        public void Construct(SignalBus signalBus, 
            PolarGridManager polarGridManager, 
            //StructureFactory structureFactory,
            StructureDictionary structureDictionary)
        {
            _signalBus = signalBus;
            //_structureFactory = structureFactory;
            _structureDictionary = structureDictionary;
            
            _world = World.DefaultGameObjectInjectionWorld;
            
            if(_world.IsCreated && !_world.EntityManager.Exists(_entity))
            {
                _entity = _world.EntityManager.CreateEntity(typeof(StructureManagerTag));

                _world.EntityManager.AddBuffer<StructureWaypointBuffer>(_entity);
                _world.EntityManager.AddBuffer<StructurePlacementOrder>(_entity);
            }
        }

        private void Start()
        {
            //_structures = new List<Structure>();

            // _selectedStructureData = testStructureData;
            //
            // TestPlaceBuildings(testStructuresAmount);
            
            _signalBus.Fire(new SelectStructureSignal(testStructureData));
        }

        private void LateUpdate()
        {
            if (_wasRun)
            {
                return;
            }

            _wasRun = true;
            
            // if (_structures.Any())
            // {
            //     _world.EntityManager.AddComponent<SomethingBuiltTag>(_entity);
            // }
        }

        public void OnRequestBuildingPlacementSignal(RequestStructurePlacementSignal requestStructurePlacementSignal)
        {
            ConstructBuilding(
                requestStructurePlacementSignal.Nodes, 
                requestStructurePlacementSignal.StructureData,
                requestStructurePlacementSignal.LocalTransform
                );
        }

        private void ConstructBuilding(
            List<PolarNode> buildingNodes, IStructureData structureData, LocalTransform localTransform)
        {
            //var structure = _structureFactory.Create(buildingNodes, structureData);
            //_structures.Add(structure);

            foreach (var polarNode in buildingNodes)
            {
                polarNode.SetBuilding(structureData);
            }

            _world.EntityManager.GetBuffer<StructurePlacementOrder>(_entity)
                  .Add(new StructurePlacementOrder 
                  { 
                      StructureIndex = 0,
                      NewTransform = localTransform,
                  });


            _world.EntityManager
                  .GetBuffer<StructureWaypointBuffer>(_entity)
                  .Add(new StructureWaypointBuffer 
                  { 
                      Position = localTransform.Position,
                  });
        }


        // [ProButton]
        // public void TestPlaceBuildings(int numberOfBuildings)
        // {
        //     for (var i = 0; i < numberOfBuildings; i++)
        //     {
        //         _signalBus.Fire(new RequestStructurePlacementSignal(_polarGridManager.GetRandomNode()));
        //     }
        // }
    }
}