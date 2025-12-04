using System;

namespace Warehouse
{
    /// <summary>
    /// Simple demonstration class used during Sprint 3 to show
    /// how routing engines can be plugged into the system.
    /// </summary>
    public static class RoutingDemoForSprint3
    {
        public static void Run(IRoutingEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            var samples = new (string barcode, double weight)[]
            {
                ("DEMO1", 3.0),
                ("DEMO2", 7.0),
                ("DEMO3", 18.0),
                ("DEMO4", 60.0)
            };

            foreach (var s in samples)
            {
                var result = engine.Route(s.barcode, s.weight);
                Console.WriteLine($"{s.barcode} → {result.TargetLane}");
            }
        }
    }
}