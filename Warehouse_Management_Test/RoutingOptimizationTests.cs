using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;
using System;

namespace Warehouse.Tests
{
    [TestClass]
    public class RoutingOptimizationTests
    {
        [TestMethod]
        public void Optimized_MatchesOriginal_Outputs()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();
            var samples = new (string b, double w)[]
            {
                ("A", 3.0), ("B", 7.0), ("C", 12.0), ("D", 55.0)
            };
            foreach (var s in samples)
                Assert.AreEqual(orig.Route(s.b, s.w).TargetLane, opt.Route(s.b, s.w).TargetLane);
        }

        [TestMethod]
        public void Optimized_PreservesBlockedBehavior()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();
            Assert.AreEqual(orig.Route("X", 100).TargetLane, opt.Route("X", 100).TargetLane);
        }

        [TestMethod]
        public void Optimized_BoundaryChecks()
        {
            var orig = new RoutingEngine();
            var opt = new OptimizedRoutingEngine();
            double[] weights = { 4.99, 5.0, 9.99, 10.0, 50.0 };
            foreach (double w in weights)
                Assert.AreEqual(orig.Route("B", w).TargetLane, opt.Route("B", w).TargetLane);
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
}
