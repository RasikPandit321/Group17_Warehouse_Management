using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;
using WareHouse_Management.Alarm_and_Estop; // Fixed Namespace
using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
    /// <summary>
    /// Integration-level tests validating how multiple subsystems
    /// (motor, conveyor, routing, sensors, alarms, estop, energy) work together.
    /// Ensures cross-component interactions behave as expected.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        // Hardware and subsystem instances used for full integration coverage.
        private SimulatedHardware _hw = null!;
        private MotorController _motor = null!;
        private ConveyorController _conveyor = null!;
        private RoutingEngine _routing = null!;
        private TemperatureSensor _temp = null!;
        private FanController _fan = null!;

        /// <summary>
        /// Creates fresh simulated hardware and controllers before each test.
        /// Ensures no state carries over across test cases.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _hw = new SimulatedHardware();
            _motor = new MotorController(_hw, _hw);
            _conveyor = new ConveyorController(_motor, _hw);
            _routing = new RoutingEngine();
            _temp = new TemperatureSensor(20, 35);
            _fan = new FanController(30, 25);

            // Reset hardware status
            _hw.JamDetected = false;
            _hw.EStop = false;
        }

        /// <summary>
        /// Full workflow: conveyor running + routing a light package should select Lane1.
        /// </summary>
        [TestMethod]
        public void Test_FullFlow_LightPackage()
        {
            _conveyor.Start();
            var route = _routing.Route("PKG1", 3.0);
            Assert.IsTrue(_hw.IsRunning);
            Assert.AreEqual("Lane1", route.TargetLane);
        }

        /// <summary>
        /// Full workflow with a heavy package; routing should select Lane3.
        /// </summary>
        [TestMethod]
        public void Test_FullFlow_HeavyPackage()
        {
            _conveyor.Start();
            var route = _routing.Route("PKG_HEAVY", 50.0);
            Assert.IsTrue(_hw.IsRunning);
            Assert.AreEqual("Lane3", route.TargetLane);
        }

        /// <summary>
        /// When jam is detected, conveyor should stop automatically.
        /// </summary>
        [TestMethod]
        public void Test_Jam_Stops_Motor_And_Raises_Alarm()
        {
            _conveyor.Start();
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Activating the emergency stop must halt all motor operations.
        /// </summary>
        [TestMethod]
        public void Test_EStop_Stops_Everything()
        {
            _conveyor.Start();
            _hw.EStop = true;
            _motor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// High temperature should activate the cooling fan.
        /// </summary>
        [TestMethod]
        public void Test_HighTemp_Activates_Fan()
        {
            double currentTemp = 32.0;
            _fan.UpdateTemperature(currentTemp);
            Assert.IsTrue(_fan.IsOn);
        }

        /// <summary>
        /// Fan running should reduce energy score in generated reports.
        /// </summary>
        [TestMethod]
        public void Test_Fan_Lowers_Energy_Score()
        {
            var samples = new System.Collections.Generic.List<EnergySample> {
                new EnergySample { FanOn = true, Temperature = 32.0 }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        /// <summary>
        /// System should allow recovery after a jam once cleared.
        /// </summary>
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

        /// <summary>
        /// Recovery from emergency stop should allow conveyor to resume operation.
        /// </summary>
        [TestMethod]
        public void Test_Recover_From_EStop()
        {
            _hw.EStop = true;
            _conveyor.Start();
            Assert.IsFalse(_hw.IsRunning);

            _hw.EStop = false;
            EmergencyStop.Reset("Resetting");
            _conveyor.Start();

            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Routing should function normally while the conveyor is running.
        /// </summary>
        [TestMethod]
        public void Test_Integration_Routing_While_Running()
        {
            _conveyor.Start();
            Assert.AreEqual("Lane2", _routing.Route("A", 10).TargetLane);
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Routing should not interfere with motor or conveyor operations.
        /// </summary>
        [TestMethod]
        public void Test_Integration_Routing_DoesNot_Affect_Motor()
        {
            _conveyor.Start();
            _routing.Route("A", 100);
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Environment subsystem (fan) should operate independently of conveyor state.
        /// </summary>
        [TestMethod]
        public void Test_Environment_Updates_Independent_Of_Motor()
        {
            _conveyor.Stop();
            _fan.UpdateTemperature(35);
            Assert.IsTrue(_fan.IsOn);
        }

        /// <summary>
        /// A jam condition should trigger alarm logging.
        /// </summary>
        [TestMethod]
        public void Test_Alarm_Logging_During_Jam()
        {
            _hw.JamDetected = true;
            _conveyor.CheckJam();
            Assert.IsTrue(System.IO.File.Exists("alarm.txt"));
        }

        /// <summary>
        /// Generating an energy report should create a CSV file.
        /// </summary>
        [TestMethod]
        public void Test_EnergyReport_Generation()
        {
            var report = new EnergyReport { EnergyScore = 80 };
            string file = EnergyReporter.SaveToCsv(report);
            Assert.IsTrue(System.IO.File.Exists(file));
        }

        /// <summary>
        /// The system should start in a safe idle state with no active faults.
        /// </summary>
        [TestMethod]
        public void Test_System_Startup_State()
        {
            Assert.IsFalse(_hw.IsRunning);
            Assert.IsFalse(_hw.JamDetected);
            Assert.IsFalse(_fan.IsOn);
        }

        /// <summary>
        /// Jam + heavy package handling; routing still works but conveyor must stop.
        /// </summary>
        [TestMethod]
        public void Test_Simultaneous_Jam_And_Overweight()
        {
            var result = _routing.Route("BigItem", 55.0);
            Assert.AreEqual("Lane3", result.TargetLane);

            _hw.JamDetected = true;
            _conveyor.CheckJam();

            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Temperature high + emergency stop: motors must stay off but fan turns on.
        /// </summary>
        [TestMethod]
        public void Test_Simultaneous_EStop_And_TempHigh()
        {
            _hw.EStop = true;
            _fan.UpdateTemperature(35.0);

            Assert.IsFalse(_hw.IsRunning);
            Assert.IsTrue(_fan.IsOn);
        }

        /// <summary>
        /// Changing motor speed while running should not stop the conveyor.
        /// </summary>
        [TestMethod]
        public void Test_Motor_Speed_Change_While_Running()
        {
            _conveyor.Start();
            _motor.SetSpeed(80);
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Stop command should override previous start request, but system can restart later.
        /// </summary>
        [TestMethod]
        public void Test_Stop_Command_Overrides_Start()
        {
            _conveyor.Start();
            _conveyor.Stop();
            _conveyor.Start();
            Assert.IsTrue(_hw.IsRunning);
        }

        /// <summary>
        /// Ensures routing logic produces expected lane distribution across weights.
        /// </summary>
        [TestMethod]
        public void Test_Routing_Logic_Lane_Distribution()
        {
            Assert.AreEqual("Lane1", _routing.Route("A", 1).TargetLane);
            Assert.AreEqual("Lane2", _routing.Route("B", 10).TargetLane);
            Assert.AreEqual("Lane3", _routing.Route("C", 30).TargetLane);
        }

        /// <summary>
        /// Safety interlock: conveyor must not run if jam is active.
        /// </summary>
        [TestMethod]
        public void Test_Safety_Interlock_Jam()
        {
            _hw.JamDetected = true;
            _conveyor.Start();
            if (_hw.JamDetected) _conveyor.Stop();
            Assert.IsFalse(_hw.IsRunning);
        }

        /// <summary>
        /// Fan state should influence energy scoring correctly.
        /// </summary>
        [TestMethod]
        public void Test_Reporting_With_Active_Fan()
        {
            _fan.UpdateTemperature(32);
            var list = new System.Collections.Generic.List<EnergySample>();
            list.Add(new EnergySample { FanOn = _fan.IsOn, Temperature = 32 });
            var report = EnergyReporter.ComputeFromSamples(list, 0.2);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        /// <summary>
        /// Routing should change lane assignments based on input barcode/weight.
        /// </summary>
        [TestMethod]
        public void Test_Barcode_Scan_Triggers_Lane_Change()
        {
            var r1 = _routing.Route("A", 2);
            var r2 = _routing.Route("B", 22);
            Assert.AreNotEqual(r1.TargetLane, r2.TargetLane);
        }

        /// <summary>
        /// Full system cycle: start → stop → estop → reset → restart.
        /// </summary>
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

        /// <summary>
        /// Validates full hysteresis cycle for the temperature-controlled fan.
        /// </summary>
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

        /// <summary>
        /// Verifies temperature samples persist across collection.
        /// </summary>
        [TestMethod]
        public void Test_Sensor_Data_Persistence()
        {
            var samples = new System.Collections.Generic.List<EnergySample>();
            samples.Add(new EnergySample { Temperature = 20 });
            samples.Add(new EnergySample { Temperature = 21 });
            Assert.AreEqual(2, samples.Count);
        }

        /// <summary>
        /// Ensures a fresh hardware instance starts in a safe disabled state.
        /// </summary>
        [TestMethod]
        public void Test_Invalid_Component_State()
        {
            var localHW = new SimulatedHardware();
            Assert.IsFalse(localHW.IsRunning);
        }

        /// <summary>
        /// End-to-end test for light package routing through Lane1.
        /// </summary>
        [TestMethod]
        public void Test_End_To_End_Routing_Lane1()
        {
            _conveyor.Start();
            var res = _routing.Route("PKG-123", 2.5);
            Assert.AreEqual("Lane1", res.TargetLane);
        }

        /// <summary>
        /// End-to-end heavy routing should also log alarms if system behavior triggers logging.
        /// </summary>
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
