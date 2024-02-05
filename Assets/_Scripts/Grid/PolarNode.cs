using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Grid
{
    public class PolarNode
    {
        public int RingIndex { get; }
        public PolarGridPosition PolarGridPosition;
        private PolarGridSystem _polarGridSystem;
        
        public PolarNode(PolarGridSystem polarGridSystem, int ringIndex, PolarGridPosition polarGridPosition)
        {
            RingIndex = ringIndex;
            _polarGridSystem = polarGridSystem;
            PolarGridPosition = polarGridPosition;
        }

        public Vector3 GetWorldPosition()
        {
            return new Vector3();
        }

        public override string ToString()
        {
            return PolarGridPosition.ToString();
        }
    }
}//test