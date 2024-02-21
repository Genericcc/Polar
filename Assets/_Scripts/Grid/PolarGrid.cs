using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;
using _Scripts.Managers;

using UnityEngine;

using Math = Unity.Physics.Math;

namespace _Scripts.Grid
{
    public class PolarGrid
    {
        public List<PolarNode> GridNodes;
        public List<Ring> Rings;
        public Dictionary<int, (float, float)> RingsBounds;

        private readonly PolarGirdRingsSettings _polarGirdRingsSettings;
        private readonly float _columnHeight;

        public PolarGrid(PolarGirdRingsSettings polarGirdRingsSettings, float columnHeight)
        {
            GridNodes = new List <PolarNode>();
            Rings = new List<Ring>();
            RingsBounds = new Dictionary<int, (float, float)>();
            
            _polarGirdRingsSettings = polarGirdRingsSettings;

            _columnHeight = columnHeight;
        }

        public void Populate(PolarNodeFactory polarNodeFactory)
        {
            var startDistanceToWorldOrigin = 0f;
            var endDistanceToWorldOrigin = 0f;

            for (var ringIndex = 0; ringIndex < _polarGirdRingsSettings.ringSettingsList.Count; ringIndex++)
            {
                var ringFactory = new RingFactory();
                var ring = ringFactory.Create(
                    ringIndex,
                    _polarGirdRingsSettings.ringSettingsList[ringIndex],
                    endDistanceToWorldOrigin,
                    polarNodeFactory);
                
                GridNodes.AddRange(ring.Nodes);
                Rings.Add(ring);
                
                startDistanceToWorldOrigin = endDistanceToWorldOrigin;
                endDistanceToWorldOrigin += ring.RingSettings.depth * _columnHeight;

                RingsBounds.TryAdd(ringIndex, (startDistanceToWorldOrigin, endDistanceToWorldOrigin));
            }

            foreach (var ringNode in Rings.SelectMany(ring => ring.Nodes))
            {
                ringNode.Set();
            }
        }

        public Vector3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            var previousFieldsCount = GetSumOfPreviousFields(polarGridPosition.ParentRingIndex);
            
            var x = (polarGridPosition.D + previousFieldsCount) * (_columnHeight) * Mathf.Cos(-polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.D + previousFieldsCount)* (_columnHeight) * Mathf.Sin(-polarGridPosition.Fi * Mathf.Deg2Rad);

            return new Vector3(x, y, z );
        }

        private int GetSumOfPreviousFields(int ringIndex)
        {
            var fields = 0;

            if (ringIndex == 0)
            {
                return 0;
            }
            
            for (var r = 1; r < _polarGirdRingsSettings.ringSettingsList.Count; r++)
            {
                if (r <= ringIndex)
                {
                    fields += _polarGirdRingsSettings.ringSettingsList[r - 1].depth;
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
                                        new PolarGridPosition(originNode.ParentRing.RingIndex, d, s * thisRingFi, 0);

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

        public PolarGridPosition GetPolarFromWorld(Vector3 worldPosition)
        {
            var purePolar = GetPurePolarFromWorld(worldPosition);
            
            var ringBoundsPair = RingsBounds.FirstOrDefault(
                x => x.Value.Item1 <= purePolar.D && purePolar.D < x.Value.Item2);
            var ringLeftoverR = purePolar.D - ringBoundsPair.Value.Item1;
            var d = Mathf.FloorToInt(ringLeftoverR / (_columnHeight));

            return new PolarGridPosition(ringBoundsPair.Key, d, purePolar.Fi, purePolar.H);
        }

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

    public class RingFactory
    {
        public Ring Create(int ringIndex, RingSettings ringSettings, float rStart, PolarNodeFactory polarNodeFactory)
        {
            var ring = new Ring(ringIndex, ringSettings, rStart);
            ring.PopulateWithNodes(polarNodeFactory);

            return ring;
        }
    }

    public class Ring
    {
        public int RingIndex { get; }
        public float RStart { get; private set; }
        public RingSettings RingSettings { get; }
        public List<PolarNode> Nodes { get; set; }

        public Ring(int ringIndex, RingSettings ringSettings, float rStart)
        {
            RingIndex = ringIndex;
            RingSettings = ringSettings;
            RStart = rStart;

            Nodes = new List<PolarNode>();
        }

        public void PopulateWithNodes(PolarNodeFactory polarNodeFactory)
        {
            var segmentsInGame = 6;

            //centreNode
            if (RingIndex == 0)
            {
                var polarGridPosition = new PolarGridPosition(0, 0, 0, RingSettings.height);
                    
                var node = polarNodeFactory.Create(polarGridPosition, this);
                Nodes.Add(node);
                    
                return;
            }

            for (var depth = 0; depth < RingSettings.depth; depth++)
            {
                for (var fi = 360 - segmentsInGame * 60; fi < 360; fi += RingSettings.fi)
                {
                    var polarGridPosition = new PolarGridPosition(RingIndex, depth, fi, RingSettings.height);

                    var node = polarNodeFactory.Create(polarGridPosition, this);
                    
                    Nodes.Add(node);
                }
            }
        }
    }
}