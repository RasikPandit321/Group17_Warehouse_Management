using System;
using Warehouse;

namespace WareHouse_Management
{
    public static class RoutingDemo
    {
        public static void Run()
        {
            Console.WriteLine("=== ROUTING DEMO START ===\n");
            string[] barcodes = { "PKG001", "PKG002", "PKG003", "PKG004" };
            double[] weights = { 3.2, 7.5, 12.0, 65.0 };

            var reader = new MockBarcodeReader(barcodes);
            IRoutingEngine engine = new RoutingEngine(); 


            Console.WriteLine("Reading barcodes and routing packages...\n");

            for (int i = 0; i < weights.Length; i++)
            {
                string barcode = reader.Read();
                double weight = weights[i];
                Routing result = engine.Route(barcode, weight);

                Console.WriteLine($"Barcode: {result.Barcode}");
                Console.WriteLine($"Weight: {result.Weight} kg");
                Console.WriteLine($"Assigned Lane: {result.TargetLane}");
                Console.WriteLine("--------------------------------\n");
            }

            Console.WriteLine("=== ROUTING DEMO COMPLETE ===");
        }
    }
}
