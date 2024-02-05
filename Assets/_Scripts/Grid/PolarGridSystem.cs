using System.Collections.Generic;
using System.Linq;

using _Scripts.Managers;

using UnityEngine;

namespace _Scripts.Grid
{
    public class PolarGridSystem
    {
        private int _maxHeight;
        private int _segmentsOutOfSix;
        
        private float _fi;
        private (float x, float y) _cellSize;
        
        private List<PolarNode> _gridNodes;
        private List<PolarGridDebugObject> _debugObjectList;
        
        private readonly GridManager _gridManager;

        public PolarGridSystem(
            GridManager gridManager, (float, float) cellSize, float fiConst, int maxHeight, int segmentsOutOfSix)
        {
            _gridManager = gridManager;
            
            _maxHeight = maxHeight;
            _segmentsOutOfSix = segmentsOutOfSix;
            
            _fi = fiConst;
            _cellSize = cellSize;

            _gridNodes = new List <PolarNode>();
            _debugObjectList = new List<PolarGridDebugObject>();

            for (var r = 0; r < _gridManager.polarGirdRingsSettings.ringSettingsList.Count; r++)
            {
                var thisRingSetting = _gridManager.polarGirdRingsSettings.ringSettingsList[r];
                var height = thisRingSetting.height;
                
                if (r == 0)
                {
                    var polarGridPosition = new PolarGridPosition(r, 0, 0, height);
                    var node = new PolarNode(this, r, polarGridPosition);

                    _gridNodes.Add(node);
                    
                    continue;
                }

                for (var fieldIndex = 0; fieldIndex < thisRingSetting.maxFields; fieldIndex++)
                {
                    for (var fi = 360 - _segmentsOutOfSix * 60f; fi < 360; fi += _fi)
                    {
                        var polarGridPosition = new PolarGridPosition(r, fieldIndex, fi, height);
                        var node = new PolarNode(this, r, polarGridPosition);

                        _gridNodes.Add(node);
                    }
                }
            }
        }

        public Vector3 GetWorldPosition(PolarGridPosition polarGridPosition)
        {
            var previousFieldsCount = GetSumOfPreviousFields(polarGridPosition);

            var x = (polarGridPosition.R + previousFieldsCount) * Mathf.Cos(polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.R + previousFieldsCount) * Mathf.Sin(polarGridPosition.Fi * Mathf.Deg2Rad);

            return new Vector3(x * _cellSize.x, y, z * _cellSize.y);
        }

        private int GetSumOfPreviousFields(PolarGridPosition polarGridPosition)
        {
            var fields = 0;

            if (polarGridPosition.Ring == 0)
            {
                return 0;
            }
            
            for (var r = 1; r < _gridManager.polarGirdRingsSettings.ringSettingsList.Count; r++)
            {
                if (r <= polarGridPosition.Ring)
                {
                    fields += _gridManager.polarGirdRingsSettings.ringSettingsList[r - 1].maxFields;
                }
            }
            
            return fields;
        }

        // public PolarGridPosition GetPolarPosition(Vector3 worldPosition)
        // {
        //     var r = Mathf.RoundToInt(Vector3.Magnitude(worldPosition));
        //     var fi = Mathf.Atan2(worldPosition.x, worldPosition.z);
        //     var h = 1; //_maxHeight - r;
        //
        //     return new PolarGridPosition(r, fi, h);
        // }

        public void CreateDebugObjects(Transform debugPrefab)
        {
            foreach (var node in _gridNodes)
            {
                var debugTransform = Object.Instantiate(
                    debugPrefab,
                    GetWorldPosition(node.PolarGridPosition),
                    Quaternion.identity);
                
                var polarGridDebugObject = debugTransform.GetComponent<PolarGridDebugObject>();
                
                polarGridDebugObject.AssignPolarNode(node);
                polarGridDebugObject.transform.SetParent(_gridManager.transform);

                var position = polarGridDebugObject.transform.position;
                polarGridDebugObject.transform.rotation =
                    Quaternion.LookRotation(position - new Vector3(0, position.y, 0));

                _debugObjectList.Add(polarGridDebugObject);
            }
        }

        public PolarNode GetGridObject(PolarGridPosition polarGridPosition)
        {
            return _gridNodes.FirstOrDefault(x => x.PolarGridPosition == polarGridPosition);
        }
    }
}