using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class MotorController_MoreTests
    {
        [TestMethod]
        public void Start_WhenFaultActive_ShouldNotRun()
        {
            // Arrange
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety { Fault = true }; // fault ON
            var motor = new MotorController(driver, safety);

            // Act
            var ok = motor.Start();

            // Assert
            Assert.IsFalse(ok, "Motor should not start when Fault is active.");
            Assert.IsFalse(driver.IsRunning, "Driver should remain stopped when Fault is active.");
            Assert.AreEqual(0, driver.StartForwardCalls, "StartForward should not be called.");
        }

        [TestMethod]
        public void Start_WhenSafe_CallsStartForwardExactlyOnce()
        {
            // Arrange
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety(); // safe
            var motor = new MotorController(driver, safety);

            // Act
            var ok = motor.Start();

            // Assert
            Assert.IsTrue(ok, "Start should succeed under safe conditions.");
            Assert.IsTrue(driver.IsRunning, "Driver should report running after start.");
            Assert.AreEqual(1, driver.StartForwardCalls, "StartForward should be called exactly once.");
        }
    }

    // --- simple fakes used by these tests ---
    public sealed class FakeMotorDriver : IMotorDriver
    {
        public bool IsRunning { get; private set; }
        public int StartForwardCalls { get; private set; }

        public void StartForward()
        {
            IsRunning = true;
            StartForwardCalls++;
        }
    }

    public sealed class FakeSafety : ISafetyInputs
    {
        public bool EStop { get; set; }
        public bool Fault { get; set; }
    }
}
