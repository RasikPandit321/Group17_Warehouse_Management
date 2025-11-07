namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Raise a few alarms (include newline so each is on its own line)
            Raise($"ALARM: Temperature high");
            Raise($"ALARM: Pressure spike");
            Raise($"ALARM: Temperature high (sensor 2)");

            Console.WriteLine("Alarms after raising:");
            PrintAllAlarms();

            // Clear any lines that contain "Temperature high"
            Clear("Temperature");

            Console.WriteLine();
            Console.WriteLine("Alarms after clearing 'Temperature':");

            Clear("Pressure");
            // Call the static EmergencyStop without instantiating it
            EmergencyStop.Estop($"EMERGENCY: Manual stop requested");

            PrintAllAlarms();
        }
    }
}
