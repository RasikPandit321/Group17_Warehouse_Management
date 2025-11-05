using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class MotorControllerTests
    {

        //--------Test 1: Motor should start normally when system is safe.------
        [TestMethod]
        public void Start_WhenSafe_SetsMotorRunning()
        {
            // Arrange
            // dummy motor driver and safety inputs
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            // create object under test
            var motor = new MotorController(driver, safety);

            // Act
            // Try starting the motor
            var result = motor.Start();

            // Assert
            Assert.IsTrue(result, "Motor should start successfully when no EStop or Fault is active.");
            Assert.IsTrue(driver.IsRunning, "Driver should indicate motor is running.");
        }


        //--- Motor must not start when emergency stop is active.----
        [TestMethod]
        public void Start_WhenEStopActive_ShouldNotRun()
        {
            // Arrange
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety { EStop = true };
            var motor = new MotorController(driver, safety);

            // Act
            var result = motor.Start();

            // Assert
            // Motor refuse to start
            Assert.IsFalse(result, "Motor should not start if EStop is active.");
            // Driver should still show that the motor is stopped
            Assert.IsFalse(driver.IsRunning, "Driver should remain stopped when EStop is active.");
        }
    }
    //--- Placeholder implementation of MotorController.----
    public class MotorController
    {
        private readonly FakeMotorDriver _driver;  // controls motor
        private readonly FakeSafety _safety;       // provide safety inputs


        // constrcutor to connect driver and safety components
        public MotorController(FakeMotorDriver driver, FakeSafety safety)
        {
            _driver = driver;
            _safety = safety;
        }

        public bool Start()
        {
            throw new NotImplementedException();
        }
    }
    //---- Mimics the real motor.----
    public sealed class FakeMotorDriver
    {

        // indicated if the motor is currently running
        public bool IsRunning { get; private set; }

        // simulates starting the motor forward direction
        public void StartForward() => IsRunning = true;
    }

    // simulates safety inputs 
    public sealed class FakeSafety
    {
        // emergency stop signal
        public bool EStop { get; set; }
        // fault signal 
        public bool Fault { get; set; }
    }
}
