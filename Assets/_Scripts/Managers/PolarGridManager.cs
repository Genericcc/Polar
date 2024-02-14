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
        [SerializeField] private Transform gridDebugObjectPrefab;
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

        public List<PolarNode> GetNeighbours(PolarNode startingNode, BuildingNodesOccupationType spaceOccupationType)
        {
            var neighbours = new List<PolarNode>();
            
            switch (spaceOccupationType)
            {
                case BuildingNodesOccupationType.Space2X2:
                    neighbours = _polarGridSystem.GetNodesForBuilding(startingNode, 2, 2);
                    break;

                case BuildingNodesOccupationType.Space2X3:
                    break;

                case BuildingNodesOccupationType.Space3X2:
                    break;

                case BuildingNodesOccupationType.Space3X3:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(spaceOccupationType), spaceOccupationType, null);
            }

            return neighbours;
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