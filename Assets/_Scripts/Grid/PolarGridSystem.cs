using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;
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
                    
                    var node = polarNodeFactory.Create(this, polarGridPosition, thisRingSetting);
                    GridNodes.Add(node);
                    
                    continue;
                }

                for (var fieldIndex = 0; fieldIndex < thisRingSetting.depth; fieldIndex++)
                {
                    for (var fi = 360 - segmentsInGame * 60f; fi < 360; fi += thisRingSetting.fi)
                    {
                        var polarGridPosition = new PolarGridPosition(ring, fieldIndex, fi, height);
                        
                        var node = polarNodeFactory.Create(this, polarGridPosition, thisRingSetting);
                        GridNodes.Add(node);
                    }
                }
            }
        }

        public Vector3 PolarToWorld(PolarGridPosition polarGridPosition)
        {
            var previousFieldsCount = GetSumOfPreviousFields(polarGridPosition);

            var x = (polarGridPosition.D + previousFieldsCount) * Mathf.Cos(-polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.D + previousFieldsCount) * Mathf.Sin(-polarGridPosition.Fi * Mathf.Deg2Rad);

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


        public bool TryGetNodesForBuilding(PolarNode originNode, (int side, int depth) shift, out List<PolarNode> nodesForBuilding)
        {
            nodesForBuilding = new List<PolarNode>();
            var startingPolarPosition = originNode.PolarGridPosition;
            var thisRingFi = _polarGirdRingsSettings.ringSettingsList[startingPolarPosition.Ring].fi;

            for (var d = 0; d < shift.depth; d++)
            {
                for (var s = 0; s < shift.side; s++)
                {
                    var polarPosition = startingPolarPosition + new PolarGridPosition(0, d, s * thisRingFi, 0);
                    if (polarPosition.Fi >= 360)
                    {
                        polarPosition.Fi = 0;
                    }
                    
                    var neighbour = GetPolarNode(polarPosition);
                    if (neighbour == null)
                    {
                        Debug.Log("No next node found, cannot build here");

                        nodesForBuilding = new List<PolarNode>();
                        return false;
                    }

                    nodesForBuilding.Add(neighbour);
                }
            }

            return true;
        }

        public PolarGridPosition GetNextFiPosition(PolarGridPosition startingPolarPosition)
        {
            var thisRingSetting = _polarGirdRingsSettings.ringSettingsList[startingPolarPosition.Ring];
            
            var nextFi = startingPolarPosition.Fi + thisRingSetting.fi;
            
            //TU FIX
            if (nextFi > 360)
            {
                nextFi = 0;
            }
            
            var neighbourPosition = new PolarGridPosition(
                startingPolarPosition.Ring,
                startingPolarPosition.D,
                nextFi,
                startingPolarPosition.H);
            
            return neighbourPosition;
        }
        
        public PolarGridPosition GetNextDepthPosition(PolarGridPosition startingPolarPosition)
        {
            var nextD = startingPolarPosition.D + 1;

            var neighbourPosition = new PolarGridPosition(
                startingPolarPosition.Ring,
                nextD,
                startingPolarPosition.Fi,
                startingPolarPosition.H);

            return neighbourPosition;
        }

        public PolarNode GetPolarNode(PolarGridPosition polarGridPosition)
        {
            return GridNodes.FirstOrDefault(x => x.PolarGridPosition == polarGridPosition);
        }

        public PolarNode GetRandom()
        {
            return GridNodes.GetRandom();
        }
    }
}