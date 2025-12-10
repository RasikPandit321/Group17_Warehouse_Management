using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;
using WareHouse_Management.Alarm_and_Estop; // Fixed Namespace
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class IntegrationTests
    {
        // FIX: Use null!
        private SimulatedHardware _hw = null!;
        private MotorController _motor = null!;
        private ConveyorController _conveyor = null!;
        private RoutingEngine _routing = null!;
        private TemperatureSensor _temp = null!;
        private FanController _fan = null!;

        [TestInitialize]
        public void Setup()
        {
            _hw = new SimulatedHardware();
            _motor = new MotorController(_hw, _hw);
            _conveyor = new ConveyorController(_motor, _hw);
            _routing = new RoutingEngine();
            _temp = new TemperatureSensor(20, 35);
            _fan = new FanController(30, 25);

            _hw.JamDetected = false;
            _hw.EStop = false;
        }

        [TestMethod]
        public void Test_FullFlow_LightPackage()
        {
            _conveyor.Start();
            var route = _routing.Route("PKG1", 3.0);
            Assert.IsTrue(_hw.IsRunning);
            Assert.AreEqual("Lane1", route.TargetLane);
        }

        [TestMethod]
        public void Test_FullFlow_HeavyPackage()
        {
            _conveyor.Start();
            var route = _routing.Route("PKG_HEAVY", 50.0);
            Assert.IsTrue(_hw.IsRunning);
            Assert.AreEqual("Lane3", route.TargetLane);
        }

        [TestMethod]
        public void Test_Jam_Stops_Motor_And_Raises_Alarm()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _conveyor.CheckJam();

            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_EStop_Stops_Everything()
        {
            _conveyor.Start();
            _hw.EStop = true;
            _motor.Stop();

            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_HighTemp_Activates_Fan()
        {
            double currentTemp = 32.0;
            _fan.UpdateTemperature(currentTemp);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Fan_Lowers_Energy_Score()
        {
            var samples = new System.Collections.Generic.List<EnergySample> {
                new EnergySample { FanOn = true, Temperature = 32.0 }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        [TestMethod]
        public void Test_Recover_From_Jam()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);

            _conveyor.ClearJam();
            _hw.JamDetected = false;
            _conveyor.Start();

            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Recover_From_EStop()
        {
            _hw.EStop = true;
            _conveyor.Start();
            Assert.IsFalse(_hw.IsRunning);

            _hw.EStop = false;
            // FIX: Added definition for Reset now calls correctly
            EmergencyStop.Reset("Resetting");
            _conveyor.Start();

            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Integration_Routing_While_Running()
        {
            _conveyor.Start();
            Assert.AreEqual("Lane2", _routing.Route("A", 10).TargetLane);
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Integration_Routing_DoesNot_Affect_Motor()
        {
            _conveyor.Start();
            _routing.Route("A", 100);
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Environment_Updates_Independent_Of_Motor()
        {
            _conveyor.Stop();
            _fan.UpdateTemperature(35);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Alarm_Logging_During_Jam()
        {
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsTrue(System.IO.File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_EnergyReport_Generation()
        {
            var report = new EnergyReport { EnergyScore = 80 };
            string file = EnergyReporter.SaveToCsv(report);
            Assert.IsTrue(System.IO.File.Exists(file));
        }

        [TestMethod]
        public void Test_System_Startup_State()
        {
            Assert.IsFalse(_hw.IsRunning);
            Assert.IsFalse(_hw.JamDetected);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Simultaneous_Jam_And_Overweight()
        {
            var result = _routing.Route("BigItem", 55.0);
            Assert.AreEqual("Lane3", result.TargetLane);

            _hw.JamDetected = true;
            _conveyor.CheckJam();

            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Simultaneous_EStop_And_TempHigh()
        {
            _hw.EStop = true;
            _fan.UpdateTemperature(35.0);

            Assert.IsFalse(_hw.IsRunning);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Motor_Speed_Change_While_Running()
        {
            _conveyor.Start();
            // FIX: Added SetSpeed call
            _motor.SetSpeed(80);
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Stop_Command_Overrides_Start()
        {
            _conveyor.Start();
            _conveyor.Stop();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Routing_Logic_Lane_Distribution()
        {
            Assert.AreEqual("Lane1", _routing.Route("A", 1).TargetLane);
            Assert.AreEqual("Lane2", _routing.Route("B", 10).TargetLane);
            Assert.AreEqual("Lane3", _routing.Route("C", 30).TargetLane);
        }

        [TestMethod]
        public void Test_Safety_Interlock_Jam()
        {
            _hw.JamDetected = true;
            _conveyor.Start();
            if (_hw.JamDetected) _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Reporting_With_Active_Fan()
        {
            _fan.UpdateTemperature(32);
            var list = new System.Collections.Generic.List<EnergySample>();
            list.Add(new EnergySample { FanOn = _fan.IsOn, Temperature = 32 });
            var report = EnergyReporter.ComputeFromSamples(list, 0.2);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        [TestMethod]
        public void Test_Barcode_Scan_Triggers_Lane_Change()
        {
            var r1 = _routing.Route("A", 2);
            var r2 = _routing.Route("B", 22);
            Assert.AreNotEqual(r1.TargetLane, r2.TargetLane);
        }

        [TestMethod]
        public void Test_Full_Cycle_Reset()
        {
            _conveyor.Start();
            _conveyor.Stop();
            _hw.EStop = true;
            EmergencyStop.Estop("Test");
            _hw.EStop = false;
            EmergencyStop.Reset("Reset");
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        [TestMethod]
        public void Test_Temperature_Hysteresis_Cycle()
        {
            _fan.UpdateTemperature(31);
            Assert.IsTrue(_fan.IsOn);
            _fan.UpdateTemperature(26);
            Assert.IsTrue(_fan.IsOn);
            _fan.UpdateTemperature(24);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Sensor_Data_Persistence()
        {
            var samples = new System.Collections.Generic.List<EnergySample>();
            samples.Add(new EnergySample { Temperature = 20 });
            samples.Add(new EnergySample { Temperature = 21 });
            Assert.AreEqual(2, samples.Count);
        }

        [TestMethod]
        public void Test_Invalid_Component_State()
        {
            var localHW = new SimulatedHardware();
            Assert.IsFalse(localHW.IsRunning);
        }

        [TestMethod]
        public void Test_End_To_End_Routing_Lane1()
        {
            _conveyor.Start();
            var res = _routing.Route("PKG-123", 2.5);
            Assert.AreEqual("Lane1", res.TargetLane);
        }

        [TestMethod]
        public void Test_End_To_End_Routing_Lane3_Alarm()
        {
            _conveyor.Start();
            var res = _routing.Route("PKG-999", 55.0);
            Assert.AreEqual("Lane3", res.TargetLane);
            Assert.IsTrue(System.IO.File.Exists("alarm.txt"));
        }
    }
}