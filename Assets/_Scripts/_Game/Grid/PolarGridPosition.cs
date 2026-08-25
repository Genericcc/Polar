using System;

namespace _Scripts._Game.Grid
{
    public struct PolarGridPosition //: IEquatable<PolarGridPosition>
    {
        public int ParentRingIndex;
        public int D; //Depth or radius
        public int Fi; 
        public float H; //Height above ground level, assuming some future verticality

        public PolarGridPosition(int parentRingIndex, int d, int fi, float h)
        {
            ParentRingIndex = parentRingIndex;
            D = d;
            Fi = fi;
            H = h;
        }

        public override bool Equals(object obj)
        {
            return obj is PolarGridPosition position &&
                   ParentRingIndex == position.ParentRingIndex &&
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
            return HashCode.Combine(ParentRingIndex, D, Fi, H);
        }

        public override string ToString()
        {
            return $"d:{D}, fi:{Fi}";
        }

        public static bool operator ==(PolarGridPosition a, PolarGridPosition b)
        {
            return a.ParentRingIndex == b.ParentRingIndex &&
                   a.D == b.D &&
                   a.Fi == b.Fi &&
                   a.H == b.H;
        }

        public static PolarGridPosition operator +(PolarGridPosition a, PolarGridPosition b)
        {
            return new PolarGridPosition(
                a.ParentRingIndex + b.ParentRingIndex, 
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