using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketIOClient;
using AlarmService;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;
using Warehouse;

namespace WareHouse_Management
{
    internal class Program
    {
        // Global reference to Socket client
        static SocketIOClient.SocketIO client = null!;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== SMART WAREHOUSE SYSTEM (SPRINT 3) ===");
            Console.WriteLine("Connecting to HMI Dashboard...");

            // --- 1. SOCKET.IO SETUP ---
            client = new SocketIOClient.SocketIO("http://localhost:3001");

            client.OnConnected += (sender, e) => {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Connected to HMI Dashboard!");
                Console.ResetColor();
            };

            // Listen for commands FROM the Dashboard
            client.On("conveyor:start", response => {
                Console.WriteLine(" [HMI CMD] Start Received");
            });

            try
            {
                await client.ConnectAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("⚠️ Could not connect to HMI. Is the Node server running?");
            }

            // --- 2. SYSTEM SETUP ---
            // Environment (Sprint 3)
            var tempSensor = new TemperatureSensor(20, 35);
            var fan = new FanController(30, 25);
            var energySamples = new List<EnergySample>(); // Store samples for reporting

            // Conveyor & Safety
            var hardware = new SimulatedHardware();
            var motorCtrl = new MotorController(hardware, hardware);
            var conveyorCtrl = new ConveyorController(motorCtrl, hardware);

            // Routing
            var routingEngine = new RoutingEngine();
            var diverter = new DiverterGateController();

            // Ensure data file exists
            if (!System.IO.File.Exists("barcodes.txt"))
                System.IO.File.WriteAllLines("barcodes.txt", new[] { "PKG-100", "PKG-500", "PKG-900" });

            var scanner = new BarcodeScannerSensor("barcodes.txt");
            var random = new Random();

            // --- 3. WIRING EVENTS ---

            // Handle HMI Commands
            client.On("conveyor:start", _ => conveyorCtrl.Start());
            client.On("conveyor:stop", _ => conveyorCtrl.Stop());
            client.On("conveyor:speed", response => {
                // Future: Handle speed changes here
            });

            // Routing Event Logic (Only define this ONCE)
            scanner.OnBarcodeScanned += async (barcode) =>
            {
                double weight = random.NextDouble() * 59.0 + 1.0;
                var route = routingEngine.Route(barcode, weight);

                string logMsg = $"[ROUTING] {barcode} ({weight:F1}kg) -> {route.TargetLane}";

                if (route.TargetLane == "BLOCKED")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(logMsg + " [REJECTED]");
                    Alarm.Raise($"Overweight: {barcode}");
                    await client.EmitAsync("alarm:new", new { message = $"Overweight Pkg: {barcode}" });
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(logMsg);
                    diverter.ActivateGate(route.TargetLane);
                }
                Console.ResetColor();

                await client.EmitAsync("barcode:scanned", new { code = barcode });
                await client.EmitAsync("diverter:activated", new { zone = route.TargetLane });
            };

            // Start Scanner in background
            _ = Task.Run(() => scanner.StartScanning());

            Console.WriteLine("System Running. Press [Q] to Quit, [J] Jam, [E] E-Stop.");

            // --- 4. MAIN SIMULATION LOOP (Required for Sprint 3 Logic) ---
            while (true)
            {
                // A. Handle Keyboard Input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q) break;

                    if (key == ConsoleKey.J)
                    {
                        hardware.JamDetected = !hardware.JamDetected;
                        if (hardware.JamDetected)
                        {
                            conveyorCtrl.CheckJam();
                            Alarm.Raise("Conveyor Jammed!");
                            await client.EmitAsync("alarm:new", new { message = "Conveyor Jammed!" });
                        }
                        else
                        {
                            conveyorCtrl.ClearJam();
                        }
                    }
                    if (key == ConsoleKey.E)
                    {
                        hardware.EStop = !hardware.EStop;
                        if (hardware.EStop)
                        {
                            motorCtrl.Stop();
                            EmergencyStop.Estop("E-STOP ACTIVATED");
                            await client.EmitAsync("alarm:new", new { message = "E-STOP ACTIVATED" });
                        }
                    }
                    if (key == ConsoleKey.S) conveyorCtrl.Start();
                    if (key == ConsoleKey.X) conveyorCtrl.Stop();
                }

                // B. Sprint 3 Environment Logic
                double currentTemp = tempSensor.ReadTemperature();
                fan.UpdateTemperature(currentTemp);

                // Collect Energy Sample
                energySamples.Add(new EnergySample
                {
                    Temperature = currentTemp,
                    FanOn = fan.IsOn,
                    Timestamp = DateTime.Now
                });

                // Keep sample list small
                if (energySamples.Count > 100) energySamples.RemoveAt(0);

                // Generate Real-time Report
                var report = EnergyReporter.ComputeFromSamples(energySamples, 0.4);

                // C. Send Unified Update to HMI
                await client.EmitAsync("system:update", new
                {
                    conveyor = new
                    {
                        running = hardware.IsRunning,
                        speed = 50
                    },
                    env = new
                    {
                        temp = currentTemp.ToString("F1"),
                        fanOn = fan.IsOn,
                        energyScore = report.EnergyScore
                    }
                });

                // D. Console Status Line
                Console.Write($"\r [ENV] {currentTemp:F1}C (Fan: {fan.IsOn}) Score: {report.EnergyScore:F0} | Motor: {hardware.IsRunning}   ");

                await Task.Delay(400);
            }

            await client.DisconnectAsync();
        }
    }
}