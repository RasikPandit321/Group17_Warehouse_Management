using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class ConveyorControllerTests
    {
        // 1) Start should run motor when no jam and safety is OK
        [TestMethod]
        public void Start_WhenNoJamAndSafe_ShouldStartMotor()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor { JamDetected = false };
            var conveyor = new ConveyorController(motor, jamSensor);

            var ok = conveyor.Start();

            Assert.IsTrue(ok);
            Assert.IsTrue(driver.IsRunning);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 2) Jam detected while running should stop motor and set jam flag
        [TestMethod]
        public void CheckJam_WhenJamDetected_ShouldStopMotorAndSetJamActive()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;

            conveyor.CheckJam();

            Assert.IsTrue(conveyor.JamActive);
            Assert.IsFalse(driver.IsRunning);
        }

        // 3) When jam active, Start should not restart motor
        [TestMethod]
        public void Start_WhenJamActive_ShouldNotStartMotor()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;
            conveyor.CheckJam(); // JamActive = true

            var callsBefore = driver.StartForwardCalls;

            var ok = conveyor.Start();

            Assert.IsFalse(ok);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
            Assert.IsFalse(driver.IsRunning);
        }

        // 4) After clearing jam, Start should work again
        [TestMethod]
        public void ClearJam_ThenStart_ShouldAllowMotorToRunAgain()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            jamSensor.JamDetected = true;
            conveyor.CheckJam();  // JamActive = true

            jamSensor.JamDetected = false;
            conveyor.ClearJam();  // JamActive = false

            var ok = conveyor.Start();

            Assert.IsTrue(ok);
            Assert.IsTrue(driver.IsRunning);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 5) CheckJam when no jam should not stop motor
        [TestMethod]
        public void CheckJam_WhenNoJam_ShouldNotChangeMotorState()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor { JamDetected = false };
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();
            var callsBefore = driver.StartForwardCalls;

            conveyor.CheckJam();

            Assert.IsTrue(driver.IsRunning);
            Assert.AreEqual(callsBefore, driver.StartForwardCalls);
            Assert.IsFalse(conveyor.JamActive);
        }

        // 6) Stop should always stop motor
        [TestMethod]
        public void Stop_ShouldStopMotor()
        {
            var driver = new FakeMotorDriver();
            var safety = new FakeSafety();
            var motor = new MotorController(driver, safety);
            var jamSensor = new FakeJamSensor();
            var conveyor = new ConveyorController(motor, jamSensor);

            conveyor.Start();

            conveyor.Stop();

            Assert.IsFalse(driver.IsRunning);
        }
    }
}


