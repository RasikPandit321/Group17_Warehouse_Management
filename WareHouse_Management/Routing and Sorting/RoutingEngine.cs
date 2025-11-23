namespace Warehouse
{
    public class RoutingEngine
    {
        private const double OverweightLimit = 50.0;

        public Routing Route(string barcode, double weight)
        {
            string lane;

            if (weight > OverweightLimit)
            {
                lane = "BLOCKED";
            }
            else if (weight < 5)
            {
                lane = "Lane1";
            }
            else if (weight < 10)
            {
                lane = "Lane2";
            }
            else
            {
                lane = "Lane3";
            }

            return new Routing(barcode, weight, lane);
        }
    }
}