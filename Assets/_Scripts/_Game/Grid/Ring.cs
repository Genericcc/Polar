using System.Collections.Generic;

using _Scripts.Extensions;

using UnityEngine;

namespace _Scripts._Game.Grid
{
    public class Ring : MonoBehaviour
    {
        public int RingIndex { get; private set; }
        public RingSettings RingSettings { get; private set; }
        public (float min, float max) Bounds { get; private set; }
        public List<PolarNode> Nodes { get; set; }

        public void Initialise(int ringIndex, RingSettings ringSettings)
        {
            RingIndex = ringIndex;
            RingSettings = ringSettings;

            Nodes = new List<PolarNode>();
        }

        public void PopulateWithNodes(PolarNodeFactory polarNodeFactory)
        {
            var segmentsInGame = 6;

            //centreNode
            // if (RingIndex == 0)
            // {
            //     var polarGridPosition = new PolarGridPosition(0, 0, 0, RingSettings.height);
            //         
            //     var node = polarNodeFactory.Create(polarGridPosition, this);
            //     Nodes.Add(node);
            //         
            //     return;
            // }

            for (var depth = 0; depth < RingSettings.depth; depth++)
            {
                for (var fi = 360 - segmentsInGame * 60; fi < 360; fi += RingSettings.fi)
                {
                    var polarGridPosition = new PolarGridPosition(RingIndex, depth, fi, RingSettings.height);

                    var node = polarNodeFactory.Create(polarGridPosition, this);
                    node.transform.SetParent(this.transform, false);
                    Nodes.Add(node);
                }
            }
        }

        public void SetBounds((float startDistanceToWorldOrigin, float endDistanceToWorldOrigin) valueTuple)
        {
            Bounds = valueTuple;
        }

        public void CreateMesh(int sides, Material newMaterial)
        {
            var generator = GetComponent<RoundMeshGenerator>();
            generator.CreateRoundMesh(Bounds.max, sides);

            var mesh = GetComponent<MeshRenderer>();
            mesh.material = newMaterial;
        }
    }
}