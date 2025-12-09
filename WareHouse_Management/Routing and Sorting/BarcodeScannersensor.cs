using System;
using System.IO;

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

            Console.WriteLine("📡 Starting barcode scan simulation...\n");
            foreach (var line in File.ReadLines(_filePath))
            {
                var barcode = line.Trim();
                if (!string.IsNullOrEmpty(barcode))
                {
                    Console.WriteLine($"Scanned: {barcode} at {DateTime.Now:T}");
                    OnBarcodeScanned?.Invoke(barcode);
                }
                // UPDATE: Reduced delay to 500ms to keep lanes full
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}