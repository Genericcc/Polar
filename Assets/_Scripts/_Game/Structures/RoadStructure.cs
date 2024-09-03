using Zenject;

namespace _Scripts._Game.Structures
{
    public class RoadStructure : Structure
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