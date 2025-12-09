using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test
{
    // ====================================================================
    // 1. CONVEYOR CONTROLLER UNIT TESTS
    // ====================================================================

    [TestClass]
    public class ConveyorControllerTests
    {
        // 1) Start should run motor when no jam and safety is OK
        [TestMethod]
        public void Start_WhenNoJamAndSafe_ShouldStartMotor()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor { JamDetected = false };
            var conveyor = new ConveyorController(motor, jamSensor);

            // ACT
            var ok = conveyor.Start();

            // ASSERT
            Assert.IsTrue(ok);
            Assert.IsTrue(driver.IsRunning);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 2) Jam detected while running should stop motor and set jam flag
        [TestMethod]
        public void CheckJam_WhenJamDetected_ShouldStopMotorAndSetJamActive()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;

            // ACT
            conveyor.CheckJam();

            // ASSERT
            Assert.IsTrue(conveyor.JamActive);
            Assert.IsFalse(driver.IsRunning);
        }

        // 3) When jam active, Start should not restart motor
        [TestMethod]
        public void Start_WhenJamActive_ShouldNotStartMotor()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;
            conveyor.CheckJam(); // JamActive = true

            var callsBefore = driver.StartForwardCalls;

            // ACT
            var ok = conveyor.Start();

            // ASSERT
            Assert.IsFalse(ok);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
            Assert.IsFalse(driver.IsRunning);
        }

        // 4) After clearing jam, Start should work again
        [TestMethod]
        public void ClearJam_ThenStart_ShouldAllowMotorToRunAgain()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;
            conveyor.CheckJam(); // JamActive = true

            // ACT
            jamSensor.JamDetected = false;
            conveyor.ClearJam(); // JamActive = false

            var ok = conveyor.Start();

            // ASSERT
            Assert.IsTrue(ok);
            Assert.IsTrue(driver.IsRunning);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 5) CheckJam when no jam should not stop motor
        [TestMethod]
        public void CheckJam_WhenNoJam_ShouldNotChangeMotorState()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor { JamDetected = false };
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            var callsBefore = driver.StartForwardCalls;

            // ACT
            conveyor.CheckJam();

            // ASSERT
            Assert.IsTrue(driver.IsRunning);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 6) Stop should always stop motor
        [TestMethod]
        public void Stop_ShouldStopMotor()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();

            // ACT
            conveyor.Stop();

            // ASSERT
            Assert.IsFalse(driver.IsRunning);
        }
    }


    // ====================================================================
    // 2. MOTOR CONTROLLER UNIT TESTS
    // ====================================================================

    [TestClass]
    public class MotorControllerTests
    {
        // Motor should start successfully when safe
        [TestMethod]
        public void Start_WhenSafe_ShouldReturnTrue_AndRunMotor()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety { EStop = false, Fault = false };
            var motor = new MotorController(driver, safety);

            // ACT
            var ok = motor.Start();

            // ASSERT
            Assert.IsTrue(ok, "Motor should start when no EStop or Fault is active.");
            Assert.IsTrue(driver.IsRunning, "Driver should show motor running.");
        }

        // Motor should not start when EStop is active
        [TestMethod]
        public void Start_WhenEStopActive_ShouldReturnFalse_AndNotRun()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety { EStop = true, Fault = false };
            var motor = new MotorController(driver, safety);

            // ACT
            var ok = motor.Start();

            // ASSERT
            Assert.IsFalse(ok, "Motor should not start when EStop is pressed.");
            Assert.IsFalse(driver.IsRunning);
        }

        // Motor should not start when Fault is active
        [TestMethod]
        public void Start_WhenFaultActive_ShouldReturnFalse_AndNotRun()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety { EStop = false, Fault = true };
            var motor = new MotorController(driver, safety);

            // ACT
            var ok = motor.Start();

            // ASSERT
            Assert.IsFalse(ok, "Motor should not start when a fault is active.");
            Assert.IsFalse(driver.IsRunning);
        }

        // Motor should stop after starting
        [TestMethod]
        public void Stop_AfterStart_ShouldLeaveMotorNotRunning()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);

            motor.Start();

            // ACT
            motor.Stop();

            // ASSERT
            Assert.IsFalse(driver.IsRunning, "Motor should stop after Stop() is called.");
        }

        // Stop should not throw when already stopped
        [TestMethod]
        public void Stop_WhenAlreadyStopped_ShouldNotThrow()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);

            // ACT
            motor.Stop();

            // ASSERT
            Assert.IsFalse(driver.IsRunning);
        }

        // Start should only trigger once when already running
        [TestMethod]
        public void Start_Twice_ShouldCallStartForwardOnlyOnce()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);

            // ACT
            motor.Start();
            motor.Start();

            // ASSERT
            Assert.AreEqual(1, driver.StartForwardCalls, "StartForward should be called only once.");
        }

        // Motor can start again after being stopped
        [TestMethod]
        public void Start_AfterStop_ShouldStartAgain()
        {
            // ARRANGE
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);

            // ACT
            motor.Start();
            motor.Stop();
            motor.Start();

            // ASSERT
            Assert.AreEqual(2, driver.StartForwardCalls, "Motor should be able to start again after stopping.");
        }
    }


    // ====================================================================
    // 3. FAKE/MOCK CLASSES (Required for Testing)
    // ====================================================================

    // Fake class for IMotorDriver
    public sealed class FakeMotorDriver : IMotorDriver
    {
        public bool IsRunning { get; private set; }
        public int StartForwardCalls { get; private set; }

        public void StartForward()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                StartForwardCalls++;
            }
        }

        public void Stop() => IsRunning = false;
    }

    // Fake class for ISafetyInputs
    public sealed class FakeSafety : ISafetyInputs
    {
        public bool EStop { get; set; }
        public bool Fault { get; set; }
    }

    // Fake class for IJamSensor
    public sealed class FakeJamSensor : IJamSensor
    {
        public bool JamDetected { get; set; }
    }
}