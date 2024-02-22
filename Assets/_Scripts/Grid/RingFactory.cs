namespace _Scripts.Grid
{
    public class RingFactory
    {
        public Ring Create(int ringIndex, RingSettings ringSettings, float rStart, PolarNodeFactory polarNodeFactory)
        {
            var ring = new Ring(ringIndex, ringSettings, rStart);
            ring.PopulateWithNodes(polarNodeFactory);

            return ring;
        }
    }
}