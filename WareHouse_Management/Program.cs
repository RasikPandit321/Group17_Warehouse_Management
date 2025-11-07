using WareHouse_Management.Conveyor_and_Motor;

namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create demo driver and safety inputs
            var driver = new DemoMotorDriver();
            var safety = new DemoSafety();

            var motor = new MotorController(driver, safety);

            Console.WriteLine("=== Smart Warehouse Motor Controller===");

            // Try starting motor (safe condition)
            Console.WriteLine("Attempting to start motor...");
            var started = motor.Start();
            Console.WriteLine($"Motor started: {started}, Running: {driver.IsRunning}");

            // Simulate Emergency Stop
            Console.WriteLine("\nActivating Emergency Stop...");
            safety.EStop = true;
            var safeStart = motor.Start();
            Console.WriteLine($"Attempt to start during EStop: {safeStart}");
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            // Reset and restart motor
            Console.WriteLine("\nResetting EStop and restarting motor...");
            safety.EStop = false;
            motor.Start();
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            // Stop motor
            Console.WriteLine("\nStopping motor...");
            motor.Stop();
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            Console.WriteLine("\n=== Completed ===");
        }
    }

    // Simple demo-only driver class (for console simulation)
    public class DemoMotorDriver : IMotorDriver
    {
        public bool IsRunning { get; private set; }
        public void StartForward() => IsRunning = true;
        public void Stop() => IsRunning = false;
    }

    // Simple demo-only safety input class
    public class DemoSafety : ISafetyInputs
    {
        public bool EStop { get; set; } = false;
        public bool Fault { get; set; } = false;
    }
}

