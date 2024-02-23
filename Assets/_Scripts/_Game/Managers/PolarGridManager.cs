using System.Collections.Generic;
using System.Linq;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using UnityEngine;

using Zenject;

namespace _Scripts._Game.Managers
{
    public class PolarGridManager : MonoBehaviour
    {
        [SerializeField]
        [Range(0.5f, 5f)]
        private float columnHeight = 2;

        [InspectorButton("CreateGrid")]
        public bool rebuild;
        
        private PolarNodeFactory _polarNodeFactory;
        
        [SerializeField]
        private PolarGridRingsSettings polarGridRingsSettings;

        private PolarGrid _polarGrid;
        private RingFactory _ringFactory;

        public bool Initalised { get; set; }
        
        [Inject]
        public void Construct(PolarGridRingsSettings injectedPolarGridRingsSettings,
            RingFactory ringFactory,
            PolarNodeFactory polarNodeFactory)
        {
            _ringFactory = ringFactory;
            _polarNodeFactory = polarNodeFactory;

            if (polarGridRingsSettings == null)
            {
                polarGridRingsSettings = injectedPolarGridRingsSettings;
            }
            
            CreateGrid();
        }

        private void CreateGrid()
        {
            ClearGrid();
            
            _polarGrid = new PolarGrid(polarGridRingsSettings, columnHeight);
            _polarGrid.PopulateGrid(_polarNodeFactory, _ringFactory);

            if (_polarGrid.Rings.Any())
            {
                foreach (var ring in _polarGrid.Rings)
                {
                    var height = ring.Nodes.First().PolarGridPosition.H;
                    
                    var newPos = new Vector3(ring.transform.position.x, height, ring.transform.position.z);
                    var newRot = new Vector3(90, 0, 0);
                    
                    ring.transform.SetPositionAndRotation(newPos, Quaternion.Euler(newRot));
                }
            }

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
        
        public bool TryGetNodesForBuilding(PolarNode originNode, StructureSizeType spaceOccupationType, out List<PolarNode> nodes)
        {
            nodes = new List<PolarNode>();

            (int, int) searchRange = spaceOccupationType switch
            {
                StructureSizeType.Size1X1 => (1, 1),
                StructureSizeType.Size1X2 => (1, 2),
                StructureSizeType.Size2X1 => (2, 1),
                StructureSizeType.Size2X2 => (2, 2),
                StructureSizeType.Size2X3 => (2, 3),
                StructureSizeType.Size3X2 => (3, 2),
                StructureSizeType.Size3X3 => (3, 3),
                _ => (69, 69)
            };

            if (_polarGrid.TryGetNodesForBuilding(originNode, searchRange, out var results))
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
            return _polarGrid.GetNodePolarPositionAt(worldPosition);
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