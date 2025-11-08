using System;
using System.Threading.Tasks;
using AlarmService;
using WareHouse_Management;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Smart Warehouse Backend Starting...\n");

        // 🔔 Raise an initial alarm
        Alarm.Raise("System boot complete");

        // 🛑 Trigger E-Stop
        EmergencyStop.Estop("Emergency stop test triggered");

        // ⚙️ Motor control simulation
        var motor = new MotorController(
            driver: new FakeMotorDriver(),
            safety: new FakeSafetyInputs { EStop = false, Fault = false }
        );

        bool started = motor.Start();
        Console.WriteLine(started ? "✅ Motor started" : "❌ Motor blocked by safety");

        // 🌡️ Temperature simulation
        var tempSensor = new TemperatureSensor(20.0, 35.0);
        double temp = tempSensor.ReadTemperature();
        Console.WriteLine($"🌡️ Temperature: {temp}°C");

        // 📦 Barcode scanning
        var barcodePath = Path.Combine(AppContext.BaseDirectory, "barcodes.txt");
        var scanner = new BarcodeScannerSensor(barcodePath);
        scanner.OnBarcodeScanned += code =>
        {
            Console.WriteLine($"📦 Scanned barcode: {code}");
        };
        scanner.StartScanning();

        // 🚦 Diverter gate routing
        var diverter = new DiverterGateController();
        diverter.ActivateGate("Zone A");

        // Keep backend alive
        await Task.Delay(-1);
    }

    // 🔧 Fake implementations for testing
    public class FakeMotorDriver : IMotorDriver
    {
        public bool IsRunning { get; private set; } = false;
        public void StartForward()
        {
            IsRunning = true;
            Console.WriteLine("⚙️ Motor running forward");
        }
        public void Stop()
        {
            IsRunning = false;
            Console.WriteLine("🛑 Motor stopped");
        }
    }

    public class FakeSafetyInputs : ISafetyInputs
    {
        public bool EStop { get; set; }
        public bool Fault { get; set; }
    }
}