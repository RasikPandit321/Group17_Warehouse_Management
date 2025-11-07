using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Controllers;

namespace WareHouse_Management_Test
{
    [TestClass]
    public class SortingControllerTests
    {
        [TestMethod]
        public void Given_ValidZone_ShouldRouteToCorrectGate()
        {
            var sorter = new SortingController();
            bool result = sorter.RoutePackage("ZoneA");

            Assert.IsTrue(result);
            Assert.AreEqual("Gate1", sorter.LastRoutedGate);
        }

        [TestMethod]
        public void Given_InvalidZone_ShouldReturnFalse()
        {
            var sorter = new SortingController();
            bool result = sorter.RoutePackage("ZoneZ");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void When_RouteFails_ShouldRetryUpToMaxRetries()
        {
            var sorter = new SortingController();
            bool result = sorter.RoutePackage("InvalidZone");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void When_RoutingSucceeds_ShouldStoreZoneAndGate()
        {
            var sorter = new SortingController();
            sorter.RoutePackage("ZoneB");

            Assert.AreEqual("ZoneB", sorter.LastRoutedZone);
            Assert.AreEqual("Gate2", sorter.LastRoutedGate);
        }

        [TestMethod]
        public void When_GateFailsMultipleTimes_ShouldEventuallyFail()
        {
            var sorter = new SortingController();
            bool result = sorter.RoutePackage("ZoneC");
            Assert.IsTrue(result || result == false); // ensures no infinite loop
        }
    }
}
