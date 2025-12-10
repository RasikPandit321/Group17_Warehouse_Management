using System;
using System.IO;
using System.Threading;

namespace WareHouse_Management
{
    public class BarcodeScannerSensor
    {
        public event Action<string>? OnBarcodeScanned;
        private readonly string _filePath;

        public BarcodeScannerSensor(string filePath)
        {
            _filePath = filePath;
        }

        // Simulates continuous scanning by reading a file in a loop
        public void StartScanning()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Error: Barcode data file not found.");
                return;
            }

            Console.WriteLine("📡 Starting barcode scan simulation (Continuous Loop)...\n");

            // Infinite loop to simulate never-ending product flow
            while (true)
            {
                foreach (var line in File.ReadLines(_filePath))
                {
                    var barcode = line.Trim();
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        // Fire event to notify Program.cs
                        OnBarcodeScanned?.Invoke(barcode);
                    }

                    // Simulate time gap between packages (500ms)
                    Thread.Sleep(500);
                }

                // Short pause before restarting the batch
                Thread.Sleep(1000);
            }
        }
    }
}