namespace Warehouse
{
    /// <summary>
    /// Represents the routing result for a scanned package.
    /// </summary>
    public class Routing
    {
        public string Barcode { get; }
        public double Weight { get; }
        public string TargetLane { get; }

        public Routing(string barcode, double weight, string targetLane)
        {
            Barcode = barcode;
            Weight = weight;
            TargetLane = targetLane;
        }
    }
}