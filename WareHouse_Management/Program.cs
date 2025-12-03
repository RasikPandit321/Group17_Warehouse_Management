using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WareHouse_Management.Environment;

namespace WareHouse_Management
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Temperature Sensor Output");
            Console.WriteLine("--------------------------");

            // Option A: random temperature sensor (used in Sprint 2 demo)
            var sensor = new TemperatureSensor(22, 32);

            // Option B: CSV-based sensor (available for future use)
            // Ensure temps_demo.csv is present in the output folder before enabling.
            // var csvSensor = TemperatureSensor.FromCsv("temps_demo.csv");
            // var sensor = csvSensor;

            // Fan controller with hysteresis thresholds
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            // 5 readings, approx. 10 seconds total
            for (int i = 0; i < 5; i++)
            {
                double temp = sensor.ReadTemperature();

                // Update fan state based on the current temperature
                fan.UpdateTemperature(temp);
                string fanState = fan.IsOn ? "ON" : "OFF";

                Console.WriteLine($"{DateTime.Now:HH:mm:ss}  Temp = {temp:F1} °C   |   Fan = {fanState}");
                await Task.Delay(2000);
            }

            Console.WriteLine();
            Console.WriteLine("Running energy analysis...");
            await GenerateEnergyReportAsync();

            Console.WriteLine();
            Console.WriteLine("Completed. Press any key to exit...");
            Console.ReadKey();
        }
        private static async Task GenerateEnergyReportAsync()
        {
            int sampleCount = 60;
            int sampleIntervalMs = 1000;
            double intervalSeconds = sampleIntervalMs / 1000.0;

            var sensor = new TemperatureSensor(22, 32);
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
            Console.WriteLine("Energy report generated.");
            Console.WriteLine("------------------------");
            Console.WriteLine($"Report File: {csvPath}");
            Console.WriteLine($"Average Temperature     : {report.AverageTemperature:F2} °C");
            Console.WriteLine($"Average Fan Runtime     : {report.AverageFanRuntimeSeconds:F2} s");
            Console.WriteLine($"Total Fan ON Time       : {report.TotalFanOnSeconds:F2} s");
            Console.WriteLine($"Fan ON Percentage       : {report.FanOnPercent:F1}%");
            Console.WriteLine($"Energy Score (0–100)    : {report.EnergyScore:F1}");
        }
    }
}
