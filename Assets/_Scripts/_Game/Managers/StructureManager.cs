using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.DOTS.Components.Buffers;
using _Scripts._Game.DOTS.Components.Tags;
using _Scripts._Game.Grid;
using _Scripts._Game.Structures;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;
using _Scripts.Zenject.Installers;
using _Scripts.Zenject.Signals;

using com.cyborgAssets.inspectorButtonPro;

using Unity.Entities;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers
{
    public class StructureManager : MonoBehaviour
    {
        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;
        private StructureFactory _structureFactory;

        private Entity _entity;
        private World _world;
        private bool _wasRun;

        private List<Structure> _structures;
        
        [Header("Testing")]
        public int testStructuresAmount;
        public BaseStructureData testStructureData;
        
        private IStructureData _selectedStructureData;
        private StructureDictionary _structureDictionary;

        [Inject]
        public void Construct(SignalBus signalBus, 
            PolarGridManager polarGridManager, 
            StructureFactory structureFactory,
            StructureDictionary structureDictionary)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _structureFactory = structureFactory;
            _structureDictionary = structureDictionary;
            
            _world = World.DefaultGameObjectInjectionWorld;
            
            if(_world.IsCreated && !_world.EntityManager.Exists(_entity))
            {
                _entity = _world.EntityManager.CreateEntity(typeof(StructureManagerTag));

                _world.EntityManager.AddBuffer<StructureWaypointBuffer>(_entity);
            }
        }

        private void Start()
        {
            _structures = new List<Structure>();
            
            TestPlaceBuildings(testStructuresAmount, testStructureData);
        }

        private void LateUpdate()
        {
            if (_wasRun)
            {
                return;
            }

            _wasRun = true;
            
            if (_structures.Any())
            {
                _world.EntityManager.AddComponent<SomethingBuiltTag>(_entity);
            }
        }

        public void OnRequestBuildingPlacementSignal(RequestStructurePlacementSignal requestStructurePlacementSignal)
        {
            if (_selectedStructureData == null)
            {
                return;
            }
            
            var originBuildNode = requestStructurePlacementSignal.OriginBuildNode;
            var buildingSize = _selectedStructureData.StructureSizeType; //requestBuildingPlacementSignal.BuildingData.buildingSizeType;

            if (!_polarGridManager.TryGetNodesForBuilding(originBuildNode, buildingSize, out var nodesToBuildOn))
            {
                return;
            }

            if (!CanBuildOnNodes(nodesToBuildOn))
            {
                return;
            }
            
            ConstructBuilding(nodesToBuildOn, _selectedStructureData);
        }

        private bool CanBuildOnNodes(IEnumerable<PolarNode> buildingNodes)
        {
            if (buildingNodes.All(x => x.IsFree))
            {
                return true;
            }

            return false;
        }

        private void ConstructBuilding(List<PolarNode> buildingNodes, IStructureData structureData)
        {
            var newBuilding = _structureFactory.Create(buildingNodes, structureData);
            _structures.Add(newBuilding);

            foreach (var polarNode in buildingNodes)
            {
                polarNode.SetBuilding(newBuilding);
            }

            newBuilding.OnBuild();

            _world.EntityManager
                  .GetBuffer<StructureWaypointBuffer>(_entity)
                  .Add(new StructureWaypointBuffer 
                  { 
                      Position = newBuilding.transform.position,
                  });
        }

        //[ProButton]
        public void TestPlaceBuildings(int numberOfBuildings, IStructureData structureData)
        {
            _selectedStructureData = structureData;
            
            for (var i = 0; i < numberOfBuildings; i++)
            {
                _signalBus.Fire(new RequestStructurePlacementSignal(_polarGridManager.GetRandomNode()));
            }
        }

        public void OnSelectStructureToBuild(SelectStructureSignal selectStructureSignal)
        {
            _selectedStructureData = selectStructureSignal.StructureData;
        }
    }
}