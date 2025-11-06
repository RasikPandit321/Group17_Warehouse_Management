using System;
using System.Threading.Tasks;
using WareHouse_Management.Environment;

namespace WareHouse_Management
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sensor = new TemperatureSensor(22, 32);

            Console.WriteLine("Temperature Sensor Demo (5 readings)");
            Console.WriteLine("-------------------------------------");

            for (int i = 0; i < 5; i++)
            {
                double t = sensor.ReadTemperature();
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}  Temp = {t:F1} °C");
                await Task.Delay(2000);
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
