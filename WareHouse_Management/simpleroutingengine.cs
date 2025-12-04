// Provides the baseline routing logic used for comparison and testing.
// Implements the shared IRoutingEngine contract.
namespace Warehouse
{
    public class SimpleRoutingEngine : IRoutingEngine
    {
        private const double OverweightLimit = 50.0;

        public Routing Route(string barcode, double weight)
        {
            // Block items exceeding the system's safety threshold.
            if (weight > OverweightLimit)
                return new Routing(barcode, weight, "BLOCKED");

            // Light, medium, and heavy categories.
            if (weight < 5)
                return new Routing(barcode, weight, "Lane1");

            if (weight < 10)
                return new Routing(barcode, weight, "Lane2");

            return new Routing(barcode, weight, "Lane3");
        }
    }
}
