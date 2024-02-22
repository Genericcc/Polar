using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using _Scripts.Data.Dictionaries;
using _Scripts.Grid;
using _Scripts.Structures;
using _Scripts.Structures.StructuresData;
using _Scripts.Zenject.Signals;

using com.cyborgAssets.inspectorButtonPro;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class StructureManager : MonoBehaviour
    {
        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;
        private StructureFactory _structureFactory;

        public List<Structure> buildings;

        public StructureData testStructureData;

        private Entity Entity;
        private World World;
        private bool wasRun;

        public int numberOfBuildings;
        [SerializeField]
        private StructureData selectedStructureData;

        private StructureDictionary _structureDictionary;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager, StructureFactory structureFactory,
            StructureDictionary structureDictionary)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _structureFactory = structureFactory;
            _structureDictionary = structureDictionary;
            
            buildings = new List<Structure>();
            
            World = World.DefaultGameObjectInjectionWorld;
            
            if(World.IsCreated && !World.EntityManager.Exists(Entity))
            {
                Entity = World.EntityManager.CreateEntity(typeof(BuildingManagerTag));

                World.EntityManager.AddBuffer<BuildingPositionBuffer>(Entity);
            }
        }

        private void Start()
        {
            TestPlaceBuildings(numberOfBuildings, testStructureData);
        }

        private void LateUpdate()
        {
            if (wasRun)
            {
                return;
            }

            wasRun = true;
            
            if (buildings.Any())
            {
                World.EntityManager.AddComponent<SomethingBuiltTag>(Entity);
            }
        }

        public void OnRequestBuildingPlacementSignal(RequestBuildingPlacementSignal requestBuildingPlacementSignal)
        {
            var originBuildNode = requestBuildingPlacementSignal.OriginBuildNode;
            var buildingSize = StructureSizeType.Size2X2; //requestBuildingPlacementSignal.BuildingData.buildingSizeType;

            if (!_polarGridManager.TryGetNodesForBuilding(originBuildNode, buildingSize, out var nodesToBuildOn))
            {
                return;
            }

            if (!CanBuildOnNodes(nodesToBuildOn))
            {
                return;
            }
            
            ConstructBuilding(nodesToBuildOn, requestBuildingPlacementSignal.StructureData);
        }

        private bool CanBuildOnNodes(IEnumerable<PolarNode> buildingNodes)
        {
            if (buildingNodes.All(x => x.IsFree))
            {
                return true;
            }

            return false;
        }

        private void ConstructBuilding(List<PolarNode> buildingNodes, StructureData structureData)
        {
            var newBuilding = _structureFactory.Create(buildingNodes, structureData);
            buildings.Add(newBuilding);

            foreach (var polarNode in buildingNodes)
            {
                polarNode.SetBuilding(newBuilding);
            }
            
            newBuilding.OnBuild();
            
            //DOTS testing
            World.EntityManager.GetBuffer<BuildingPositionBuffer>(Entity).Add(new BuildingPositionBuffer
            {
                Value = newBuilding.transform.position, 
            });
        }

        [ProButton]
        public void TestPlaceBuildings(int numberOfBuildings, StructureData structureData)
        {
            for (var i = 0; i < numberOfBuildings; i++)
            {
                _signalBus.Fire(new RequestBuildingPlacementSignal(structureData, _polarGridManager.GetRandomNode()));
            }
        }

        public void Select(StructureType structureType)
        {
            var selectedStructure = _structureDictionary.Get(structureType);
            
        }
    }
    
    public struct BuildingPositionBuffer : IBufferElementData
    {
        public float3 Value;
    }
    
    public struct BuildingManagerTag : IComponentData
    {
    }
    
    public struct SomethingBuiltTag : IComponentData
    {
    }
}