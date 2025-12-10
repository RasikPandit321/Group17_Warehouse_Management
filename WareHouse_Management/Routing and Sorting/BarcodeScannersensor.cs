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

        public void StartScanning()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Error: Barcode data file not found.");
                return;
            }

            Console.WriteLine("📡 Starting barcode scan simulation (Continuous Loop)...\n");

            // FIX: Wraps the reading logic in an infinite loop
            while (true)
            {
                foreach (var line in File.ReadLines(_filePath))
                {
                    var barcode = line.Trim();
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        // The Program.cs logic will decide if it should process this
                        // based on whether the motor is running.
                        OnBarcodeScanned?.Invoke(barcode);
                    }

                    // Scanning speed (500ms = 2 packages per second)
                    Thread.Sleep(500);
                }

                // Optional: Short pause before restarting the batch
                Thread.Sleep(1000);
            }
        }
    }
}