using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Warehouse
{
    public static class RoutingPerformance
    {
        public static TimeSpan Measure(IRoutingEngine engine, IEnumerable<(string barcode, double weight)> inputs)
        {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (var (barcode, weight) in inputs)
                engine.Route(barcode, weight);

            sw.Stop();
            return sw.Elapsed;
        }

        public static IEnumerable<(string, double)> Repeat(string barcode, double weight, int count)
        {
            for (int i = 0; i < count; i++)
                yield return (barcode, weight);
        }
    }
}
