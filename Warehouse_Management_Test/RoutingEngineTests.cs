using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;

namespace Warehouse.Tests
{
    [TestClass]
    public class RoutingEngineTests
    {
        [TestMethod]
        public void RoutingEngine_ShouldRouteSmallItemsToLane1()
        {
            var engine = new RoutingEngine();

            var routing = engine.Route("PKG100", 2.5); // weight < 5kg

            Assert.AreEqual("Lane1", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldRouteMediumItemsToLane2()
        {
            var engine = new RoutingEngine();

            var routing = engine.Route("PKG200", 7.0); // 5–10 kg

            Assert.AreEqual("Lane2", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldRouteHeavyItemsToLane3()
        {
            var engine = new RoutingEngine();

            var routing = engine.Route("PKG300", 15.0); // >10 kg

            Assert.AreEqual("Lane3", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldBlockOverweightPackages()
        {
            var engine = new RoutingEngine();

            var routing = engine.Route("PKG400", 55.0); // too heavy

            Assert.AreEqual("BLOCKED", routing.TargetLane);
        }
    }
}
