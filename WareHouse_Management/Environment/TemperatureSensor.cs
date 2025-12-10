using System;
using System.Collections.Generic;
using System.IO;

namespace WareHouse_Management.Environment
{
    public class TemperatureSensor
    {
        private readonly Random _random = new Random(); // Random generator for simulated temps

        public double MinTemp { get; } // Minimum random temperature
        public double MaxTemp { get; } // Maximum random temperature

        private readonly Queue<double>? _csvValues; // Optional queue of CSV temperatures

        public TemperatureSensor(double minTemp, double maxTemp)
        {
            if (minTemp >= maxTemp)
                throw new ArgumentException("minTemp must be less than maxTemp");

            MinTemp = minTemp;
            MaxTemp = maxTemp;
            _csvValues = null; // No CSV mode for this constructor
        }

        public static TemperatureSensor FromCsv(string path)
        {
            // Validate input CSV file path
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("CSV path must be provided.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("CSV file not found.", path);

            var values = new Queue<double>();

            // Read each line and parse temperature values
            foreach (var line in File.ReadAllLines(path))
            {
                if (double.TryParse(line.Trim(), out double temp))
                    values.Enqueue(temp);
            }

            if (values.Count == 0)
                throw new InvalidOperationException("CSV contained no valid temperatures.");

            return new TemperatureSensor(values); // Create sensor in CSV mode
        }

        private TemperatureSensor(Queue<double> csvValues)
        {
            _csvValues = csvValues; // Store CSV temp sequence
            MinTemp = double.NaN;   // Not used in CSV mode
            MaxTemp = double.NaN;   // Not used in CSV mode
        }

        public double ReadTemperature()
        {
            // CSV mode: continuously cycle through provided values
            if (_csvValues != null)
            {
                double value = _csvValues.Dequeue();
                _csvValues.Enqueue(value); // Put it back for looping
                return Math.Round(value, 1);
            }

            // Random mode: generate temp between MinTemp and MaxTemp
            double randomValue = MinTemp + _random.NextDouble() * (MaxTemp - MinTemp);
            return Math.Round(randomValue, 1);
        }
    }
}
