using System.Collections.Generic;
using _Scripts.Grid;
using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Transform gridDebugObjectPrefab;
        [SerializeField] private Vector2 cellSize = new (5, 5);
        [SerializeField] private float densityFactor = 0f;
        
        private PolarNodeFactory _polarNodeFactory;
        private PolarGirdRingsSettings _polarGirdRingsSettings;
        
        public PolarGridSystem PolarGridSystem;
        
        [Inject]
        public void Construct(PolarNodeFactory polarNodeFactory, PolarGirdRingsSettings polarGirdRingsSettings)
        {
            _polarNodeFactory = polarNodeFactory;
            _polarGirdRingsSettings = polarGirdRingsSettings;
            
            CreateGrid();
        }

        private void CreateGrid()
        {
            ClearGrid();
            
            PolarGridSystem = new PolarGridSystem(_polarGirdRingsSettings, (cellSize.x, cellSize.y), densityFactor, _polarNodeFactory);
        }

        private void ClearGrid()
        {
            if (PolarGridSystem != null)
            {
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        // public void AddUnitAtGridPosition(PolarGridPosition polarGridPosition, Unit unit)
        // {
        //     PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);
        // }
        //
        // public List<Unit> GetUnitListAtGridPosition(PolarGridPosition polarGridPosition)
        // {
        //     PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);
        //
        //     return null;
        // }
        //
        // public void RemoveUnitAtGridPosition(PolarGridPosition polarGridPosition, Unit unit)
        // {
        //     PolarNode polarNode = _polarGridSystem.GetGridObject(polarGridPosition);
        // }
        //
        // public void UnitMovedGridPosition(Unit unit, PolarGridPosition fromPolarGridPosition, PolarGridPosition toPolarGridPosition)
        // {
        //     RemoveUnitAtGridPosition(fromPolarGridPosition, unit);
        //
        //     AddUnitAtGridPosition(toPolarGridPosition, unit);
        // }

        //public PolarGridPosition GetGridPosition(Vector3 worldPosition) => _polarGridSystem.GetPolarPosition(worldPosition);

    }
}