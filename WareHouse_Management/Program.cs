using System;

namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ruleEngine = new RuleEngine();
            var diverter = new DiverterGateController();
            var scanner = new BarcodeScannerSensor("barcodes.txt");

            // Connect events
            scanner.OnBarcodeScanned += (barcode) =>
            {
                string zone = ruleEngine.DetermineRoute(barcode);
                diverter.ActivateGate(zone);
            };

            // Start simulation
            scanner.StartScanning();
        }
    }
}
