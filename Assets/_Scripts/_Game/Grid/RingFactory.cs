namespace _Scripts._Game.Grid
{
    public class RingFactory
    {
        public Ring Create(int ringIndex, RingSettings ringSettings, float rStart)
        {
            var ring = new Ring(ringIndex, ringSettings, rStart);
            return ring;
        }
    }
}