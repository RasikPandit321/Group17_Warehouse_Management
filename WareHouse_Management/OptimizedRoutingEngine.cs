namespace Warehouse
{
    public class OptimizedRoutingEngine : IRoutingEngine
    {
        private const double OverweightLimit = 50.0;

        public Routing Route(string barcode, double weight)
        {
            if (weight > OverweightLimit)
                return new Routing(barcode, weight, "BLOCKED");

            if (weight < 5)
                return new Routing(barcode, weight, "Lane1");

            if (weight < 10)
                return new Routing(barcode, weight, "Lane2");

            return new Routing(barcode, weight, "Lane3");
        }
    }
}
