using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;

using UnityEngine;

namespace _Scripts.Grid
{
    public class PolarGrid
    {
        public List<PolarNode> GridNodes;
        public List<Ring> Rings;
        public Dictionary<Ring, (float minBound, float maxBound)> RingsBounds;

        private readonly PolarGridRingsSettings _polarGridRingsSettings;
        private readonly float _columnHeight;

        private const int FullCircle = 360;
        
        //TODO wyczyścić / refaktor 
        //TODO obrócić spawn nodów o 90 stopni żeby pasował do myszki, albo obczić czemu polar z myszki jest obrócony o 90 stopni
        // Rings można zredukować?
        public PolarGrid(PolarGridRingsSettings polarGridRingsSettings, float columnHeight)
        {
            GridNodes = new List <PolarNode>();
            Rings = new List<Ring>();
            RingsBounds = new Dictionary<Ring, (float, float)>();
            
            _polarGridRingsSettings = polarGridRingsSettings;

            _columnHeight = columnHeight;
        }

        public void Populate(PolarNodeFactory polarNodeFactory)
        {
            var startDistanceToWorldOrigin = 0f;
            var endDistanceToWorldOrigin = 0f;

            for (var ringIndex = 0; ringIndex < _polarGridRingsSettings.ringSettingsList.Count; ringIndex++)
            {
                var ringFactory = new RingFactory();
                var ring = ringFactory.Create(
                    ringIndex,
                    _polarGridRingsSettings.ringSettingsList[ringIndex],
                    endDistanceToWorldOrigin,
                    polarNodeFactory);
                
                GridNodes.AddRange(ring.Nodes);
                Rings.Add(ring);
                
                startDistanceToWorldOrigin = endDistanceToWorldOrigin;
                endDistanceToWorldOrigin += ring.RingSettings.depth * _columnHeight;

                RingsBounds.TryAdd(ring, (startDistanceToWorldOrigin, endDistanceToWorldOrigin));
            }

            foreach (var ringNode in Rings.SelectMany(ring => ring.Nodes))
            {
                ringNode.Set();
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
            
            var ringBoundsPair = RingsBounds.FirstOrDefault(
                x => x.Value.minBound <= purePolar.D && purePolar.D < x.Value.maxBound);

            if (ringBoundsPair.Key == null)
            {
                Debug.LogWarning("Mouse outside the rings. Returning default");
                return default;
            }
            
            var ringLocalStart = purePolar.D - ringBoundsPair.Value.Item1;
            var snappedD = Mathf.FloorToInt(ringLocalStart / _columnHeight);
            
            var ringFi = purePolar.Fi - FullCircle % ringBoundsPair.Key.RingSettings.fi;
            var snappedFi = (FullCircle % ringBoundsPair.Key.RingSettings.fi) * ringBoundsPair.Key.RingSettings.fi;

            return new PolarGridPosition(ringBoundsPair.Key.RingIndex, snappedD, purePolar.Fi, purePolar.H);
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