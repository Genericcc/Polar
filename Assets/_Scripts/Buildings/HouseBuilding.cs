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
        }

        public override void OnDemolish()
        {
        }
    }
}