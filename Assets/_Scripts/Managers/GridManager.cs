using System.Collections.Generic;
using _Scripts.Grid;
using UnityEngine;

namespace _Scripts.Managers
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [SerializeField] private Transform gridDebugObjectPrefab;
        [SerializeField] private int maxHeight = 5;
        [SerializeField] private int segmentsInGame = 4;
        [SerializeField] private Vector2 cellSize = new (5, 5);
        [SerializeField] private int fi = 30;
        
        [InspectorButton("CreateGrid")]
        public bool createGrid;
        
        [InspectorButton("ClearGrid")]
        public bool clearGrid;
        
        [SerializeField]
        public PolarGirdRingsSettings polarGirdRingsSettings;

        private PolarGridSystem _polarGridSystem;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There's more than one LevelGrid! " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateGrid();
        }

        private void CreateGrid()
        {
            ClearGrid();
            
            _polarGridSystem = new PolarGridSystem(this, (cellSize.x, cellSize.y), fi, maxHeight, segmentsInGame);
            _polarGridSystem.CreateDebugObjects(gridDebugObjectPrefab);
        }

        private void ClearGrid()
        {
            if (_polarGridSystem != null)
            {
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        public void AddUnitAtGridPosition(PolarGridPosition polarGridPosition, Unit unit)
        {
            PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);
        }

        public List<Unit> GetUnitListAtGridPosition(PolarGridPosition polarGridPosition)
        {
            PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);

            return null;
        }

        public void RemoveUnitAtGridPosition(PolarGridPosition polarGridPosition, Unit unit)
        {
            PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);
        }

        public void UnitMovedGridPosition(Unit unit, PolarGridPosition fromPolarGridPosition, PolarGridPosition toPolarGridPosition)
        {
            RemoveUnitAtGridPosition(fromPolarGridPosition, unit);

            AddUnitAtGridPosition(toPolarGridPosition, unit);
        }

        //public PolarGridPosition GetGridPosition(Vector3 worldPosition) => _polarGridSystem.GetPolarPosition(worldPosition);

    }
}