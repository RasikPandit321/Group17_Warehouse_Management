using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Models;

namespace WareHouse_Management_Test
{
    [TestClass]
    public class RoutingModelTests
    {
        [TestMethod]
        public void Given_ValidBarcode_ShouldReturnCorrectZone()
        {
            var router = new RoutingModel();
            string result = router.GetTargetZone("PKG-ABC123");
            Assert.AreEqual("ZoneA", result);
        }

        [TestMethod]
        public void Given_UnknownBarcode_ShouldReturnErrorZone()
        {
            var router = new RoutingModel();
            string result = router.GetTargetZone("PKG-UNKNOWN");
            Assert.AreEqual("ZoneError", result);
        }

        [TestMethod]
        public void When_BarcodeProcessed_ShouldCreateLogEntry()
        {
            var router = new RoutingModel();
            router.GetTargetZone("PKG-XYZ789");
            Assert.IsTrue(router.HasLogEntry("PKG-XYZ789"));
        }

        [TestMethod]
        public void When_LogEntryExists_TimestampShouldNotBeEmpty()
        {
            var router = new RoutingModel();
            router.GetTargetZone("PKG-ABC123");
            string ts = router.GetTimestamp("PKG-ABC123");
            Assert.IsFalse(string.IsNullOrEmpty(ts));
        }

        [TestMethod]
        public void Given_EmptyBarcode_ShouldReturnErrorZone()
        {
            var router = new RoutingModel();
            string result = router.GetTargetZone("");
            Assert.AreEqual("ZoneError", result);
        }

        [TestMethod]
        public void Given_LowercaseBarcode_ShouldReturnSameZone()
        {
            var router = new RoutingModel();
            string result = router.GetTargetZone("pkg-abc123");
            Assert.AreEqual("ZoneA", result);
        }
    }
}
    
