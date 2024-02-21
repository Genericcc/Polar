using System;
using UnityEngine;

namespace _Scripts.Grid
{
    public struct PolarGridPosition : IEquatable<PolarGridPosition>
    {
        public int D;
        public int Fi;
        public int H;

        public PolarGridPosition(int d, int fi, int h)
        {
            D = d;
            Fi = fi;
            H = h;
        }

        public override bool Equals(object obj)
        {
            return obj is PolarGridPosition position &&
                   D == position.D &&
                   Fi == position.Fi &&
                   H == position.H;
        }

        public bool Equals(PolarGridPosition other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(D, Fi, H);
        }

        public override string ToString()
        {
            return $"r:{D}; fi:{Fi}; h:{H}";
        }

        public static bool operator ==(PolarGridPosition a, PolarGridPosition b)
        {
            return a.D == b.D &&
                   a.Fi == b.Fi &&
                   a.H == b.H;
        }

        public static PolarGridPosition operator +(PolarGridPosition a, PolarGridPosition b)
        {
            return new PolarGridPosition(
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