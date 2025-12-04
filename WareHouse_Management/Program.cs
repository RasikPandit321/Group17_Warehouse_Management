using AlarmService;
using LogService;
using static AlarmService.Alarm;

namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ;
            // samples of how to use // can be deleted
            // Raise a few alarms (include newline so each is on its own line)
            Raise($"ALARM: Temperature high");
            Raise($"ALARM: Pressure spike");
            Raise($"ALARM: Temperature high (sensor 2)");

            PrintAllAlarms();

            Log.Archive("Something");

            Log.PrintLogs();

            Console.WriteLine("\n\nAfter clear below\n\n");

            Log.ClearLogs();

            Log.PrintLogs();

            ClearAlarms();

            PrintAllAlarms();

 
        }
    }
}