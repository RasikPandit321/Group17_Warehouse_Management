using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;

namespace WareHouse_Management
{
    public class BarcodeScannerSensor
    {
        // Event triggered when a barcode is scanned
        public event Action<string>? OnBarcodeScanned;

        private readonly string _filePath;

        public BarcodeScannerSensor(string filePath)
        {
            _filePath = filePath;
        }

        // Simulate reading barcodes from file
        public void StartScanning()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Error: Barcode data file not found.");
                return;
            }

            Console.WriteLine("📡 Starting barcode scan simulation...\n");

            foreach (var line in File.ReadLines(_filePath))
            {
                var barcode = line.Trim();
                if (!string.IsNullOrEmpty(barcode))
                {
                    Console.WriteLine($"Scanned: {barcode} at {DateTime.Now:T}");
                    OnBarcodeScanned?.Invoke(barcode);
                }

                System.Threading.Thread.Sleep(1000); // Simulate delay
            }
        }
    }
}