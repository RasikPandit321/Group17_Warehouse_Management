using System;

namespace WareHouse_Management.Environment
{
    public class TemperatureSensor
    {
        private readonly Random _random = new Random();

        public double MinTemp { get; }
        public double MaxTemp { get; }

        public TemperatureSensor(double minTemp, double maxTemp)
        {
            if (minTemp >= maxTemp)
                throw new ArgumentException("minTemp must be less than maxTemp");

            MinTemp = minTemp;
            MaxTemp = maxTemp;
        }

        public double ReadTemperature()
        {
            double value = MinTemp + _random.NextDouble() * (MaxTemp - MinTemp);
            return Math.Round(value, 1);   // 1 decimal place
        }
    }
}