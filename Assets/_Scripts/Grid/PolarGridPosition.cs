using System;
using UnityEngine;

namespace _Scripts.Grid
{
    public struct PolarGridPosition : IEquatable<PolarGridPosition>
    {
        public int Ring;
        public int D;
        public float Fi;
        public int H;

        public PolarGridPosition(int ring, int d, float fi, int h)
        {
            Ring = ring;
            D = d;
            Fi = fi;
            H = h;
        }

        public override bool Equals(object obj)
        {
            return obj is PolarGridPosition position &&
                   Ring == position.Ring &&
                   D == position.D &&
                   Mathf.Abs(Fi - position.Fi) < 0.01 &&
                   H == position.H;
        }

        public bool Equals(PolarGridPosition other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ring, D, Fi, H);
        }

        public override string ToString()
        {
            return $"ring:{Ring}; r:{D}; fi:{Fi}; h:{H}";
        }

        public static bool operator ==(PolarGridPosition a, PolarGridPosition b)
        {
            return a.Ring == b.Ring &&
                   a.D == b.D &&
                   Mathf.Abs(a.Fi - b.Fi) < 0.01 &&
                   a.H == b.H;
        }

        public static PolarGridPosition operator +(PolarGridPosition a, PolarGridPosition b)
        {
            return new PolarGridPosition(
                a.Ring + b.Ring, 
                a.D + b.D, 
                a.Fi + b.Fi, 
                a.H + b.H);
        }

        public static bool operator !=(PolarGridPosition a, PolarGridPosition b)
        {
            return !(a == b);
        }
    }
}