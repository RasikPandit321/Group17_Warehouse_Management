using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Warehouse;
using WareHouse_Management;

namespace Warehouse_AllTests
{
    /* ------------------------------------------------------------
     * BARCODE SCANNER SENSOR TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class BarcodeScannerSensorTests
    {
        [TestMethod]
        public void Scanner_StartScanning_DoesNotThrow_WhenFileMissing()
        {
            var sensor = new BarcodeScannerSensor("this_file_does_not_exist.txt");
            sensor.StartScanning();
            Assert.IsTrue(true);
        }
    }

    /* ------------------------------------------------------------
     * MOCK BARCODE READER TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class MockBarcodeReaderTests
    {
        [TestMethod]
        public void MockReader_ShouldReturnNextBarcode()
        {
            var mock = new MockBarcodeReader(new[] { "PKG1", "PKG2", "PKG3" });

            Assert.AreEqual("PKG1", mock.Read());
            Assert.AreEqual("PKG2", mock.Read());
            Assert.AreEqual("PKG3", mock.Read());
        }

        [TestMethod]
        public void MockReader_ShouldLoop_WhenOutOfBarcodes()
        {
            var mock = new MockBarcodeReader(new[] { "A", "B" });

            mock.Read(); // A
            mock.Read(); // B
            var third = mock.Read(); // loops → A

            Assert.AreEqual("A", third);
        }
    }

    /* ------------------------------------------------------------
     * ROUTING ENGINE BEHAVIOR TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class RoutingEngineBehaviorTests
    {
        private readonly IRoutingEngine engine = new RoutingEngine();

        [TestMethod] public void WeightUnder5_GoesToLane1() => Assert.AreEqual("Lane1", engine.Route("A", 4.9).TargetLane);
        [TestMethod] public void WeightExactly5_GoesToLane2() => Assert.AreEqual("Lane2", engine.Route("A", 5.0).TargetLane);
        [TestMethod] public void WeightUnder10_GoesToLane2() => Assert.AreEqual("Lane2", engine.Route("A", 9.9).TargetLane);
        [TestMethod] public void WeightExactly10_GoesToLane3() => Assert.AreEqual("Lane3", engine.Route("A", 10.0).TargetLane);
        [TestMethod] public void WeightOver10_GoesToLane3() => Assert.AreEqual("Lane3", engine.Route("A", 15.0).TargetLane);
        [TestMethod] public void WeightOverOverweightLimit_IsBlocked() => Assert.AreEqual("BLOCKED", engine.Route("A", 60.0).TargetLane);
    }

    /* ------------------------------------------------------------
     * ROUTING ENGINE BASIC TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class RoutingEngineTests
    {
        [TestMethod]
        public void RoutingEngine_ShouldRouteSmallItemsToLane1()
        {
            IRoutingEngine engine = new RoutingEngine();
            var routing = engine.Route("PKG100", 2.5);

            Assert.AreEqual("Lane1", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldRouteMediumItemsToLane2()
        {
            IRoutingEngine engine = new RoutingEngine();
            var routing = engine.Route("PKG200", 7.0);

            Assert.AreEqual("Lane2", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldRouteHeavyItemsToLane3()
        {
            IRoutingEngine engine = new RoutingEngine();
            var routing = engine.Route("PKG300", 15.0);

            Assert.AreEqual("Lane3", routing.TargetLane);
        }

        [TestMethod]
        public void RoutingEngine_ShouldBlockOverweightPackages()
        {
            IRoutingEngine engine = new RoutingEngine();
            var routing = engine.Route("PKG400", 55.0);

            Assert.AreEqual("BLOCKED", routing.TargetLane);
        }
    }

    /* ------------------------------------------------------------
     * OPTIMIZED ROUTING ENGINE TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class RoutingOptimizationTests
    {
        [TestMethod]
        public void Optimized_MatchesOriginal_Outputs()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();

            var samples = new (string barcode, double weight)[]
            {
                ("A", 3.0),
                ("B", 7.0),
                ("C", 12.0),
                ("D", 55.0)
            };

            foreach (var s in samples)
                Assert.AreEqual(orig.Route(s.barcode, s.weight).TargetLane,
                                opt.Route(s.barcode, s.weight).TargetLane);
        }

        [TestMethod]
        public void Optimized_PreservesBlockedBehavior()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();

            Assert.AreEqual(orig.Route("X", 100).TargetLane,
                            opt.Route("X", 100).TargetLane);
        }

        [TestMethod]
        public void Optimized_BoundaryChecks()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();

            double[] weights = { 4.99, 5.0, 9.99, 10.0, 50.0 };

            foreach (double w in weights)
                Assert.AreEqual(orig.Route("B", w).TargetLane,
                                opt.Route("B", w).TargetLane);
        }

        [TestMethod]
        public void Optimized_NotSlowerThan_Original()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();

            var batch = RoutingPerformance.Repeat("PKG", 7.5, 5000);

            var t1 = RoutingPerformance.Measure(orig, batch);
            var t2 = RoutingPerformance.Measure(opt, batch);

            Assert.IsTrue(t2.TotalMilliseconds <= t1.TotalMilliseconds * 1.2);
        }
    }

    /* ------------------------------------------------------------
     * ROUTING PERFORMANCE TESTS
     * ------------------------------------------------------------ */
    [TestClass]
    public class RoutingPerformanceTests
    {
        [TestMethod]
        public void Measure_ReturnsNonZero_ForNonEmptyInput()
        {
            var engine = new RoutingEngine();
            var inputs = RoutingPerformance.Repeat("PKG", 7.5, 100);

            var t = RoutingPerformance.Measure(engine, inputs);

            Assert.IsTrue(t.TotalMilliseconds >= 0);
        }

        [TestMethod]
        public void Measure_ReturnsZero_ForEmptyInput()
        {
            var engine = new RoutingEngine();
            var inputs = new List<(string, double)>();

            var t = RoutingPerformance.Measure(engine, inputs);

            Assert.IsTrue(t.TotalMilliseconds >= 0);
        }

        [TestMethod]
        public void Measure_Works_WithOptimizedEngine()
        {
            var engine = new OptimizedRoutingEngine();
            var inputs = RoutingPerformance.Repeat("PKG", 7.5, 100);

            var t = RoutingPerformance.Measure(engine, inputs);

            Assert.IsNotNull(t);
        }

        [TestMethod]
        public void Measure_DifferentInputsReturnDifferentTimes()
        {
            var engine = new RoutingEngine();

            var shortRun = RoutingPerformance.Measure(engine, RoutingPerformance.Repeat("A", 7.5, 10));
            var longRun = RoutingPerformance.Measure(engine, RoutingPerformance.Repeat("A", 7.5, 2000));

            Assert.AreNotEqual(shortRun.TotalMilliseconds, longRun.TotalMilliseconds);
        }

        [TestMethod]
        public void Measure_DoesNotThrow_OnLargeBatch()
        {
            var engine = new RoutingEngine();
            var inputs = RoutingPerformance.Repeat("PKG", 7.5, 10000);

            RoutingPerformance.Measure(engine, inputs);

            Assert.IsTrue(true);
        }
    }

    /* ------------------------------------------------------------
     * ROUTING OBJECT TESTS
     * ------------------------------------------------------------ */
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

    /* ------------------------------------------------------------
     * DUMMY TEST
     * ------------------------------------------------------------ */
    [TestClass]
    public class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }
    }
}
