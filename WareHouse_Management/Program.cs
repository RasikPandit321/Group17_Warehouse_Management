using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketIOClient;
using WareHouse_Management.Alarm_and_Estop;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;
using Warehouse;

namespace WareHouse_Management
{
    internal class Program
    {
        static SocketIOClient.SocketIO client = null!;
        static int conveyorSpeed = 50;
        static double smoothedTemp = 20.0;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== SMART WAREHOUSE SYSTEM (SPRINT 3) ===");
            Console.WriteLine("Connecting to HMI Dashboard...");

            // Setup Socket Connection
            client = new SocketIOClient.SocketIO("http://localhost:3001");

            client.OnConnected += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Connected to HMI Dashboard!");
                Console.ResetColor();
            };

            try
            {
                await client.ConnectAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("⚠️ Could not connect to HMI. Is the Node server running?");
            }

            // Initialize Sensors and Controllers
            var tempSensor = new TemperatureSensor(20, 35);
            var fan = new FanController(30, 25);
            var energySamples = new List<EnergySample>();

            var hardware = new SimulatedHardware();
            var motorCtrl = new MotorController(hardware, hardware);
            var conveyorCtrl = new ConveyorController(motorCtrl, hardware);

            var routingEngine = new RoutingEngine();
            var diverter = new DiverterGateController();

            // Create dummy barcode data if missing
            if (!System.IO.File.Exists("barcodes.txt"))
            {
                var mockData = new List<string>();
                for (int i = 1; i <= 50; i++) mockData.Add($"PKG-{1000 + i}");
                System.IO.File.WriteAllLines("barcodes.txt", mockData);
            }

            var scanner = new BarcodeScannerSensor("barcodes.txt");
            var random = new Random();

            // Helper function for Jam Simulation
            async void ToggleJam()
            {
                hardware.JamDetected = !hardware.JamDetected;
                if (hardware.JamDetected)
                {
                    conveyorCtrl.CheckJam(); // Stops the motor
                    Alarm.Raise("Conveyor Jammed!");
                    await client.EmitAsync("alarm:new", new { message = "Conveyor Jammed!" });
                }
                else
                {
                    conveyorCtrl.ClearJam();
                    // Auto-restart motor when Jam is cleared
                    Console.WriteLine(" [AUTO-RESTART] Resuming Conveyor...");
                    conveyorCtrl.Start();
                }
            }

            // Helper function for E-Stop Simulation
            async void ToggleEStop()
            {
                hardware.EStop = !hardware.EStop;
                if (hardware.EStop)
                {
                    motorCtrl.Stop();
                    EmergencyStop.Estop("E-STOP ACTIVATED");

                    // FIX: Added this line to trigger the Beep sound
                    Alarm.Raise("E-STOP ACTIVATED");

                    await client.EmitAsync("alarm:new", new { message = "E-STOP ACTIVATED" });
                }
                else
                {
                    EmergencyStop.Reset("E-STOP CLEARED");
                    await client.EmitAsync("alarm:new", new { message = "E-STOP CLEARED" });

                    // Auto-restart motor when E-Stop is released
                    Console.WriteLine(" [AUTO-RESTART] Resuming Conveyor...");
                    conveyorCtrl.Start();
                }
            }

            // Handle Incoming Commands
            client.On("conveyor:start", _ => conveyorCtrl.Start());
            client.On("conveyor:stop", _ => conveyorCtrl.Stop());
            client.On("conveyor:speed", response =>
            {
                try { conveyorSpeed = response.GetValue<int>(); } catch { }
            });

            client.On("sim:jam", _ => ToggleJam());
            client.On("sim:estop", _ => ToggleEStop());

            // Handle Report Generation Request
            client.On("request:report", async _ =>
            {
                string filename = await Task.Run(() =>
                {
                    var report = EnergyReporter.ComputeFromSamples(energySamples, 0.4);
                    return EnergyReporter.SaveToCsv(report);
                });
                await client.EmitAsync("report:generated", new { filename = filename });
                Console.WriteLine($"[REPORT] Generated: {filename}");
            });

            // Routing Event Logic
            scanner.OnBarcodeScanned += async (barcode) =>
            {
                // Do not process if conveyor is stopped
                if (!hardware.IsRunning) return;

                double weight = random.NextDouble() * 59.0 + 1.0;
                var route = routingEngine.Route(barcode, weight);

                string logMsg = $"[ROUTING] {barcode} ({weight:F1}kg) -> {route.TargetLane}";

                // Apply Routing Rules
                if (route.TargetLane == "Lane3")
                {
                    // Heavy items just get logged in yellow, no alarm raised
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(logMsg + " [HEAVY]");
                    diverter.ActivateGate(route.TargetLane);
                }
                else
                {
                    // Standard routing
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(logMsg);
                    diverter.ActivateGate(route.TargetLane);
                }
                Console.ResetColor();

                // Update UI
                await client.EmitAsync("barcode:scanned", new { code = barcode });
                await client.EmitAsync("diverter:activated", new
                {
                    zone = route.TargetLane,
                    code = barcode
                });
            };

            // Start Scanner in background task
            _ = Task.Run(() => scanner.StartScanning());

            Console.WriteLine("System Running. Press [Q] to Quit, [J] Jam, [E] E-Stop.");

            DateTime startTime = DateTime.Now;

            // Main Simulation Loop
            while (true)
            {
                // Handle Keyboard Input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q) break;
                    if (key == ConsoleKey.J) ToggleJam();
                    if (key == ConsoleKey.E) ToggleEStop();
                    if (key == ConsoleKey.S) conveyorCtrl.Start();
                    if (key == ConsoleKey.X) conveyorCtrl.Stop();
                }

                // Update Temperature (Sine Wave for Demo)
                double t = (DateTime.Now - startTime).TotalSeconds;
                smoothedTemp = 27.5 + 8.0 * Math.Sin(t * 0.4);

                fan.UpdateTemperature(smoothedTemp);

                // Collect Data for Energy Report
                energySamples.Add(new EnergySample
                {
                    Temperature = smoothedTemp,
                    FanOn = fan.IsOn,
                    Timestamp = DateTime.Now
                });

                if (energySamples.Count > 100) energySamples.RemoveAt(0);
                var report = EnergyReporter.ComputeFromSamples(energySamples, 0.4);
                int actualSpeed = hardware.IsRunning ? conveyorSpeed : 0;

                // Send Live Data to Dashboard
                await client.EmitAsync("system:update", new
                {
                    conveyor = new { running = hardware.IsRunning, speed = actualSpeed },
                    env = new { temp = smoothedTemp.ToString("F1"), fanOn = fan.IsOn, energyScore = report.EnergyScore },
                    flags = new { isJam = hardware.JamDetected, isEStop = hardware.EStop }
                });

                // Console Status Bar
                Console.Write($"\r [ENV] {smoothedTemp:F1}C (Fan: {fan.IsOn}) Score: {report.EnergyScore:F0} | Motor: {hardware.IsRunning}   ");

                // Controls loop speed
                await Task.Delay(400);
            }

            await client.DisconnectAsync();
        }
    }
}