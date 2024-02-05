using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using UnityEngine;

namespace _Scripts.Managers.Data
{
    [CreateAssetMenu(fileName = "AssetReference", menuName = "Data/AssetReference", order = 1)]
    public class AssetReference : ScriptableObject
    {
        public HouseBuilding houseBuilding;
        public HouseBuildingData houseBuildingData;
        
        
    }
}