namespace Warehouse
{
    /// <summary>
    /// Defines the contract for all routing engines in the system.
    /// </summary>
    public interface IRoutingEngine
    {
        Routing Route(string barcode, double weight);
    }
}