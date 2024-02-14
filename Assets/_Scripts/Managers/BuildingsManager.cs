using System.Collections.Generic;
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

        public void OnBuildNewBuildingSignal(RequestBuildingPlacementSignal requestBuildingPlacementSignal)
        {
            ConstructBuilding(requestBuildingPlacementSignal.PolarNodes, requestBuildingPlacementSignal.BuildingData);
        }

        private void ConstructBuilding(List<PolarNode> polarNodes, BuildingData buildingData)
        {
            var newBuilding = _buildingFactory.Create(polarNodes, buildingData);
            _buildings.Add(newBuilding);

            foreach (var polarNode in polarNodes)
            {
                polarNode.SetBuilding(newBuilding);
            }
        }
    }
}