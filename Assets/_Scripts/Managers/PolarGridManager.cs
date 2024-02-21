using System.Collections.Generic;

using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;

using UnityEngine;

using Zenject;

namespace _Scripts.Managers
{
    public class PolarGridManager : MonoBehaviour
    {
        [SerializeField]
        [Range(0.1f, 5f)]
        private float columnHeight;

        [InspectorButton("CreateGrid")]
        public bool rebuild;
        
        private PolarNodeFactory _polarNodeFactory;
        
        [SerializeField]
        private PolarGirdRingsSettings polarGirdRingsSettings;

        private PolarGrid _polarGrid;

        public bool Initalised { get; set; }
        
        [Inject]
        public void Construct(PolarNodeFactory polarNodeFactory, PolarGirdRingsSettings injectedPolarGirdRingsSettings)
        {
            _polarNodeFactory = polarNodeFactory;

            if (polarGirdRingsSettings == null)
            {
                polarGirdRingsSettings = injectedPolarGirdRingsSettings;
            }
            
            CreateGrid();
        }

        private void CreateGrid()
        {
            ClearGrid();
            
            _polarGrid = new PolarGrid(polarGirdRingsSettings, columnHeight);
            _polarGrid.Populate(_polarNodeFactory);

            Initalised = true;
        }

        private void ClearGrid()
        {
            if (_polarGrid != null)
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

            if (_polarGrid.TryGetNodesForBuilding(originNode, checkShifts, out var results))
            {
                nodes = results;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public Vector3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            return _polarGrid.GetWorldFromPolar(polarGridPosition);
        }

        public PolarGridPosition GetPolarFromWorld(Vector3 worldPosition)
        {
            return _polarGrid.GetPolarFromWorld(worldPosition);
        }

        public PolarNode GetRandomNode()
        {
            return _polarGrid.GetRandom();
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