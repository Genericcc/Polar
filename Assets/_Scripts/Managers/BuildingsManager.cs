using System;
using System.Collections.Generic;
using _Scripts.Buildings;
using _Scripts.Grid;
using _Scripts.Managers.Data;
using UnityEngine;

namespace _Scripts.Managers
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance { get; private set; }
        
        public AssetReference assetReference;
        private BuildingFactory _buildingFactory;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There's more than one BuildingsManager! " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            assetReference = FindObjectOfType<AssetReference>();
            _buildingFactory = new BuildingFactory();
        }

        private void Start()
        {
            CreateStartingBuildings();
        }

        private void CreateStartingBuildings()
        {
            //_buildingFactory.CreateBuilding()
        }
    }
}