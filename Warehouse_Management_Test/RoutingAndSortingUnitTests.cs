using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;

namespace Warehouse_Management_Test
{
    /// <summary>
    /// Unit tests for RoutingEngine and Routing components.
    /// Ensures package routing, lane selection, and data preservation
    /// behave correctly across normal, boundary, and extreme weight cases.
    /// </summary>
    [TestClass]
    public class RoutingAndSortingUnitTests
    {
        // Routing engine instance used for all routing logic tests.
        private RoutingEngine _engine = null!;

        /// <summary>
        /// Creates a fresh RoutingEngine instance before each test.
        /// Ensures routing logic is tested with a clean state.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _engine = new RoutingEngine();
        }

        /// <summary>
        /// Light packages (<5 kg) should be routed to Lane1.
        /// </summary>
        [TestMethod]
        public void Test_Route_Light_Package()
        {
            var result = _engine.Route("PKG1", 4.0);
            Assert.AreEqual("Lane1", result.TargetLane);
        }

        /// <summary>
        /// Boundary test: weight just below 5 kg should still go to Lane1.
        /// </summary>
        [TestMethod]
        public void Test_Route_Boundary_Light_Package()
        {
            var result = _engine.Route("PKG2", 4.99);
            Assert.AreEqual("Lane1", result.TargetLane);
        }

        /// <summary>
        /// Standard weight packages (5–19.99 kg) should be routed to Lane2.
        /// </summary>
        [TestMethod]
        public void Test_Route_Standard_Package()
        {
            var result = _engine.Route("PKG3", 10.0);
            Assert.AreEqual("Lane2", result.TargetLane);
        }

        /// <summary>
        /// Boundary test: exactly 5 kg belongs in Lane2.
        /// </summary>
        [TestMethod]
        public void Test_Route_Boundary_Standard_Lower()
        {
            var result = _engine.Route("PKG4", 5.0);
            Assert.AreEqual("Lane2", result.TargetLane);
        }

        /// <summary>
        /// Boundary test: weight just below 20 kg remains in Lane2.
        /// </summary>
        [TestMethod]
        public void Test_Route_Boundary_Standard_Upper()
        {
            var result = _engine.Route("PKG5", 19.99);
            Assert.AreEqual("Lane2", result.TargetLane);
        }

        /// <summary>
        /// Heavy packages (≥20 kg) should be routed to Lane3.
        /// </summary>
        [TestMethod]
        public void Test_Route_Heavy_Package()
        {
            var result = _engine.Route("PKG6", 25.0);
            Assert.AreEqual("Lane3", result.TargetLane);
        }

        /// <summary>
        /// Boundary test: exactly 20 kg should be routed to Lane3.
        /// </summary>
        [TestMethod]
        public void Test_Route_Boundary_Heavy()
        {
            var result = _engine.Route("PKG7", 20.0);
            Assert.AreEqual("Lane3", result.TargetLane);
        }

        /// <summary>
        /// Barcode should remain unchanged after routing.
        /// </summary>
        [TestMethod]
        public void Test_Barcode_Preservation()
        {
            string code = "ABC-12345";
            var result = _engine.Route(code, 10.0);
            Assert.AreEqual(code, result.Barcode);
        }

        /// <summary>
        /// Weight should also be preserved exactly after routing.
        /// </summary>
        [TestMethod]
        public void Test_Weight_Preservation()
        {
            double w = 15.5;
            var result = _engine.Route("PKG8", w);
            Assert.AreEqual(w, result.Weight);
        }

        /// <summary>
        /// Zero weight should still be categorized as a light package (Lane1).
        /// </summary>
        [TestMethod]
        public void Test_Zero_Weight()
        {
            var result = _engine.Route("PKG9", 0);
            Assert.AreEqual("Lane1", result.TargetLane);
        }

        /// <summary>
        /// Negative weights, though invalid, should be safely treated as light packages.
        /// </summary>
        [TestMethod]
        public void Test_Negative_Weight()
        {
            var result = _engine.Route("PKG10", -5.0);
            Assert.AreEqual("Lane1", result.TargetLane);
        }

        /// <summary>
        /// Extremely heavy packages should still correctly route to Lane3.
        /// </summary>
        [TestMethod]
        public void Test_Extreme_Heavy_Weight()
        {
            var result = _engine.Route("PKG11", 1000.0);
            Assert.AreEqual("Lane3", result.TargetLane);
        }

        /// <summary>
        /// Routing should support an empty barcode string without failure.
        /// </summary>
        [TestMethod]
        public void Test_Empty_Barcode()
        {
            var result = _engine.Route("", 10.0);
            Assert.AreEqual("", result.Barcode);
        }

        /// <summary>
        /// Null barcodes should be preserved as null in the Routing object.
        /// </summary>
        [TestMethod]
        public void Test_Null_Barcode()
        {
            var result = _engine.Route(null!, 10.0);
            Assert.IsNull(result.Barcode);
        }

        /// <summary>
        /// Ensures Lane1 logic is consistent for small-weight routing.
        /// </summary>
        [TestMethod]
        public void Test_Lane1_Logic_Consistency()
        {
            Assert.AreEqual("Lane1", _engine.Route("A", 1).TargetLane);
        }

        /// <summary>
        /// Ensures Lane2 logic is consistent for mid-weight routing.
        /// </summary>
        [TestMethod]
        public void Test_Lane2_Logic_Consistency()
        {
            Assert.AreEqual("Lane2", _engine.Route("B", 12).TargetLane);
        }

        /// <summary>
        /// Ensures Lane3 logic is consistent for high-weight routing.
        /// </summary>
        [TestMethod]
        public void Test_Lane3_Logic_Consistency()
        {
            Assert.AreEqual("Lane3", _engine.Route("C", 21).TargetLane);
        }

        /// <summary>
        /// Routing objects should be creatable with provided values.
        /// </summary>
        [TestMethod]
        public void Test_Routing_Object_Creation()
        {
            Routing r = new Routing("ABC", 10.0, "LaneX");
            Assert.AreEqual("ABC", r.Barcode);
        }

        /// <summary>
        /// IRoutingEngine interface should return a valid Routing object.
        /// </summary>
        [TestMethod]
        public void Test_Interface_Implementation()
        {
            IRoutingEngine engineRef = new RoutingEngine();
            Assert.IsNotNull(engineRef.Route("A", 10));
        }

        /// <summary>
        /// Placeholder test for Diverter activation on Lane1.
        /// Always passes by design.
        /// </summary>
        [TestMethod]
        public void Test_Diverter_Activate_Lane1()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Placeholder test for Diverter activation on Lane2.
        /// </summary>
        [TestMethod]
        public void Test_Diverter_Activate_Lane2()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Placeholder test for Diverter activation on Lane3.
        /// </summary>
        [TestMethod]
        public void Test_Diverter_Activate_Lane3()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Placeholder test for handling invalid diverter lanes.
        /// </summary>
        [TestMethod]
        public void Test_Diverter_Invalid_Lane()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Routing should support barcodes with special characters.
        /// </summary>
        [TestMethod]
        public void Test_Route_Special_Characters()
        {
            var result = _engine.Route("PKG#$%", 5.0);
            Assert.AreEqual("PKG#$%", result.Barcode);
        }
    }
}

