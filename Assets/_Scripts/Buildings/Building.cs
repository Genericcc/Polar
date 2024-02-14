using _Scripts.Buildings.BuildingsData;

using UnityEngine;

namespace _Scripts.Buildings
{
    public abstract class Building : MonoBehaviour
    {
        public BuildingData buildingData;

        public string Name => buildingData.ToString();
        
        public abstract void OnBuild();
    }
}