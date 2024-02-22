using System.Collections.Generic;

namespace _Scripts.Grid
{
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