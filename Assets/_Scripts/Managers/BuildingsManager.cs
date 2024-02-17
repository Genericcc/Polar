using System.Collections.Generic;
using System.Linq;

using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using _Scripts.Zenject.Signals;

using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class BuildingsManager : MonoBehaviour
    {
        private SignalBus _signalBus;
        private PolarGridManager _polarGridManager;
        private BuildingFactory _buildingFactory;

        private List<Building> _buildings = new List<Building>();
        
        [Inject]
        public void Construct(SignalBus signalBus, PolarGridManager polarGridManager, BuildingFactory buildingFactory)
        {
            _signalBus = signalBus;
            _polarGridManager = polarGridManager;
            _buildingFactory = buildingFactory;
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
            _buildings.Add(newBuilding);

            foreach (var polarNode in buildingNodes)
            {
                polarNode.SetBuilding(newBuilding);
            }
        }
    }
}