using System;
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
            Console.WriteLine("Completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
