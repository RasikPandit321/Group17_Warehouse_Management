using System;

namespace Warehouse
{
    public class RoutingEngine : IRoutingEngine
    {
        // Define sorting thresholds
        private const double LightLimit = 5.0;     // 0-5kg
        private const double StandardLimit = 20.0; // 5-20kg

        public Routing Route(string barcode, double weight)
        {
            string lane;

            // Lane 1: Small/Light items (Express)
            if (weight < LightLimit)
            {
                lane = "Lane1";
            }
            // Lane 2: Standard items
            else if (weight < StandardLimit)
            {
                lane = "Lane2";
            }
            // Lane 3: Heavy/Overweight items (Special Handling)
            else
            {
                lane = "Lane3";
            }

            return new Routing(barcode, weight, lane);
        }
    }
}