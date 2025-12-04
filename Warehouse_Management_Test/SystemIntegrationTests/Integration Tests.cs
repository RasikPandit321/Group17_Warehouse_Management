using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.SystemIntegration;
using WareHouse_Management.Interfaces;

namespace Warehouse_Management_Test.SystemIntegrationTests
{
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