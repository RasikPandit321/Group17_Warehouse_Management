// Tests the collaboration between MotorController and ConveyorController.


using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.SystemIntegration; //  integration class location

namespace Warehouse_Management_Test.SystemIntegrationTests
{
    [TestClass]
    public class SystemIntegrationTests
    {
        // --- Helper Method ---
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

        // -------------------------------------------------------------
        // A. CORE OPERATIONAL TESTS
        // -------------------------------------------------------------

        [TestMethod]
        public void Test01_System_StartAndStop_Successful()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            Assert.IsTrue(driver.IsRunning);
            system.StopSystem();
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void Test02_System_StartTwice_MotorOnlyStartsOnce()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            system.StartSystem();
            Assert.AreEqual(1, driver.StartForwardCalls);
        }

        [TestMethod]
        public void Test03_System_StopWhenAlreadyStopped_DoesNotThrow()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StopSystem();
            system.StopSystem();
            Assert.IsFalse(driver.IsRunning);
        }

        // -------------------------------------------------------------
        // B. SAFETY AND BLOCKING TESTS
        // -------------------------------------------------------------

        [TestMethod]
        public void Test04_System_Start_WhenEStopActive_ShouldBeBlocked()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            safety.EStop = true;
            var startOk = system.StartSystem();
            Assert.IsFalse(startOk);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void Test05_System_Start_WhenFaultActive_ShouldBeBlocked()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            safety.Fault = true;
            var startOk = system.StartSystem();
            Assert.IsFalse(startOk);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void Test06_System_EStop_WhileRunning_BlocksRestartAfterStop()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            system.StopSystem();

            safety.EStop = true;
            var startAttempt = system.StartSystem();

            Assert.IsFalse(startAttempt);
            Assert.IsFalse(driver.IsRunning);
        }

        // -------------------------------------------------------------
        // C. JAM DETECTION AND RECOVERY TESTS
        // -------------------------------------------------------------

        [TestMethod]
        public void Test07_FullSystem_JamDetection_StopsMotorImmediately()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            Assert.IsFalse(driver.IsRunning);
            Assert.IsTrue(system.IsJamActive);
        }

        [TestMethod]
        public void Test08_System_PollIssues_WhenNoJam_DoesNotStopMotor()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            var callsBefore = driver.StartForwardCalls;

            system.PollSystemForIssues();

            Assert.IsTrue(driver.IsRunning);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
        }

        [TestMethod]
        public void Test09_System_Start_WhileJamActive_IsBlocked()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            jamSensor.JamDetected = true;
            system.PollSystemForIssues(); // Sets JamActive = true

            var startAttempt = system.StartSystem();

            Assert.IsFalse(startAttempt);
            Assert.IsFalse(driver.IsRunning);
        }

        [TestMethod]
        public void Test10_System_CannotClearJam_IfSensorStillDetected()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();
            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            // ACT: Technician attempts to clear fault, BUT sensor is still TRUE
            system.ClearSystemFault();

            Assert.IsTrue(system.IsJamActive, "Jam state MUST NOT be cleared.");
            Assert.IsFalse(system.StartSystem(), "Start should still be blocked.");
        }

        // -------------------------------------------------------------
        // D. STATUS AND FINAL INTEGRATION TESTS
        // -------------------------------------------------------------

        [TestMethod]
        public void Test11_System_StatusReportsCorrectly_WhenRunning()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();

            Assert.IsTrue(system.IsRunning, "IsRunning status should be true.");
            Assert.IsFalse(system.IsJamActive, "IsJamActive status should be false.");
        }

        [TestMethod]
        public void Test12_System_StatusReportsCorrectly_WhenJamActive()
        {
            var system = SetupIntegratedSystem(out var driver, out var safety, out var jamSensor);
            system.StartSystem();

            jamSensor.JamDetected = true;
            system.PollSystemForIssues();

            Assert.IsFalse(system.IsRunning, "IsRunning status should be false (motor stopped).");
            Assert.IsTrue(system.IsJamActive, "IsJamActive status should be true.");
        }
    }
}