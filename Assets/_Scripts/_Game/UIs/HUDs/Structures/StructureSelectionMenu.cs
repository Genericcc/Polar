using _Scripts._Game.Managers;
using _Scripts.Data.Dictionaries;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    public class StructureSelectionMenu : MonoBehaviour
    {
        [SerializeField]
        private Transform buttonsParent;

        private InputReader _inputReader;
        private RectTransform _visualTransform;
        private UIAnimator _uiAnimator;

        private StructureDictionary _structureDictionary;
        private StructureButtonFactory _buttonFactory;

        [Inject]
        public void Construct(
            InputReader inputReader,
            StructureDictionary structureDictionary,
            StructureButtonFactory buttonFactory)
        {
            _inputReader = inputReader;
            _structureDictionary = structureDictionary;
            _buttonFactory = buttonFactory;

            _visualTransform = transform.GetChild(0) as RectTransform;

            _uiAnimator = GetComponent<UIAnimator>();
        }

        // Start, not Construct - Zenject injects scene objects between Awake and Start,
        // so this guarantees the factory is there
        private void Start()
        {
            SpawnButtons();
        }

        private void SpawnButtons()
        {
            var parent = buttonsParent != null ? buttonsParent : _visualTransform;

            foreach (var structure in _structureDictionary.structures)
            {
                if (structure == null)
                {
                    Debug.LogWarning($"{name}: StructureDictionary holds an empty entry, skipping it");
                    continue;
                }

                _buttonFactory.Create(structure, parent);
            }
        }

        private void OnEnable()
        {
            _inputReader.StructuresMenu += OnStructureSelectionSignal;
        }

        private void OnDisable()
        {
            _inputReader.StructuresMenu -= OnStructureSelectionSignal;
        }

        public void OnStructureSelectionSignal()
        {
            if (_uiAnimator != null)
            {
                _uiAnimator.HandleTweening(_visualTransform);
            }

            _visualTransform.gameObject.SetActive(!_visualTransform.gameObject.activeInHierarchy);
        }

        // Zenject alternative, because signals reach disabled GOs
        // public void OnToggleStructureMenuSignal()
        // {
        //     gameObject.SetActive(!gameObject.activeInHierarchy);
        // }
    }
}
