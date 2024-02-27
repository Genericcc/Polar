using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;

using UnityEngine;

namespace _Scripts._Game.Grid
{
    public class PolarGrid
    {
        public readonly List<PolarNode> GridNodes;
        public readonly List<Ring> Rings;

        private readonly PolarGridRingsSettings _polarGridRingsSettings;
        private readonly float _columnHeight;

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
                
                var startDistanceToWorldOrigin = endDistanceToWorldOrigin;
                endDistanceToWorldOrigin += ring.RingSettings.depth * _columnHeight;
                
                ring.SetBounds((startDistanceToWorldOrigin, endDistanceToWorldOrigin));
                ring.CreateMesh(50);
                ring.PopulateWithNodes(polarNodeFactory);
                
                GridNodes.AddRange(ring.Nodes);
                
                Rings.Add(ring);
            }
        }

        public Vector3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            var previousFieldsCount = GetSumOfPreviousFields(polarGridPosition.ParentRingIndex);
            
            var x = (polarGridPosition.D + previousFieldsCount) * _columnHeight * Mathf.Cos(-polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.D + previousFieldsCount) * _columnHeight * Mathf.Sin(-polarGridPosition.Fi * Mathf.Deg2Rad);

            return new Vector3(x, y, z);
        }
        
        public Vector3 GetWorldFromPurePolar(PurePolarCoords purePolar)
        {
            var x = purePolar.Radius * Mathf.Cos(-purePolar.Fi * Mathf.Deg2Rad);
            var y = purePolar.H;
            var z = purePolar.Radius * Mathf.Sin(-purePolar.Fi * Mathf.Deg2Rad);

            return new Vector3(x, y, z);
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
                    var fi = s * thisRingFi;
                    var polarPosition = startingPolarPosition + new PolarGridPosition(0, d, fi, 0);

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

            var ring = GetRingThroughRadius(purePolar.Radius);
            if (ring is null)
            {
                Debug.Log("Mouse outside the rings. Returning default");
                return default;
            }
            
            var distance = purePolar.Radius - ring.Bounds.min;
            var snappedD = Mathf.FloorToInt(distance / _columnHeight);
            
            var howManyFiFitsIn = Mathf.FloorToInt(purePolar.Fi / ring.RingSettings.fi);
            var snappedFi = howManyFiFitsIn * ring.RingSettings.fi;

            return new PolarGridPosition(ring.RingIndex, snappedD, snappedFi, purePolar.H);
        }

        private Ring GetRingThroughRadius(float purePolarRadius)
        {
            return Rings.FirstOrDefault(x => x.Bounds.min <= purePolarRadius && purePolarRadius < x.Bounds.max);
        }

        public PurePolarCoords GetPurePolarFromWorld(Vector3 worldPosition) 
        {
             var r = new Vector2(worldPosition.x, worldPosition.z).magnitude;
             
             // CZEMU MINUS Z? Bo tworzę grid ze wskazówkami zegara, a trygonometria działa w drugą stronę
             // link z obrazkiem (tam oś y to moje -z): https://www.omnicalculator.com/math/cartesian-to-polar 
             var deg = Mathf.Atan2(-worldPosition.z, worldPosition.x) * Mathf.Rad2Deg;;
             var fi = (float)((deg + 360) % 360);
             var h = worldPosition.y;

             return new PurePolarCoords(r, fi, h);
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

    public struct PurePolarCoords
    {
        public float Radius;
        public float Fi;
        public float H;

        public PurePolarCoords(float radius, float fi, float h)
        {
            Radius = radius;
            Fi = fi;
            H = h;
        }

        public override string ToString()
        {
            return $"R: {Radius}, Fi: {Fi}, H: {H}";
        }
    }
}