using Zenject;

namespace _Scripts._Game.Structures
{
    public class HouseStructure : Structure
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