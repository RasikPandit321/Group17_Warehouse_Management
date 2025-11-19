using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;

namespace Warehouse.Tests
{
    [TestClass]
    public class RoutingTests
    {
        [TestMethod]
        public void Routing_ShouldStoreDataCorrectly()
        {
            var routing = new Routing("ABC123", 5.5, "Lane1");

            Assert.AreEqual("ABC123", routing.Barcode);
            Assert.AreEqual(5.5, routing.Weight);
            Assert.AreEqual("Lane1", routing.TargetLane);
        }
    }
}
