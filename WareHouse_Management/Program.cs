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
        static SocketIOClient.SocketIO client = null!;

        static int conveyorSpeed = 50;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== SMART WAREHOUSE SYSTEM (SPRINT 3) ===");
            Console.WriteLine("Connecting to HMI Dashboard...");

            // --- 1. SOCKET.IO SETUP ---
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

            // --- 2. SYSTEM SETUP ---
            var tempSensor = new TemperatureSensor(20, 35);
            var fan = new FanController(30, 25);
            var energySamples = new List<EnergySample>();

            var hardware = new SimulatedHardware();
            var motorCtrl = new MotorController(hardware, hardware);
            var conveyorCtrl = new ConveyorController(motorCtrl, hardware);

            var routingEngine = new RoutingEngine();
            var diverter = new DiverterGateController();

            if (!System.IO.File.Exists("barcodes.txt"))
                System.IO.File.WriteAllLines("barcodes.txt", new[] { "PKG-100", "PKG-500", "PKG-900" });

            var scanner = new BarcodeScannerSensor("barcodes.txt");
            var random = new Random();

            // --- 3. WIRING EVENTS ---

            // Handle HMI Commands
            client.On("conveyor:start", _ => conveyorCtrl.Start());
            client.On("conveyor:stop", _ => conveyorCtrl.Stop());

            // FIX 2: Update the conveyorSpeed variable when a command is received
            client.On("conveyor:speed", response =>
            {
                try
                {
                    conveyorSpeed = response.GetValue<int>();
                    Console.WriteLine($" [HMI CMD] Speed set to: {conveyorSpeed}");
                }
                catch { /* Ignore malformed data */ }
            });

            // Routing Event Logic
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

            Console.WriteLine("System Running. Press [Q] to Quit, [J] Jam, [E] E-Stop, [R] Report.");
            
            // --- 4. MAIN SIMULATION LOOP (Heartbeat) ---
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

                    // Save energy report on R key
                    if (key == ConsoleKey.R)
                    {
                        // 4.0 seconds because the loop uses Task.Delay(4000)
                        await EnvironmentEnergyRunner.GenerateFromSamplesAsync(energySamples, 4.0);
                    }
                }

                // B. Environment Logic
                double currentTemp = tempSensor.ReadTemperature();
                fan.UpdateTemperature(currentTemp);

                energySamples.Add(new EnergySample
                {
                    Temperature = currentTemp,
                    FanOn = fan.IsOn,
                    Timestamp = DateTime.Now
                });

                if (energySamples.Count > 100) energySamples.RemoveAt(0);
                var report = EnergyReporter.ComputeFromSamples(energySamples, 0.4);

                // FIX: Determine the speed to send. If the conveyor is NOT running, speed MUST be 0.
                int actualSpeed = hardware.IsRunning ? conveyorSpeed : 0;

                // C. Send Unified Update to HMI
                await client.EmitAsync("system:update", new
                {
                    conveyor = new
                    {
                        running = hardware.IsRunning,
                        speed = actualSpeed // <-- Sends 0 when stopped
                    },
                    env = new
                    {
                        temp = currentTemp.ToString("F1"),
                        fanOn = fan.IsOn,
                        energyScore = report.EnergyScore
                    }
                });
                // DEBUG – Confirms GUI and console are using same temperature
                Console.WriteLine($"\n[SENT TO GUI] Temperature = {currentTemp:F1} °C");
                // D. Console Status Line
                Console.Write($"\r [ENV] {currentTemp:F1}C (Fan: {fan.IsOn}) Score: {report.EnergyScore:F0} | Motor: {hardware.IsRunning}   ");

                await Task.Delay(4000);
            }

            await client.DisconnectAsync();
        }
    }
}