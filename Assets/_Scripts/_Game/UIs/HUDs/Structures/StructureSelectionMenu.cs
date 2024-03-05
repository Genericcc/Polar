using System;

using _Scripts._Game.Managers;
using _Scripts.Zenject.Installers;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    public class StructureSelectionMenu : MonoBehaviour
    {
        private InputReader _inputReader;
        private RectTransform _visualTransform;
        private UIAnimator _uiAnimator;

        [Inject]
        public void Construct(InputReader inputReader)
        {
            _inputReader = inputReader;
            _visualTransform = transform.GetChild(0) as RectTransform; 

            _uiAnimator = GetComponent<UIAnimator>();
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