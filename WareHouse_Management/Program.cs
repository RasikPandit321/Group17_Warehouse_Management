using WareHouse_Management.Conveyor_and_Motor;

namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== SMART WAREHOUSE – MOTOR & CONVEYOR DEMO ===\n");
            Console.ResetColor();

            // Demo hardware objects (simulation only)
            var driver = new DemoMotorDriver();
            var safety = new DemoSafety();
            var jamSensor = new DemoJamSensor();

            var motor = new MotorController(driver, safety);
            var conveyor = new ConveyorController(motor, jamSensor);

            // -----------------------------
            // BASIC START/STOP
            // -----------------------------

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Motor Start/Stop & Safety Demo ---\n");
            Console.ResetColor();

            Console.WriteLine("Attempting to start motor (safe conditions)...");
            bool started = motor.Start();
            Console.WriteLine($"Motor started: {started}, Running: {driver.IsRunning}");

            Console.WriteLine("\nActivating Emergency Stop...");
            safety.EStop = true;
            bool denied = motor.Start();
            Console.WriteLine($"Attempt to start during EStop: {denied}");
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            Console.WriteLine("\nResetting EStop and restarting motor...");
            safety.EStop = false;
            motor.Start();
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            Console.WriteLine("\nStopping motor...");
            motor.Stop();
            Console.WriteLine($"Motor running: {driver.IsRunning}");

            // -----------------------------
            // JAM DETECTION
            // -----------------------------

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Conveyor Jam Detection Demo ---\n");
            Console.ResetColor();

            Console.WriteLine("Starting conveyor...");
            conveyor.Start();
            Console.WriteLine($"Motor running: {driver.IsRunning}, JamActive: {conveyor.JamActive}");

            Console.WriteLine("\nSimulating jam detection...");
            jamSensor.JamDetected = true;
            conveyor.CheckJam();
            Console.WriteLine($"After jam: MotorRunning: {driver.IsRunning}, JamActive: {conveyor.JamActive}");

            Console.WriteLine("\nClearing jam sensor...");
            jamSensor.JamDetected = false;
            conveyor.ClearJam();
            Console.WriteLine($"JamActive after clear: {conveyor.JamActive}");

            Console.WriteLine("\nAttempting to start conveyor after jam cleared...");
            bool ok = conveyor.Start();
            Console.WriteLine($"Start successful: {ok}, Motor running: {driver.IsRunning}");
        }
    }

    // -------------------------------
    // Simulation classes (demo only)
    // -------------------------------

    public class DemoMotorDriver : IMotorDriver
    {
        public bool IsRunning { get; private set; }

        public void StartForward() => IsRunning = true;

        public void Stop() => IsRunning = false;
    }

    public class DemoSafety : ISafetyInputs
    {
        public bool EStop { get; set; } = false;
        public bool Fault { get; set; } = false;
    }

    public class DemoJamSensor : IJamSensor
    {
        public bool JamDetected { get; set; }
    }
}
