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

        private Entity _entity;
        private World _world;
        private bool _wasRun;

        [Header("Testing")]
        public int testStructuresAmount;
        public BaseStructureData testStructureData;
        
        private IPlacementValidator _placementValidator;
        private PolarGridManager _polarGridManager;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            
            _world = World.DefaultGameObjectInjectionWorld;
            
            if(_world.IsCreated && !_world.EntityManager.Exists(_entity))
            {
                _entity = _world.EntityManager.CreateEntity(typeof(StructureManagerTag));

                _world.EntityManager.AddBuffer<StructurePlacementOrder>(_entity);
                _world.EntityManager.AddBuffer<PeopleSpawnOrder>(_entity);
                _world.EntityManager.AddBuffer<StructureWaypointBuffer>(_entity);
            }

            //TestPlaceBuildings(testStructuresAmount);
        }
        
        [ProButton]
        public void TestPlaceBuildings(int testStructuresAmount)
        {
            for (var i = 0; i < testStructuresAmount; i++)
            {
                var nodes = new List<PolarNode>();
                nodes.Add(_polarGridManager.GetRandomNode());

                var lt = new LocalTransform();
                lt.Position = nodes[0].CentrePosition;
                
                _signalBus.Fire(new RequestStructurePlacementSignal(nodes, testStructureData, lt));
            }
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
            foreach (var polarNode in buildingNodes)
            {
                polarNode.SetBuilding(structureData);
            }

            _world.EntityManager
                  .GetBuffer<StructurePlacementOrder>(_entity)
                  .Add(new StructurePlacementOrder 
                  { 
                      StructureId = structureData.ID,
                      NewTransform = localTransform,
                  });

            _world.EntityManager
                  .GetBuffer<PeopleSpawnOrder>(_entity)
                  .Add(new PeopleSpawnOrder 
                  { 
                      PeopleAmount = structureData.Inhabitants,
                      SpawnTransform = localTransform,
                  });
            
            _world.EntityManager
                  .GetBuffer<StructureWaypointBuffer>(_entity)
                  .Add(new StructureWaypointBuffer 
                  { 
                      Position = localTransform.Position,
                  });
        }
    }
}