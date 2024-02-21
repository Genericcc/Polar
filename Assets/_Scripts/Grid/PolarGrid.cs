using System;
using System.Collections.Generic;
using System.Linq;

using _Scripts.Extensions;
using _Scripts.Managers;

using UnityEngine;

namespace _Scripts.Grid
{
    public class PolarGrid
    {
        public List<PolarNode> GridNodes;
        public List<Ring> Rings;

        private readonly PolarGirdRingsSettings _polarGirdRingsSettings;
        private readonly float _densityFactor;
        private readonly (int x, int y) _cellSize;

        public PolarGrid(PolarGirdRingsSettings polarGirdRingsSettings, (int x, int y) cellSize, float densityFactor)
        {
            GridNodes = new List <PolarNode>();
            Rings = new List<Ring>();
            
            _polarGirdRingsSettings = polarGirdRingsSettings;

            _densityFactor = densityFactor;
            _cellSize = cellSize;
        }

        public void Populate(PolarNodeFactory polarNodeFactory)
        {
            var distanceToWorldOrigin = 0;

            for (var ringIndex = 0; ringIndex < _polarGirdRingsSettings.ringSettingsList.Count; ringIndex++)
            {
                var ringFactory = new RingFactory();
                var ring = ringFactory.Create(
                    ringIndex,
                    _polarGirdRingsSettings.ringSettingsList[ringIndex],
                    distanceToWorldOrigin,
                    polarNodeFactory);
                
                GridNodes.AddRange(ring.Nodes);
                Rings.Add(ring);

                distanceToWorldOrigin += ring.RingSettings.depth * _cellSize.y;
            }

            foreach (var ringNode in Rings.SelectMany(ring => ring.Nodes))
            {
                ringNode.Set();
            }
        }

        public Vector3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            var node = GridNodes.FirstOrDefault(x => x.PolarGridPosition == polarGridPosition);
            if (node == null)
            {
                Debug.LogError("Node not found!");
                return new Vector3();
            }
            
            var previousFieldsCount = GetSumOfPreviousFields(node);

            var x = (polarGridPosition.D + previousFieldsCount) * Mathf.Cos(-polarGridPosition.Fi * Mathf.Deg2Rad);
            var y = polarGridPosition.H;
            var z = (polarGridPosition.D + previousFieldsCount) * Mathf.Sin(-polarGridPosition.Fi * Mathf.Deg2Rad);

            return new Vector3(x * (_cellSize.x - _densityFactor), y, z * (_cellSize.y - _densityFactor));
        }

        private int GetSumOfPreviousFields(PolarNode polarNode)
        {
            var fields = 0;

            if (polarNode.ParentRing.RingIndex == 0)
            {
                return 0;
            }
            
            for (var r = 1; r < _polarGirdRingsSettings.ringSettingsList.Count; r++)
            {
                if (r <= polarNode.ParentRing.RingIndex)
                {
                    fields += _polarGirdRingsSettings.ringSettingsList[r - 1].depth;
                }
            }
            
            return fields;
        }
        
        public bool TryGetNodesForBuilding(PolarNode originNode, (int side, int depth) shift, out List<PolarNode> nodesForBuilding)
        {
            nodesForBuilding = new List<PolarNode>();
            var startingPolarPosition = originNode.PolarGridPosition;
            var thisRingFi = originNode.ParentRing.RingSettings.fi;

            for (var d = 0; d < shift.depth; d++)
            {
                for (var s = 0; s < shift.side; s++)
                {
                    var polarPosition = startingPolarPosition + new PolarGridPosition(d, s * thisRingFi, 0);
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
            //var ring = GetRing(purePolar.D);
            var d = 0; //GetD(ring, purePolar.D);

            return new PolarGridPosition(d, purePolar.Fi, purePolar.H);
        }

        public PolarGridPosition GetPurePolarFromWorld(Vector3 worldPosition)
        {
            var r = Mathf.RoundToInt(Vector3.Magnitude(worldPosition));
            var fi = Mathf.RoundToInt(Mathf.Atan2(worldPosition.x, worldPosition.z));
            var h = Mathf.RoundToInt(worldPosition.y);

            return new PolarGridPosition(r, fi, h);
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
        public Ring Create(int ringIndex, RingSettings ringSettings, int rStart, PolarNodeFactory polarNodeFactory)
        {
            var ring = new Ring(ringIndex, ringSettings, rStart);
            ring.PopulateWithNodes(polarNodeFactory);

            return ring;
        }
    }

    public class Ring
    {
        public int RingIndex { get; }
        public int RStart { get; private set; }
        public RingSettings RingSettings { get; }
        public List<PolarNode> Nodes { get; set; }

        public Ring(int ringIndex, RingSettings ringSettings, int rStart)
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
                var polarGridPosition = new PolarGridPosition(0, 0, RingSettings.height);
                    
                var node = polarNodeFactory.Create(polarGridPosition, this);
                Nodes.Add(node);
                    
                return;
            }

            for (var depth = 1; depth <= RingSettings.depth; depth++)
            {
                for (var fi = 360 - segmentsInGame * 60; fi < 360; fi += RingSettings.fi)
                {
                    var polarGridPosition = new PolarGridPosition(depth, fi, RingSettings.height);

                    var node = polarNodeFactory.Create(polarGridPosition, this);
                    
                    Nodes.Add(node);
                }
            }
        }
    }
}