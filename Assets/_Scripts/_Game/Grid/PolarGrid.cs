using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;

using UnityEngine;

namespace _Scripts._Game.Grid
{
    public class PolarGrid
    {
        public List<PolarNode> GridNodes;
        public List<Ring> Rings;

        private readonly PolarGridRingsSettings _polarGridRingsSettings;
        private readonly float _columnHeight;

        private const int FullCircle = 360;
        
        //TODO wyczyścić / refaktor 
        //TODO obrócić spawn nodów o 90 stopni żeby pasował do myszki, albo obczić czemu polar z myszki jest obrócony o 90 stopni
        public PolarGrid(PolarGridRingsSettings polarGridRingsSettings, float columnHeight)
        {
            GridNodes = new List <PolarNode>();
            Rings = new List<Ring>();
            
            _polarGridRingsSettings = polarGridRingsSettings;
            _columnHeight = columnHeight;
        }

        public void PopulateGrid(PolarNodeFactory polarNodeFactory, RingFactory ringFactory)
        {
            var endDistanceToWorldOrigin = 0f;

            for (var ringIndex = 0; ringIndex < _polarGridRingsSettings.ringSettingsList.Count; ringIndex++)
            {
                var ring = ringFactory.Create(ringIndex);
                
                ring.PopulateWithNodes(polarNodeFactory);
                GridNodes.AddRange(ring.Nodes);
                
                var startDistanceToWorldOrigin = endDistanceToWorldOrigin;
                endDistanceToWorldOrigin += ring.RingSettings.depth * _columnHeight;
                
                ring.SetBounds((startDistanceToWorldOrigin, endDistanceToWorldOrigin));
                ring.CreateMesh();
                
                Rings.Add(ring);
            }
        }

        public Vector3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            var previousFieldsCount = GetSumOfPreviousFields(polarGridPosition.ParentRingIndex);
            
            var x = (polarGridPosition.D + previousFieldsCount) * _columnHeight * Mathf.Cos(-polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.D + previousFieldsCount)* _columnHeight * Mathf.Sin(-polarGridPosition.Fi * Mathf.Deg2Rad);

            return new Vector3(x, y, z );
        }

        private int GetSumOfPreviousFields(int ringIndex)
        {
            var fields = 0;

            if (ringIndex == 0)
            {
                return 0;
            }
            
            for (var r = 1; r < _polarGridRingsSettings.ringSettingsList.Count; r++)
            {
                if (r <= ringIndex)
                {
                    fields += _polarGridRingsSettings.ringSettingsList[r - 1].depth;
                }
            }
            
            return fields;
        }

        public bool TryGetNodesForBuilding(
            PolarNode originNode, (int side, int depth) shift, out List<PolarNode> nodesForBuilding)
        {
            nodesForBuilding = new List<PolarNode>();
            var startingPolarPosition = originNode.PolarGridPosition;
            var thisRingFi = originNode.ParentRing.RingSettings.fi;

            for (var d = 0; d < shift.depth; d++)
            {
                for (var s = 0; s < shift.side; s++)
                {
                    var polarPosition = startingPolarPosition +
                                        new PolarGridPosition(0, d, s * thisRingFi, 0);

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

        public PolarGridPosition GetNodePolarPositionAt(Vector3 worldPosition)
        {
            var purePolar = GetPurePolarFromWorld(worldPosition);
            
            var ring = Rings.FirstOrDefault(
                x => x.Bounds.min <= purePolar.D && purePolar.D < x.Bounds.max);

            if (ring == null)
            {
                Debug.LogWarning("Mouse outside the rings. Returning default");
                return default;
            }
            
            var ringLocalStart = purePolar.D - ring.Bounds.min;
            var snappedD = Mathf.FloorToInt(ringLocalStart / _columnHeight);


            var howManyFiFitsIne = purePolar.Fi / ring.RingSettings.fi;
            var snappedFi = howManyFiFitsIne * ring.RingSettings.fi;

            return new PolarGridPosition(ring.RingIndex, snappedD, snappedFi, purePolar.H);
        }

        /// <summary>
        /// Gets Pure Polar Coordinates with ParentRingIndex as 0.
        /// d is r, fi is in degrees
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public PolarGridPosition GetPurePolarFromWorld(Vector3 worldPosition) 
        {
             var r = Mathf.RoundToInt(Vector3.Magnitude(worldPosition));
             var deg = Mathf.Atan2(worldPosition.x, worldPosition.z) * Mathf.Rad2Deg;;
             var fi = (int)((deg + 360) % 360);
             var h = Mathf.RoundToInt(worldPosition.y);

             return new PolarGridPosition(0, r, fi, h);
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