using System;
using System.Collections.Generic;
using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using _Scripts.Managers.Data;
using _Scripts.Zenject.Signals;

using UnityEngine;

namespace _Scripts.Managers
{
    public class BuildingsManager : MonoBehaviour
    {
        private BuildingFactory _buildingFactory;
        
        public delegate void BuildingEventHandler(PolarNode polarNode, BuildingData buildingData);
        public event BuildingEventHandler OnBuildingConstructed;

        private List<Building> buildings = new List<Building>();

        public void ConstructBuilding()
        {
            // var newBuilding = _buildingFactory.CreateBuilding();
            // buildings.Add(newBuilding);
            //
            // OnBuildingConstructed?.Invoke(newBuilding);
        }

        public void DestroyBuilding(Building building)
        {
            buildings.Remove(building);
        }
        
        private void Awake()
        {
            _buildingFactory = new BuildingFactory();
        }

        public void OnBuildingBuiltSignal(BuildNewBuildingSignal buildNewBuildingSignal)
        {
            Debug.Log($"Building built");
            //Debug.Log($"Building built: {buildingBuiltSignal.Building.Name}");
        }
    }
}