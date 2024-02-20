using System;
using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class PolarGridManager : MonoBehaviour
    {
        [SerializeField] private Vector2 cellSize = new (5, 5);
        [SerializeField] private float densityFactor = 0f;
        
        private PolarNodeFactory _polarNodeFactory;
        private PolarGirdRingsSettings _polarGirdRingsSettings;

        private PolarGridSystem _polarGridSystem;
        
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
            
            _polarGridSystem = new PolarGridSystem(_polarGirdRingsSettings, (cellSize.x, cellSize.y), densityFactor, _polarNodeFactory);
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
        
        public bool TryGetNodesForBuilding(PolarNode originNode, BuildingSizeType spaceOccupationType, out List<PolarNode> nodes)
        {
            nodes = new List<PolarNode>();

            var checkShifts = spaceOccupationType switch
            {
                BuildingSizeType.Size2X2 => (2, 2),
                BuildingSizeType.Size2X3 => (2, 3),
                BuildingSizeType.Size3X2 => (3, 2),
                BuildingSizeType.Size3X3 => (3, 3),
                _ => (69, 69)
            };

            if (_polarGridSystem.TryGetNodesForBuilding(originNode, checkShifts, out var results))
            {
                nodes = results;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public Vector3 PolarToWorld(PolarGridPosition polarGridPosition)
        {
            return _polarGridSystem.PolarToWorld(polarGridPosition);
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

        public PolarNode GetRandomNode()
        {
            return _polarGridSystem.GetRandom();
        }
    }
}