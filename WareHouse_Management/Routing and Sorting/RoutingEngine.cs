using System;

namespace Warehouse
{
    public class RoutingEngine : IRoutingEngine
    {
        private const double OverweightLimit = 50.0;

        public Routing Route(string barcode, double weight)
        {
            string lane;

            // 1. Safety Check: Overweight packages are blocked immediately
            if (weight > OverweightLimit)
            {
                lane = "BLOCKED";
            }
            // 2. Sorting Logic: Route based on weight
            else if (weight < 5.0)
            {
                lane = "Lane1"; // Small items
            }
            else if (weight < 10.0)
            {
                lane = "Lane2"; // Medium items
            }
            else
            {
                lane = "Lane3"; // Heavy items
            }

            // Future expansion: You could add logic here to check 'barcode' string 
            // if you wanted specific barcodes to go to special zones.

            return new Routing(barcode, weight, lane);
        }
    }
}