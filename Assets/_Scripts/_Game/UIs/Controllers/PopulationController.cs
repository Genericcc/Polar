using _Scripts._Game.Managers;

using UnityEngine.UIElements;

namespace _Scripts._Game.UIs.Controllers
{
    public class PopulationController : IHudController
    {
        private readonly DataQueryManager _statsProvider;

        private Label _valueLabel;

        public PopulationController(DataQueryManager statsProvider)
        {
            _statsProvider = statsProvider;
        }

        public void Bind(VisualElement root)
        {
            _valueLabel = root.Q<Label>("population-value");
            _valueLabel.text = _statsProvider.CurrentPopulation.ToString();
            _statsProvider.PopulationChanged += OnPopulationChanged;
        }

        private void OnPopulationChanged(int population)
        {
            _valueLabel.text = population.ToString();
        }

        public void Dispose()
        {
            _statsProvider.PopulationChanged -= OnPopulationChanged;
        }
    }
}
