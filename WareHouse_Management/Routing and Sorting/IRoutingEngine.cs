namespace Warehouse
{
    public interface IRoutingEngine
    {
        Routing Route(string barcode, double weight);
    }
}