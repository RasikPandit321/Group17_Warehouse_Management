using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;
using Warehouse;
using AlarmService;
using LogService;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class IntegrationTests
    {
        // Path cleanup helpers
        private string _alarmFile = Path.GetFullPath("alarm.txt");
        private string _logFile = Path.GetFullPath("logs.txt");
        private string _barcodeFile = Path.GetFullPath("barcodes.txt");

        [TestInitialize]
        public void Setup()
        {
            // Clean slate before every test
            if (File.Exists(_alarmFile)) File.Delete(_alarmFile);
            if (File.Exists(_logFile)) File.Delete(_logFile);
            if (File.Exists(_barcodeFile)) File.Delete(_barcodeFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_alarmFile)) File.Delete(_alarmFile);
            if (File.Exists(_logFile)) File.Delete(_logFile);
            if (File.Exists(_barcodeFile)) File.Delete(_barcodeFile);
        }

        // =================================================================
        // 1. MOTOR & SAFETY INTEGRATION (US-2.1)
        // =================================================================

        [TestMethod]
        public void System_Motor_Starts_When_Safety_Clear()
        {
            // Arrange
            var hardware = new SimulatedHardware();
            var motor = new MotorController(hardware, hardware);

            // Act
            bool success = motor.Start();

            // Assert
            Assert.IsTrue(success, "Motor should return true on start success");
            Assert.IsTrue(hardware.IsRunning, "Hardware state should be Running");
        }

        [TestMethod]
        public void System_Motor_Stops_Immediately_On_EStop()
        {
            // Arrange
            var hardware = new SimulatedHardware();
            var motor = new MotorController(hardware, hardware);
            motor.Start();

            // Act: Simulate pressing E-Stop button physically
            hardware.EStop = true;

            // Try to start again (should fail)
            bool startResult = motor.Start();

            // Assert
            Assert.IsFalse(startResult, "Motor should refuse to start when EStop is active");
            // Note: In the real loop, we poll EStop. Here we verify the Controller respects the flag.
        }

        // 2. CONVEYOR & JAM INTEGRATION 

        [TestMethod]
        public void Conveyor_AutoStops_When_Jam_Detected()
        {
            // Arrange
            var hardware = new SimulatedHardware();
            var motor = new MotorController(hardware, hardware);
            var conveyor = new ConveyorController(motor, hardware);

            conveyor.Start();
            Assert.IsTrue(hardware.IsRunning, "Pre-condition: Motor running");

            // Act: Simulate Jam Sensor activating
            hardware.JamDetected = true;
            conveyor.CheckJam(); // This is called in the main loop

            // Assert
            Assert.IsFalse(hardware.IsRunning, "Motor should have auto-stopped due to jam");
            Assert.IsTrue(conveyor.JamActive, "Conveyor should be in JamActive state");
        }

        [TestMethod]
        public void Conveyor_Cannot_Restart_Until_Jam_Cleared()
        {
            // Arrange
            var hardware = new SimulatedHardware();
            var conveyor = new ConveyorController(new MotorController(hardware, hardware), hardware);

            // Create Jam Condition
            hardware.JamDetected = true;
            conveyor.CheckJam();

            // Act 1: Try to start while sensor is still blocked
            bool startWhileBlocked = conveyor.Start();

            // Act 2: Clear sensor, but forget to call ClearJam() logic
            hardware.JamDetected = false;
            bool startWithoutReset = conveyor.Start();

            // Act 3: Properly clear jam logic
            conveyor.ClearJam();
            bool startAfterReset = conveyor.Start();

            // Assert
            Assert.IsFalse(startWhileBlocked, "Should not start while sensor blocked");
            Assert.IsFalse(startWithoutReset, "Should not start before software jam reset");
            Assert.IsTrue(startAfterReset, "Should start after Jam Cleared");
        }

        // 3. ALARM SYSTEM INTEGRATION

        [TestMethod]
        public void Alarm_Service_Writes_To_Disk()
        {
            // Act
            Alarm.Raise("Test Alarm 123");

            // Assert
            Assert.IsTrue(File.Exists(_alarmFile), "Alarm file should be created");
            string content = File.ReadAllText(_alarmFile);
            StringAssert.Contains(content, "Test Alarm 123");
        }

        [TestMethod]
        public void EmergencyStop_Triggers_Both_Logs_And_Alarms()
        {
            // Act
            EmergencyStop.Estop("CRITICAL FAILURE");

            // Assert
            string alarmContent = File.ReadAllText(_alarmFile);
            string logContent = File.ReadAllText(_logFile);

            StringAssert.Contains(alarmContent, "CRITICAL FAILURE", "Should be in alarm.txt");
            StringAssert.Contains(logContent, "CRITICAL FAILURE", "Should be in logs.txt");
        }

        [TestMethod]
        public void Clearing_Jam_Clears_Specific_Alarm_Entry()
        {
            // Arrange
            Alarm.Raise("Other Fault");
            Alarm.Raise("Conveyor Jammed!");
            Alarm.Raise("Another Fault");

            // Act
            Alarm.Clear("Conveyor Jammed!");

            // Assert
            string[] lines = Alarm.ReadAll();
            Assert.AreEqual(2, lines.Length, "Should have removed the jam entry");
            Assert.IsFalse(lines[0].Contains("Conveyor Jammed!"));
        }

        // 4. ROUTING LOGIC INTEGRATION (US-2.4)

        [TestMethod]
        public void Routing_Calculates_Lane_And_Weight()
        {
            // Arrange
            var engine = new RoutingEngine();

            // Act
            var result = engine.Route("PKG-001", 55.0);

            // Assert
            Assert.AreEqual("BLOCKED", result.TargetLane, "Heavy package should be blocked");
            Assert.AreEqual(55.0, result.Weight);
        }

        [TestMethod]
        public void Barcode_Scanner_Event_Fires_Correctly()
        {
            // Arrange
            File.WriteAllText(_barcodeFile, "SCAN_ME");
            var scanner = new BarcodeScannerSensor(_barcodeFile);
            string? receivedBarcode = null;

            // Subscribe to event
            scanner.OnBarcodeScanned += (code) => receivedBarcode = code;

            // Act
            scanner.StartScanning();

            // Assert
            Assert.AreEqual("SCAN_ME", receivedBarcode);
        }

        // =================================================================
        // 5. ENVIRONMENT INTEGRATION (US-2.5)
        // =================================================================

        [TestMethod]
        public void Fan_Turns_On_When_Temp_Exceeds_Threshold()
        {
            // Arrange
            var fan = new FanController(onThreshold: 30.0, offThreshold: 25.0);

            // Act
            fan.UpdateTemperature(29.0); // Below threshold
            bool state1 = fan.IsOn;

            fan.UpdateTemperature(30.1); // Just above threshold
            bool state2 = fan.IsOn;

            // Assert
            Assert.IsFalse(state1, "Fan should be OFF initially");
            Assert.IsTrue(state2, "Fan should turn ON > 30.0");
        }

        [TestMethod]
        public void Fan_Stays_On_Until_Temp_Drops_Below_OffThreshold()
        {
            // Arrange (Hysteresis test)
            var fan = new FanController(30.0, 25.0);

            // Turn it on first
            fan.UpdateTemperature(31.0);
            Assert.IsTrue(fan.IsOn);

            // Act: Drop temp, but still above OFF threshold
            fan.UpdateTemperature(26.0);

            // Assert
            Assert.IsTrue(fan.IsOn, "Fan should stay ON due to hysteresis (26 > 25)");

            // Act: Drop below OFF threshold
            fan.UpdateTemperature(24.9);
            Assert.IsFalse(fan.IsOn, "Fan should finally turn OFF (< 25)");
        }

        [TestMethod]
        public void TemperatureSensor_Reads_From_CSV_Loop()
        {
            // Arrange
            File.WriteAllLines("test_temps.csv", new[] { "20.5", "21.5" });
            var sensor = TemperatureSensor.FromCsv("test_temps.csv");

            // Act & Assert
            Assert.AreEqual(20.5, sensor.ReadTemperature());
            Assert.AreEqual(21.5, sensor.ReadTemperature());
            Assert.AreEqual(20.5, sensor.ReadTemperature(), "Should loop back to start");
        }

        // =================================================================
        // 6. FULL SYSTEM SCENARIOS
        // =================================================================

        [TestMethod]
        public void Full_Scenario_Jam_During_Operation()
        {
            // Setup complete system
            var hw = new SimulatedHardware();
            var motor = new MotorController(hw, hw);
            var conveyor = new ConveyorController(motor, hw);

            // 1. Start Normal
            conveyor.Start();
            Assert.IsTrue(hw.IsRunning);

            // 2. Jam Happens
            hw.JamDetected = true;
            conveyor.CheckJam(); // Simulating the loop check

            // 3. System Reacts
            Assert.IsFalse(hw.IsRunning, "Motor Stopped");
            Assert.IsTrue(conveyor.JamActive, "Jam Active");

            // 4. Operator clears jam physically but hasn't reset software
            hw.JamDetected = false;
            Assert.IsFalse(conveyor.Start(), "Software lock prevents start");

            // 5. Operator Resets Software
            conveyor.ClearJam();

            // 6. System Ready
            Assert.IsFalse(conveyor.JamActive);
            // (Note: Auto-restart is technically unsafe, so user must press start again)
            conveyor.Start();
            Assert.IsTrue(hw.IsRunning, "Resumed");
        }

        [TestMethod]
        public void Full_Scenario_EStop_Overrides_Everything()
        {
            var hw = new SimulatedHardware();
            var motor = new MotorController(hw, hw);

            motor.Start();
            Assert.IsTrue(hw.IsRunning);

            // Safety Input Triggered
            hw.EStop = true;

            // Logic check (MotorController doesn't have a loop, so we simulate the next attempt to start/maintain)
            // In the real loop, we just stop it.
            motor.Stop();

            // Try to restart while EStop is held
            bool result = motor.Start();

            Assert.IsFalse(result);
            Assert.IsFalse(hw.IsRunning);
        }
    }

    [TestClass]
    public class SystemIntegrationTests
    {
        // --- Helper Method ---
        // Sets up the entire integrated system chain
        private WarehouseTransportSystem SetupIntegratedSystem(
            out FakeMotorDriver driver, out FakeSafety safety, out FakeJamSensor jamSensor)
        {
            driver = new FakeMotorDriver();
            safety = new FakeSafety();
            jamSensor = new FakeJamSensor();

            var motor = new MotorController(driver, safety);
            var conveyor = new ConveyorController(motor, jamSensor);
            return new WarehouseTransportSystem(motor, conveyor);
        }

        [TestMethod]
        public void System_StartAndStop_Successful()     // checks if start stop is successful
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);

            // ACT: start and then stop the integrated system
            system.StartSystem();
            Assert.IsTrue(driver.IsRunning);

            system.StopSystem();
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void System_StartTwice_MotorOnlyStartsOnce()  // confirms system prevents redundant motor start commands
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            // ACT: Call start system twice
            system.StartSystem();
            system.StartSystem();
            // Assert: Check the FakeDriver to confirm start was only called once
            Assert.AreEqual(1, driver.StartForwardCalls);
        }

        [TestMethod]
        public void System_StopWhenAlreadyStopped_DoesNotThrow() // confirms stop system() is safe to call multiple times
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            // ACT: stop system twice
            system.StopSystem();
            system.StopSystem();
            // Assert: Check state (passes if no exception is thrown)
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void System_Start_WhenEStopActive_ShouldBeBlocked() // Confirms E-stop block the high level start command
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            safety.EStop = true;
            // ACT: Attempt to start the system
            var startOk = system.StartSystem();
            // Assert: Verify start is blocked
            Assert.IsFalse(startOk);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void System_Start_WhenFaultActive_ShouldBeBlocked() // Confirms Motor Fault blocks the high level start command
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            safety.Fault = true;
            // ACT: Attempt to start the system
            var startOk = system.StartSystem();
            // Assert: Verify start is blocked
            Assert.IsFalse(startOk);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void System_EStop_WhileRunning_BlocksRestartAfterStop() // Confirms safety applies even when system is stopped
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            system.StopSystem();

            // Act: EStop is pressed while idle, then system tries to start
            safety.EStop = true;
            var startAttempt = system.StartSystem();
            // Assert: Verify start is still blocked
            Assert.IsFalse(startAttempt);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void FullSystem_JamDetection_StopsMotorImmediately() // Confirms jam detection triggers safe shutdown
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();

            // ACT: Simulate jam and system check
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            // Assert: Verify system reacted
            Assert.IsFalse(driver.IsRunning);
            Assert.IsTrue(system.IsJamActive);
        }

        [TestMethod]
        public void System_PollIssues_WhenNoJam_DoesNotStopMotor() // Confirms background poll does not interrupt normal run
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            var callsBefore = driver.StartForwardCalls;
            // ACT: System Poll check while running and safe
            system.PollSystemForIssues();
            // Assert: Verify motor state is unchanged
            Assert.IsTrue(driver.IsRunning);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
        }

        [TestMethod]
        public void System_Start_WhileJamActive_IsBlocked() // Confirms system cannot restart while jam flag is set
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            jamSensor.JamDetected = true;
            system.PollSystemForIssues(); // Sets JamActive = true
            // ACT: Attempt to start
            var startAttempt = system.StartSystem();
            // Assert: Verify start is blocked
            Assert.IsFalse(startAttempt);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void System_CannotClearJam_IfSensorStillDetected() // Confirms logical clearance is blocked by physical sensor
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            // ACT: Technician attempts to clear fault, BUT sensor is still TRUE
            system.ClearSystemFault();

            // Assert: Verify jam state remains active
            Assert.IsTrue(system.IsJamActive, "Jam state MUST NOT be cleared.");
            Assert.IsFalse(system.StartSystem(), "Start should still be blocked.");
        }

        [TestMethod]
        public void System_StatusReportsCorrectly_WhenRunning() // Confirms status properties work when system is operational
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            // Assert: Check the high level status properties
            Assert.IsTrue(system.IsRunning, "IsRunning status should be true.");
            Assert.IsFalse(system.IsJamActive, "IsJamActive status should be false.");
        }

        [TestMethod]
        public void System_StatusReportsCorrectly_WhenJamActive() // Confirms status properties reflect a fault state
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            // ACT: simulate jam
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();
            // Assert: Check the high-level status properties
            Assert.IsFalse(system.IsRunning, "IsRunning status should be false (motor stopped).");
            Assert.IsTrue(system.IsJamActive, "IsJamActive status should be true.");
        }

        [TestMethod]
        public void ITransportService_RequestMovement_StartsSystem() // Confirms Sorting can start system
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            ITransportService sortingService = system;
            bool success = sortingService.RequestMovement("PKG_001");

            Assert.IsTrue(success, "RequestMovement must start the system when safe.");
            Assert.IsTrue(driver.IsRunning, "Motor must be running.");
        }

        [TestMethod]
        public void ITransportService_HoldMovement_StopsSystem() // Confirms Sorting can stop system
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();

            ITransportService sortingService = system;
            sortingService.HoldMovement();

            Assert.IsFalse(driver.IsRunning, "HoldMovement must stop the motor.");
        }

        [TestMethod]
        public void ITransportService_IsAvailable_BlockedByJam() // Confirms Sorting is warned when jam is active   
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);

            // Simulate active jam (Motor stopped, JamActive = true)
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            ITransportService sortingService = system;

            Assert.IsFalse(sortingService.IsAvailableForTransport, "System should be unavailable when JamActive.");
        }

        [TestMethod]
        public void ITransportService_RequestMovement_BlockedByEStop() // Confirms safety blocks external start request
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            safety.EStop = true; // Simulate E-Stop pressed

            ITransportService sortingService = system;
            bool success = sortingService.RequestMovement("PKG_002");

            Assert.IsFalse(success, "RequestMovement must be blocked by EStop.");
            Assert.IsFalse(driver.IsRunning, "Motor must remain stopped.");
        }
    }
}
