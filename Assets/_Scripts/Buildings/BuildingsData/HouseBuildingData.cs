using UnityEngine;

namespace _Scripts.Buildings.BuildingsData
{
    [CreateAssetMenu(fileName = "HouseBuildingData", menuName = "Data/BuildingData/HouseBuildingData", order = 1)]
    public class HouseBuildingData : BuildingData
    {
        public int workers;
    }
}