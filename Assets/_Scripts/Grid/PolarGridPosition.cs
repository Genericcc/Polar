using System;
using UnityEngine;

namespace _Scripts.Grid
{
    public struct PolarGridPosition : IEquatable<PolarGridPosition>
    {
        public int Ring;
        public int R;
        public float Fi;
        public int H;

        public PolarGridPosition(int ring, int r, float fi, int h)
        {
            Ring = ring;
            R = r;
            Fi = fi;
            H = h;
        }

        public override bool Equals(object obj)
        {
            return obj is PolarGridPosition position &&
                   Ring == position.Ring &&
                   R == position.R &&
                   Mathf.Abs(Fi - position.Fi) < 0.01 &&
                   H == position.H;
        }

        public bool Equals(PolarGridPosition other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ring, R, Fi, H);
        }

        public override string ToString()
        {
            return $"ring:{Ring}; r:{R}; fi:{Fi}; h:{H}";
        }

        public static bool operator ==(PolarGridPosition a, PolarGridPosition b)
        {
            return a.Ring == b.Ring &&
                   a.R == b.R &&
                   Mathf.Abs(a.Fi - b.Fi) < 0.01 &&
                   a.H == b.H;
        }

        public static bool operator !=(PolarGridPosition a, PolarGridPosition b)
        {
            return !(a == b);
        }
    }
}