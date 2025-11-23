using System;
using System.Collections.Generic;
using System.IO;

namespace WareHouse_Management.Environment
{
    public class TemperatureSensor
    {
        private readonly Random _random = new Random();

        public double MinTemp { get; }
        public double MaxTemp { get; }

        private readonly Queue<double>? _csvValues;

        public TemperatureSensor(double minTemp, double maxTemp)
        {
            if (minTemp >= maxTemp)
                throw new ArgumentException("minTemp must be less than maxTemp");

            MinTemp = minTemp;
            MaxTemp = maxTemp;
            _csvValues = null;
        }

        public static TemperatureSensor FromCsv(string path)
        {
            // ✔ NEW validation
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("CSV path must be provided.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("CSV file not found.", path);

            var values = new Queue<double>();

            foreach (var line in File.ReadAllLines(path))
            {
                if (double.TryParse(line.Trim(), out double temp))
                    values.Enqueue(temp);
            }

            if (values.Count == 0)
                throw new InvalidOperationException("CSV contained no valid temperatures.");

            return new TemperatureSensor(values);
        }

        private TemperatureSensor(Queue<double> csvValues)
        {
            _csvValues = csvValues;
            MinTemp = double.NaN;
            MaxTemp = double.NaN;
        }

        public double ReadTemperature()
        {
            // CSV mode: replay values from the queue in a loop
            if (_csvValues != null)
            {
                double value = _csvValues.Dequeue();
                _csvValues.Enqueue(value);
                return Math.Round(value, 1);
            }

            // Random mode
            double randomValue = MinTemp + _random.NextDouble() * (MaxTemp - MinTemp);
            return Math.Round(randomValue, 1);
        }
    }
}