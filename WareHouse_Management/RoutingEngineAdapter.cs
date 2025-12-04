namespace Warehouse
{
    public class RoutingEngineAdapter : IRoutingEngine
    {
        private readonly IRoutingEngine _inner;

        public RoutingEngineAdapter(IRoutingEngine inner) => _inner = inner;

        public Routing Route(string barcode, double weight) => _inner.Route(barcode, weight);
    }
}
