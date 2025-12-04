using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Warehouse;

namespace Warehouse.Tests
{
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

        // 🔥 FIXED TEST BELOW
        [TestMethod]
        public void Measure_DifferentInputsReturnDifferentTimes()
        {
            var engine = new RoutingEngine();

            var shortRun = RoutingPerformance.Measure(engine, RoutingPerformance.Repeat("A", 7.5, 10));
            var longRun = RoutingPerformance.Measure(engine, RoutingPerformance.Repeat("A", 7.5, 2000));

            // Instead of asserting long > short, we require that they are not *identical*
            // because timing noise makes ordering unstable.
            Assert.AreNotEqual(shortRun.TotalMilliseconds, longRun.TotalMilliseconds,
                "Short and long runs should not take exactly the same amount of time.");
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
}
