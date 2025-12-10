using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WareHouse_Management.Environment
{
    public static class EnvironmentEnergyRunner
    {
        // Standalone demo
        public static async Task RunAsync()
        {
            Console.WriteLine("Temperature Sensor Output");
            Console.WriteLine("--------------------------");

            // Same range as the live system
            var sensor = new TemperatureSensor(20, 35);
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            // Short live console demo
            for (int i = 0; i < 5; i++)
            {
                double temp = sensor.ReadTemperature();
                fan.UpdateTemperature(temp);

                string fanState = fan.IsOn ? "ON" : "OFF";
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}  Temp = {temp:F1} °C   |   Fan = {fanState}");

                await Task.Delay(2000);
            }

            Console.WriteLine();
            Console.WriteLine("Running energy analysis demo...");
            await GenerateEnergyReportAsync(sensor);
        }

        // Generates its own samples from the given sensor
        private static async Task GenerateEnergyReportAsync(TemperatureSensor sensor)
        {
            int sampleCount = 60;
            int sampleIntervalMs = 1000;
            double intervalSeconds = sampleIntervalMs / 1000.0;

            var fan = new FanController(onThreshold: 30, offThreshold: 25);
            var samples = new List<EnergySample>();

            Console.WriteLine("Collecting monitoring data (60 seconds)...");
            Console.WriteLine("Time        Temp (°C)   Fan");

            for (int i = 0; i < sampleCount; i++)
            {
                double temp = sensor.ReadTemperature();
                fan.UpdateTemperature(temp);

                samples.Add(new EnergySample
                {
                    Temperature = temp,
                    FanOn = fan.IsOn
                });

                string fanState = fan.IsOn ? "ON" : "OFF";
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}   {temp,6:F1}      {fanState}");

                await Task.Delay(sampleIntervalMs);
            }

            EnergyReport report = EnergyReporter.ComputeFromSamples(samples, intervalSeconds);
            string csvPath = EnergyReporter.SaveToCsv(report);

            Console.WriteLine();
            Console.WriteLine("Energy report generated (demo).");
            Console.WriteLine("------------------------");
            Console.WriteLine($"Report File: {csvPath}");
            Console.WriteLine($"Average Temperature     : {report.AverageTemperature:F2} °C");
            Console.WriteLine($"Average Fan Runtime     : {report.AverageFanRuntimeSeconds:F2} s");
            Console.WriteLine($"Total Fan ON Time       : {report.TotalFanOnSeconds:F2} s");
            Console.WriteLine($"Fan ON Percentage       : {report.FanOnPercent:F1}%");
            Console.WriteLine($"Energy Score (0–100)    : {report.EnergyScore:F1}");
        }

        // Used by Program.cs – generates CSV report from live system samples
        public static Task GenerateFromSamplesAsync(
            List<EnergySample> samples,
            double sampleIntervalSeconds)
        {
            Console.WriteLine();

            if (samples == null || samples.Count == 0)
            {
                Console.WriteLine("No energy samples collected yet. Let the system run for a while first.");
                return Task.CompletedTask;
            }

            EnergyReport report = EnergyReporter.ComputeFromSamples(samples, sampleIntervalSeconds);
            string csvPath = EnergyReporter.SaveToCsv(report);

            Console.WriteLine("Energy report generated from live system data.");
            Console.WriteLine("------------------------");
            Console.WriteLine($"Report File: {csvPath}");
            Console.WriteLine($"Average Temperature     : {report.AverageTemperature:F2} °C");
            Console.WriteLine($"Average Fan Runtime     : {report.AverageFanRuntimeSeconds:F2} s");
            Console.WriteLine($"Total Fan ON Time       : {report.TotalFanOnSeconds:F2} s");
            Console.WriteLine($"Fan ON Percentage       : {report.FanOnPercent:F1}%");
            Console.WriteLine($"Energy Score (0–100)    : {report.EnergyScore:F1}");

            return Task.CompletedTask;
        }
    }
}
