using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.SystemIntegration;    

namespace WareHouse_Management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== SMART WAREHOUSE – MOTOR & CONVEYOR ===\n");
            Console.ResetColor();

            var driver = new DemoMotorDriver();
            var safety = new DemoSafety();
            var jamSensor = new DemoJamSensor();

            var motor = new MotorController(driver, safety);
            var conveyor = new ConveyorController(motor, jamSensor);

            // Usessystem manager for all control
            var transportSystem = new WarehouseTransportSystem(motor, conveyor); 

            // -----------------------------
            // BASIC START/STOP
            // -----------------------------

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Integrated System Start/Stop & Safety  ---\n");
            Console.ResetColor();

            Console.WriteLine("Attempting to start system (safe conditions)...");
            bool started = transportSystem.StartSystem();
            Console.WriteLine($"System started: {started}, Running: {transportSystem.IsRunning}");

            Console.WriteLine("\nActivating Emergency Stop...");
            safety.EStop = true; //directly manipulating the hardware state
            transportSystem.StopSystem();
            // Attempt to start while E-Stop is active
            var safeStart = transportSystem.StartSystem();
            Console.WriteLine($"Attempt to start during EStop: {safeStart}");
            Console.WriteLine($"Motor running: {transportSystem.IsRunning}");

            Console.WriteLine("\nResetting EStop and restarting system...");
            safety.EStop = false;
            transportSystem.StartSystem();
            Console.WriteLine($"Motor running: {transportSystem.IsRunning}");

            Console.WriteLine("\nStopping system...");
            transportSystem.StopSystem();
            Console.WriteLine($"Motor running: {transportSystem.IsRunning}");

            // -----------------------------
            // JAM DETECTION
            // -----------------------------

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Integrated System ---\n");
            Console.ResetColor();

            Console.WriteLine("Starting conveyor system...");
            transportSystem.StartSystem();
            Console.WriteLine($"Motor running: {transportSystem.IsRunning}, JamActive: {transportSystem.IsJamActive}");

            Console.WriteLine("\nSimulating jam detection...");
            jamSensor.JamDetected = true;
            // System polls sensors and reacts auto-stop on jam
            transportSystem.PollSystemForIssues();
            Console.WriteLine($"After jam: MotorRunning: {transportSystem.IsRunning}, JamActive: {transportSystem.IsJamActive}");

            Console.WriteLine("\nClearing jam sensor...");
            jamSensor.JamDetected = false;
            // Technician clears the logical fault state
            transportSystem.ClearSystemFault();
            Console.WriteLine($"JamActive after clear: {transportSystem.IsJamActive}");

            Console.WriteLine("\nAttempting to start system after jam cleared...");
            bool ok = transportSystem.StartSystem();
            Console.WriteLine($"Start successful: {ok}, Motor running: {transportSystem.IsRunning}");
        }

        // -------------------------------
        // Simulation classes (Remain unchanged)
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
}