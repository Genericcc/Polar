using _Scripts.Zenject.Installers;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    [RequireComponent(typeof(Button))]
    public class StructureSelectionButton : BaseView
    {
        private int StructureId { get; set; } = -1;

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

            _button.onClick.AddListener(SelectBuildingData);
        }

        // Called by StructureSelectionMenu, which owns the StructureDictionary the ids come from
        public void Initialise(int structureId, string displayName)
        {
            StructureId = structureId;

            var label = GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = displayName;
            }

            name = $"StructureButton_{displayName}";
        }

        private void SelectBuildingData()
        {
            if (StructureId < 0)
            {
                Debug.LogWarning($"Button: {name}, was never initialised with a structure id");
                return;
            }

            _signalBus.Fire(new StructureSelectedSignal(StructureId));
        }
    }
}
