using System;

using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;

using UnityEditor.UI;

using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Game.UIs.HUDs.Structures
{
    [RequireComponent(typeof(Button))]
    public class StructureSelectionButton : MonoBehaviour
    {
        [SerializeField]
        private StructureData structureData;
        
        [SerializeField]
        public StructureManager structureManager;
        
        [SerializeField]
        private Button button;

        protected void Awake()
        {
            if (structureManager == null)
            {
                structureManager = FindObjectOfType<StructureManager>();
            }
            
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            button.onClick.AddListener(SelectBuildingData);
        }

        private void SelectBuildingData()
        {
            structureManager.SelectStructureToBuild(structureData);
        }
    }
}