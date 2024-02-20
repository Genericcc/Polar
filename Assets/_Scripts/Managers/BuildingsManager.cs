using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using _Scripts.Zenject.Signals;

using com.cyborgAssets.inspectorButtonPro;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class BuildingsManager : MonoBehaviour
    {
        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;
        private BuildingFactory _buildingFactory;

        public List<Building> buildings;

        public BuildingData testBuildingData;

        private Entity Entity;
        private World World;
        private bool wasRun;

        public int numberOfBuildings;

        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager, BuildingFactory buildingFactory)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _buildingFactory = buildingFactory;
            
            buildings = new List<Building>();
            
            World = World.DefaultGameObjectInjectionWorld;
            
            if(World.IsCreated && !World.EntityManager.Exists(Entity))
            {
                Entity = World.EntityManager.CreateEntity(typeof(BuildingManagerTag));

                World.EntityManager.AddBuffer<BuildingPositionBuffer>(Entity);
            }
        }

        private void Start()
        {
            TestPlaceBuildings(numberOfBuildings, testBuildingData);
        }

        private void LateUpdate()
        {
            if (wasRun)
            {
                return;
            }

            wasRun = true;
            
            World.EntityManager.AddComponent<SomethingBuiltTag>(Entity);
        }

        public void OnRequestBuildingPlacementSignal(RequestBuildingPlacementSignal requestBuildingPlacementSignal)
        {
            var originBuildNode = requestBuildingPlacementSignal.OriginBuildNode;
            var buildingSize = BuildingSizeType.Size2X2; //requestBuildingPlacementSignal.BuildingData.buildingSizeType;

            if (!_polarGridManager.TryGetNodesForBuilding(originBuildNode, buildingSize, out var nodesToBuildOn))
            {
                return;
            }

            if (!CanBuildOnNodes(nodesToBuildOn))
            {
                return;
            }
            
            ConstructBuilding(nodesToBuildOn, requestBuildingPlacementSignal.BuildingData);
        }

        private bool CanBuildOnNodes(IEnumerable<PolarNode> buildingNodes)
        {
            if (buildingNodes.All(x => x.IsFree))
            {
                return true;
            }

            return false;
        }

        private void ConstructBuilding(List<PolarNode> buildingNodes, BuildingData buildingData)
        {
            var newBuilding = _buildingFactory.Create(buildingNodes, buildingData);
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
        public void TestPlaceBuildings(int numberOfBuildings, BuildingData buildingData)
        {
            for (var i = 0; i < numberOfBuildings; i++)
            {
                _signalBus.Fire(new RequestBuildingPlacementSignal(buildingData, _polarGridManager.GetRandomNode()));
            }
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