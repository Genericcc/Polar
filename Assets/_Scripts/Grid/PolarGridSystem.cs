using System.Collections.Generic;
using System.Linq;

using _Scripts.Managers;

using UnityEngine;

using Zenject;

namespace _Scripts.Grid
{
    public class PolarGridSystem
    {
        public List<PolarNode> GridNodes;
        
        private readonly PolarGirdRingsSettings _polarGirdRingsSettings;
        private readonly float _densityFactor;
        private readonly (float x, float y) _cellSize;

        public PolarGridSystem(PolarGirdRingsSettings polarGirdRingsSettings, (float, float) cellSize, float densityFactor, 
            PolarNodeFactory polarNodeFactory)
        {
            _polarGirdRingsSettings = polarGirdRingsSettings;

            _densityFactor = densityFactor;
            _cellSize = cellSize;

            GridNodes = new List <PolarNode>();

            for (var ring = 0; ring < _polarGirdRingsSettings.ringSettingsList.Count; ring++)
            {
                var thisRingSetting = _polarGirdRingsSettings.ringSettingsList[ring];
                var height = thisRingSetting.height;
                var segmentsInGame = _polarGirdRingsSettings.segmentsInGame;
                
                //centreNode
                if (ring == 0)
                {
                    var polarGridPosition = new PolarGridPosition(0, 0, 0, height);
                    
                    var node = polarNodeFactory.Create(this, polarGridPosition);
                    GridNodes.Add(node);
                    
                    continue;
                }

                for (var fieldIndex = 0; fieldIndex < thisRingSetting.depth; fieldIndex++)
                {
                    for (var fi = 360 - segmentsInGame * 60f; fi < 360; fi += thisRingSetting.fi)
                    {
                        var polarGridPosition = new PolarGridPosition(ring, fieldIndex, fi, height);
                        
                        var node = polarNodeFactory.Create(this, polarGridPosition);
                        GridNodes.Add(node);
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

            return new Vector3(x * (_cellSize.x - _densityFactor), y, z * (_cellSize.y - _densityFactor));
        }

        private int GetSumOfPreviousFields(PolarGridPosition polarGridPosition)
        {
            var fields = 0;

            if (polarGridPosition.Ring == 0)
            {
                return 0;
            }
            
            for (var r = 1; r < _polarGirdRingsSettings.ringSettingsList.Count; r++)
            {
                if (r <= polarGridPosition.Ring)
                {
                    fields += _polarGirdRingsSettings.ringSettingsList[r - 1].depth;
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
            // foreach (var node in _gridNodes)
            // {
            //     PolarNode polarNode;
            //
            //     if (_polarNodeFactory == null)
            //     {
            //         var debugTransform = Object.Instantiate(
            //         debugPrefab,
            //         GetWorldPosition(node.PolarGridPosition),
            //         Quaternion.identity);
            //
            //         polarNode = debugTransform.GetComponent<PolarNode>();
            //     }
            //     else
            //     {
            //         polarNode = _polarNodeFactory.Create(this, new PolarGridPosition());
            //     }
            //
            //     var gridObjectTransform = polarNode.transform;
            //     gridObjectTransform.position = GetWorldPosition(node.PolarGridPosition);
            //     
            //     var position = gridObjectTransform.position;
            //     polarNode.transform.rotation = Quaternion.LookRotation(position - new Vector3(0, position.y, 0));
            //     
            //     polarNode.transform.SetParent(_gridManager.transform);
            //
            //     _gridNodes.Add(polarNode);
            //}
        }

        public PolarNode GetGridObject(PolarGridPosition polarGridPosition)
        {
            return GridNodes.FirstOrDefault(x => x.PolarGridPosition == polarGridPosition);
        }
    }
}