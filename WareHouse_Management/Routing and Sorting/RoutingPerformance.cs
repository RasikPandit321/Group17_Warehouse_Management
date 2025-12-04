using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Warehouse
{
    /// <summary>
    /// Provides lightweight performance measurements for routing engines.
    /// </summary>
    public static class RoutingPerformance
    {
        public static TimeSpan Measure(IRoutingEngine engine, IEnumerable<(string, double)> items)
        {
            var sw = Stopwatch.StartNew();

            foreach (var (barcode, weight) in items)
                engine.Route(barcode, weight);

            sw.Stop();
            return sw.Elapsed;
        }

        /// <summary>
        /// Generates repeated test inputs.
        /// </summary>
        public static List<(string, double)> Repeat(string barcode, double weight, int count)
        {
            var list = new List<(string, double)>(count);
            for (int i = 0; i < count; i++)
                list.Add((barcode, weight));

            return list;
        }
    }
}