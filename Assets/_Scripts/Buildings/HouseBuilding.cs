using _Scripts.Zenject.Signals;

using UnityEngine;

using Zenject;

namespace _Scripts.Buildings
{
    public class HouseBuilding : Building
    {
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        public override void OnBuild()
        {
            //_signalBus.Fire(new BuildingBuiltSignal(this));
        }
    }
}