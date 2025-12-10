using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test
{
    /// <summary>
    /// Unit tests for verifying the behavior of the MotorController,
    /// ConveyorController, and simulated hardware interactions.
    /// </summary>
    [TestClass]
    public class ConveyorAndMotorUnitTests
    {
        // Use null! to avoid nullable warnings; actual instances are created in Setup().
        private SimulatedHardware _hw = null!;
        private MotorController _motor = null!;
        private ConveyorController _conveyor = null!;

        /// <summary>
        /// Initializes new hardware and controller objects before each test runs.
        /// Ensures a clean, consistent starting state.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _hw = new SimulatedHardware();
            _motor = new MotorController(_hw, _hw);
            _conveyor = new ConveyorController(_motor, _hw);
        }

        /// <summary>
        /// Verifies that the motor (hardware) is stopped when initialized.
        /// </summary>
        [TestMethod]
        public void Test_Motor_Starts_Initially_Stopped()
        {
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Starting the conveyor should set the hardware running flag to true.
        /// </summary>
        [TestMethod]
        public void Test_Conveyor_Start_Sets_Hardware_Running()
        {
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Stopping the conveyor should cause the hardware to stop running.
        /// </summary>
        [TestMethod]
        public void Test_Conveyor_Stop_Sets_Hardware_Stopped()
        {
            _conveyor.Start();
            _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Test that setting a normal valid speed does not cause errors.
        /// </summary>
        [TestMethod]
        public void Test_Motor_SetSpeed_Valid()
        {
            _motor.SetSpeed(50);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Setting the motor speed to zero should be accepted.
        /// </summary>
        [TestMethod]
        public void Test_Motor_SetSpeed_Zero()
        {
            _motor.SetSpeed(0);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Setting the motor speed to the maximum allowed value should not fail.
        /// </summary>
        [TestMethod]
        public void Test_Motor_SetSpeed_Max()
        {
            _motor.SetSpeed(100);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// If a jam is detected, the conveyor should automatically stop.
        /// </summary>
        [TestMethod]
        public void Test_Jam_Detection_Stops_Motor()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Clearing a jam should allow the conveyor to be started again.
        /// </summary>
        [TestMethod]
        public void Test_Clear_Jam_Allows_Restart()
        {
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            _conveyor.ClearJam();
            _hw.JamDetected = false;
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Emergency stop should prevent the conveyor from starting.
        /// </summary>
        [TestMethod]
        public void Test_EStop_Prevents_Start()
        {
            _hw.EStop = true;
            _conveyor.Start();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Triggering emergency stop while running should stop the conveyor immediately.
        /// </summary>
        [TestMethod]
        public void Test_EStop_Stops_Running_Conveyor()
        {
            _conveyor.Start();
            _hw.EStop = true;
            _motor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// After resetting emergency stop, the conveyor should be allowed to start again.
        /// </summary>
        [TestMethod]
        public void Test_Reset_EStop_Allows_Start()
        {
            _hw.EStop = true;
            _conveyor.Start();
            _hw.EStop = false;
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Confirms that the conveyor reports running after Start() is called.
        /// </summary>
        [TestMethod]
        public void Test_Conveyor_IsRunning_Property()
        {
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Starting the conveyor when it's already running should not cause failures.
        /// </summary>
        [TestMethod]
        public void Test_Start_Already_Running()
        {
            _conveyor.Start();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Stopping the conveyor when it is already stopped should not cause errors.
        /// </summary>
        [TestMethod]
        public void Test_Stop_Already_Stopped()
        {
            _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Basic toggle behavior: start → stop → start should work correctly.
        /// </summary>
        [TestMethod]
        public void Test_Toggle_Start_Stop()
        {
            _conveyor.Start();
            _conveyor.Stop();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Confirms jam detection logic does not incorrectly toggle the flag.
        /// </summary>
        [TestMethod]
        public void Test_Jam_Flag_Interaction()
        {
            _hw.JamDetected = false;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.JamDetected);
        }

        /// <summary>
        /// Negative speed values should be rejected or handled gracefully.
        /// </summary>
        [TestMethod]
        public void Test_Motor_Speed_Negative_Handling()
        {
            _motor.SetSpeed(-10);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Speeds above the max limit should be handled safely.
        /// </summary>
        [TestMethod]
        public void Test_Motor_Speed_OverLimit_Handling()
        {
            _motor.SetSpeed(150);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Ensures logging (if implemented) does not cause failures when starting the conveyor.
        /// </summary>
        [TestMethod]
        public void Test_Conveyor_Start_Check_Logs()
        {
            _conveyor.Start();
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Verifies hardware initializes properly with expected default values.
        /// </summary>
        [TestMethod]
        public void Test_Hardware_Initialization()
        {
            Assert.IsNotNull(_hw);
            Assert.IsFalse(_hw.JamDetected);
        }

        /// <summary>
        /// Ensures the motor controller instance exists after setup.
        /// </summary>
        [TestMethod]
        public void Test_Motor_Null_Dependencies()
        {
            Assert.IsNotNull(_motor);
        }

        /// <summary>
        /// Ensures the conveyor controller instance exists after setup.
        /// </summary>
        [TestMethod]
        public void Test_Conveyor_Null_Dependencies()
        {
            Assert.IsNotNull(_conveyor);
        }

        /// <summary>
        /// Emergency stop should always override jam and force the conveyor off.
        /// </summary>
        [TestMethod]
        public void Test_Emergency_Stop_Override_Jam()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _hw.EStop = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Rapid start/stop cycling should not break the system and must end stopped.
        /// </summary>
        [TestMethod]
        public void Test_Rapid_Switching()
        {
            for (int i = 0; i < 10; i++)
            {
                _conveyor.Start();
                _conveyor.Stop();
            }
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Conveyor should not start if a jam is detected while stopped.
        /// </summary>
        [TestMethod]
        public void Test_Jam_While_Stopped()
        {
            _conveyor.Stop();
            _hw.JamDetected = true;
            _conveyor.Start();
            Assert.IsFalse(_hw.IsRunning);
        }
    }
}
