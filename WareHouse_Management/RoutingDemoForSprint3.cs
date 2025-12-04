using System;
using System.Collections.Generic;
using Warehouse;

namespace WareHouse_Management
{
    public static class RoutingDemoForSprint3
    {
        public static void RunDemo()
        {
            var original = new RoutingEngine();
            var optimized = new OptimizedRoutingEngine();

            Console.WriteLine("=== SPRINT 3: ROUTING OPTIMIZATION DEMO ===");

            var batch = RoutingPerformance.Repeat("PKG", 7.5, 10000);

            var t1 = RoutingPerformance.Measure(original, batch);
            var t2 = RoutingPerformance.Measure(optimized, batch);

            Console.WriteLine($"Original engine:   {t1.TotalMilliseconds} ms");
            Console.WriteLine($"Optimized engine:  {t2.TotalMilliseconds} ms");

            Console.WriteLine("\nCorrectness check:");
            var samples = new (string, double)[]
            {
                ("PKG1", 3.2),
                ("PKG2", 7.5),
                ("PKG3", 12.0),
                ("PKG4", 65.0)
            };

            foreach (var (b, w) in samples)
            {
                var r1 = original.Route(b, w);
                var r2 = optimized.Route(b, w);

                Console.WriteLine($"{b} → original: {r1.TargetLane}, optimized: {r2.TargetLane}");
            }

            Console.WriteLine("=== DEMO COMPLETE ===");
        }
    }
}
