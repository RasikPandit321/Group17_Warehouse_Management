using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using AlarmService;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;
using Warehouse; // For Routing classes

namespace WareHouse_Management
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== WAREHOUSE INTEGRATION SYSTEM ===");
            Console.WriteLine("Controls: [S] Start Motor  [X] Stop Motor  [J] Toggle Jam  [E] Toggle E-Stop  [Q] Quit");
            Console.WriteLine("--------------------------------------------------------------------------------");

            // 1. SETUP ENVIRONMENT (US-2.5)
            // Simulate random temp between 20C and 35C
            var tempSensor = new TemperatureSensor(20, 35);
            // Fan ON at 30C, OFF at 25C (Hysteresis)
            var fan = new FanController(30, 25);

            // 2. SETUP CONVEYOR (US-2.1)
            var hardware = new SimulatedHardware(); // The fake motor/sensors
            var motorCtrl = new MotorController(hardware, hardware); // Motor needs driver + safety inputs
            var conveyorCtrl = new ConveyorController(motorCtrl, hardware); // Conveyor needs motor + jam sensor

            // 3. SETUP ROUTING (US-2.4)
            var routingEngine = new RoutingEngine();
            var diverter = new DiverterGateController();

            // Check if barcodes.txt exists, create dummy if not to prevent crash
            if (!System.IO.File.Exists("barcodes.txt"))
                System.IO.File.WriteAllLines("barcodes.txt", new[] { "PKG-A1", "PKG-B2", "PKG-C3", "PKG-HEAVY" });

            var scanner = new BarcodeScannerSensor("barcodes.txt");
            var randomWeight = new Random();

            // 4. WIRING EVENTS

            // Routing Event Logic
            scanner.OnBarcodeScanned += (barcode) =>
            {
                // Simulate weight (1kg to 60kg) to test Blocking logic
                double weight = randomWeight.NextDouble() * 59.0 + 1.0;

                var route = routingEngine.Route(barcode, weight);

                // Log the decision (Requirement: Log ID, lane, timestamp)
                string logMsg = $"[ROUTING] {DateTime.Now:T} | ID: {route.Barcode} | W: {route.Weight:F1}kg | -> {route.TargetLane}";

                if (route.TargetLane == "BLOCKED")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(logMsg + " (OVERWEIGHT!)");
                    Alarm.Raise($"Overweight package detected: {barcode}"); // Audible Alarm (US-2.3)
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(logMsg);
                    Console.ResetColor();
                    diverter.ActivateGate(route.TargetLane);
                }
            };

            // Run Barcode Scanner in background task so it doesn't block the Temp loop
            var scannerTask = Task.Run(() => scanner.StartScanning());

            // 5. MAIN SIMULATION LOOP
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q) break;

                    switch (key)
                    {
                        case ConsoleKey.S:
                            Console.WriteLine(" [CMD] Start Request...");
                            if (conveyorCtrl.Start()) Console.WriteLine(" [CMD] Request Accepted.");
                            else Console.WriteLine(" [CMD] Request Denied (Check Safety/Jam).");
                            break;
                        case ConsoleKey.X:
                            Console.WriteLine(" [CMD] Stop Request...");
                            conveyorCtrl.Stop();
                            break;
                        case ConsoleKey.J:
                            hardware.JamDetected = !hardware.JamDetected;
                            string jamMsg = hardware.JamDetected ? "JAM DETECTED" : "JAM CLEARED";
                            Console.WriteLine($" [SENSOR] {jamMsg}");
                            if (hardware.JamDetected)
                            {
                                conveyorCtrl.CheckJam(); // This triggers auto-stop
                                Alarm.Raise("Conveyor Jammed!"); // Audible Alarm (US-2.3)
                            }
                            else
                            {
                                conveyorCtrl.ClearJam();
                            }
                            break;
                        case ConsoleKey.E:
                            hardware.EStop = !hardware.EStop;
                            string eMsg = hardware.EStop ? "E-STOP ACTIVE" : "E-STOP RELEASED";
                            Console.WriteLine($" [SAFETY] {eMsg}");
                            if (hardware.EStop)
                            {
                                motorCtrl.Stop(); // Hard stop
                                EmergencyStop.Estop("Manual E-Stop Pressed"); // Logs and Beeps (US-2.3)
                            }
                            break;
                    }
                }

                // --- US-2.5 Environment Logic ---
                // Read Temp
                double currentTemp = tempSensor.ReadTemperature();

                // Update Fan
                fan.UpdateTemperature(currentTemp);

                // Display Status line (overwrites same line for cleanliness)
                string fanStatus = fan.IsOn ? "[FAN: ON]" : "[FAN: OFF]";
                string motorStatus = hardware.IsRunning ? "RUNNING" : "STOPPED";

                // If fan just turned on (simple edge detection for demo logging)
                if (fan.IsOn && currentTemp > 30.0 && currentTemp < 30.5)
                    Alarm.Raise($"High Temp Warning: {currentTemp}C");

                // Use \r to overwrite the status line at the bottom
                // Format: Temp | Fan | Motor | Safety
                Console.Write($"\r [ENV] {currentTemp:F1}C {fanStatus} | [CONVEYOR] {motorStatus} | Jam: {hardware.JamDetected} | EStop: {hardware.EStop}   ");

                // Wait 400ms (Approx 5 readings per 2 seconds)
                Thread.Sleep(400);
            }
        }
    }
}