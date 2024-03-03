using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Zenject.Installers;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    [RequireComponent(typeof(Button))]
    public class StructureSelectionButton : MonoBehaviour
    {
        [SerializeField]
        public BaseStructureData _structureData;
        
        private Button _button;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        protected void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_structureData == null)
            {
                Debug.Log($"Button: {name}, has no building data");
            }

            _button.onClick.AddListener(SelectBuildingData);
        }

        private void SelectBuildingData()
        {
            _signalBus.Fire(new StructureSelectedSignal(_structureData));
        }
    }
}