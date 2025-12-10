using System;

namespace Warehouse
{
    public class RoutingEngine : IRoutingEngine
    {
        // Define sorting thresholds (kg)
        private const double LightLimit = 5.0;
        private const double StandardLimit = 20.0;

        // Determines the destination lane based on package weight
        public Routing Route(string barcode, double weight)
        {
            string lane;

            // Lane 1: Light items (Express)
            if (weight < LightLimit)
            {
                lane = "Lane1";
            }
            // Lane 2: Standard items
            else if (weight < StandardLimit)
            {
                lane = "Lane2";
            }
            // Lane 3: Heavy items (Special Handling)
            else
            {
                lane = "Lane3";
            }

            return new Routing(barcode, weight, lane);
        }
    }
}