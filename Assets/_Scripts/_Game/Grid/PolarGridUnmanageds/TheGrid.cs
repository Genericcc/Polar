

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Scripts._Game.Grid.PolarGridUnmanageds
{
    public struct TheGrid
    {
        public NativeList<RingData> Rings;
        public float GridNodeDepth;
        
        public float3 GetWorldFromPolar(PolarGridPosition polarGridPosition)
        {
            var radius = Rings[polarGridPosition.ParentRingIndex].Bounds.x + polarGridPosition.D * GridNodeDepth;
            
            math.sincos(math.radians(-polarGridPosition.Fi), out var sin, out var cos);    
            var x = radius * cos;
            var y = polarGridPosition.H;
            var z = radius * sin;

            return new float3(x, y, z);
        }
    }

    public struct RingData
    {
        public NativeList<PolarNodeData> Nodes;
        public int2 GridSize;
        public float2 Bounds;
        public int Fi;
        public int Depth;
        public int Index;
    }

    public struct PolarNodeData
    {
        public int Index;
        public PolarGridPosition Coords;
    }

    public struct TheGridEntity : IComponentData
    {
        public TheGrid Grid;
    }
}