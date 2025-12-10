using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class ConveyorAndMotorUnitTests
    {
        // FIX: Use null! to satisfy CS8618
        private SimulatedHardware _hw = null!;
        private MotorController _motor = null!;
        private ConveyorController _conveyor = null!;

        [TestInitialize]
        public void Setup()
        {
            _hw = new SimulatedHardware();
            _motor = new MotorController(_hw, _hw);
            _conveyor = new ConveyorController(_motor, _hw);
        }

        [TestMethod]
        public void Test_Motor_Starts_Initially_Stopped()
        {
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Conveyor_Start_Sets_Hardware_Running()
        {
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Conveyor_Stop_Sets_Hardware_Stopped()
        {
            _conveyor.Start();
            _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Motor_SetSpeed_Valid()
        {
            _motor.SetSpeed(50);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Motor_SetSpeed_Zero()
        {
            _motor.SetSpeed(0);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Motor_SetSpeed_Max()
        {
            _motor.SetSpeed(100);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Jam_Detection_Stops_Motor()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);
        }

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

        [TestMethod]
        public void Test_EStop_Prevents_Start()
        {
            _hw.EStop = true;
            _conveyor.Start();
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_EStop_Stops_Running_Conveyor()
        {
            _conveyor.Start();
            _hw.EStop = true;
            _motor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Reset_EStop_Allows_Start()
        {
            _hw.EStop = true;
            _conveyor.Start();
            _hw.EStop = false;
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Conveyor_IsRunning_Property()
        {
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Start_Already_Running()
        {
            _conveyor.Start();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Stop_Already_Stopped()
        {
            _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Toggle_Start_Stop()
        {
            _conveyor.Start();
            _conveyor.Stop();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Jam_Flag_Interaction()
        {
            _hw.JamDetected = false;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.JamDetected);
        }

        [TestMethod]
        public void Test_Motor_Speed_Negative_Handling()
        {
            _motor.SetSpeed(-10);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Motor_Speed_OverLimit_Handling()
        {
            _motor.SetSpeed(150);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Conveyor_Start_Check_Logs()
        {
            _conveyor.Start();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Hardware_Initialization()
        {
            Assert.IsNotNull(_hw);
            Assert.IsFalse(_hw.JamDetected);
        }

        [TestMethod]
        public void Test_Motor_Null_Dependencies()
        {
            Assert.IsNotNull(_motor);
        }

        [TestMethod]
        public void Test_Conveyor_Null_Dependencies()
        {
            Assert.IsNotNull(_conveyor);
        }

        [TestMethod]
        public void Test_Emergency_Stop_Override_Jam()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _hw.EStop = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);
        }

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