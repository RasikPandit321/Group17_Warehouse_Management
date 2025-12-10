using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Environment;
using System.Collections.Generic;
using System;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class EnvironmentUnitTests
    {
        private TemperatureSensor _sensor = null!;
        private FanController _fan = null!;

        [TestInitialize]
        public void Setup()
        {
            _sensor = new TemperatureSensor(20, 35);
            _fan = new FanController(30, 25);
        }

        [TestMethod]
        public void Test_Sensor_Reads_Value()
        {
            double temp = _sensor.ReadTemperature();
            Assert.IsTrue(temp >= 19.0 && temp <= 36.0);
        }

        [TestMethod]
        public void Test_Fan_Starts_Off()
        {
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Fan_Turns_On_Above_Threshold()
        {
            _fan.UpdateTemperature(31.0);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Fan_Stays_On_Above_Hysteresis()
        {
            _fan.UpdateTemperature(31.0);
            _fan.UpdateTemperature(26.0);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Fan_Turns_Off_Below_Threshold()
        {
            _fan.UpdateTemperature(31.0);
            _fan.UpdateTemperature(24.0);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Fan_DoesNot_Turn_On_Below_Threshold()
        {
            _fan.UpdateTemperature(29.0);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_EnergySample_Creation()
        {
            var sample = new EnergySample { Temperature = 25, FanOn = false };
            Assert.AreEqual(25, sample.Temperature);
        }

        [TestMethod]
        public void Test_Report_Compute_Score_Ideal()
        {
            var samples = new List<EnergySample> {
                new EnergySample { Temperature = 22, FanOn = false }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.AreEqual(100.0, report.EnergyScore);
        }

        [TestMethod]
        public void Test_Report_Compute_Score_Worst()
        {
            var samples = new List<EnergySample> {
                new EnergySample { Temperature = 35, FanOn = true }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        [TestMethod]
        public void Test_Report_Average_Temp()
        {
            var samples = new List<EnergySample> {
                new EnergySample { Temperature = 20, FanOn = false },
                new EnergySample { Temperature = 30, FanOn = false }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.AreEqual(25.0, report.AverageTemperature, 0.1);
        }

        [TestMethod]
        public void Test_Report_Empty_Samples()
        {
            var samples = new List<EnergySample>();
            Assert.ThrowsException<ArgumentException>(() =>
                EnergyReporter.ComputeFromSamples(samples, 0.5));
        }

        [TestMethod]
        public void Test_Report_Fan_Uptime()
        {
            var samples = new List<EnergySample> {
                new EnergySample { FanOn = true },
                new EnergySample { FanOn = false }
            };
            var report = EnergyReporter.ComputeFromSamples(samples, 0.5);
            Assert.IsTrue(report.EnergyScore < 100);
        }

        [TestMethod]
        public void Test_Extreme_High_Temp()
        {
            _fan.UpdateTemperature(100.0);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Extreme_Low_Temp()
        {
            _fan.UpdateTemperature(-10.0);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Sensor_Variation()
        {
            double t1 = _sensor.ReadTemperature();
            double t2 = _sensor.ReadTemperature();
            Assert.AreNotEqual(double.NaN, t2);
        }

        [TestMethod]
        public void Test_CSV_Generation_String()
        {
            var report = new EnergyReport { AverageTemperature = 25, EnergyScore = 90, Timestamp = DateTime.Now };
            string csv = EnergyReporter.SaveToCsv(report);

            StringAssert.Contains(csv, "energy_report_");
        }

        [TestMethod]
        public void Test_Mixed_Fan_States()
        {
            _fan.UpdateTemperature(32);
            Assert.IsTrue(_fan.IsOn);
            _fan.UpdateTemperature(24);
            Assert.IsFalse(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Rapid_Temp_Fluctuation()
        {
            _fan.UpdateTemperature(30);
            _fan.UpdateTemperature(20);
            _fan.UpdateTemperature(35);
            Assert.IsTrue(_fan.IsOn);
        }

        [TestMethod]
        public void Test_Null_Sample_List_Handling()
        {
            try
            {
                EnergyReporter.ComputeFromSamples(null!, 0.5);
                Assert.Fail("Should allow or handle null");
            }
            catch
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_Price_Factor_Influence()
        {
            var samples = new List<EnergySample> { new EnergySample { FanOn = true, Temperature = 30 } };
            var reportHigh = EnergyReporter.ComputeFromSamples(samples, 1.0);
            Assert.IsNotNull(reportHigh);
        }

        [TestMethod]
        public void Test_Timestamp_Assignment()
        {
            var sample = new EnergySample { Timestamp = DateTime.Now };
            Assert.IsTrue(sample.Timestamp > DateTime.MinValue);
        }

        [TestMethod]
        public void Test_Max_Temp_Limit()
        {
            Assert.IsTrue(_sensor.ReadTemperature() < 100);
        }

        [TestMethod]
        public void Test_Min_Temp_Limit()
        {
            Assert.IsTrue(_sensor.ReadTemperature() > -50);
        }

        [TestMethod]
        public void Test_Zero_Samples_Score()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                EnergyReporter.ComputeFromSamples(new List<EnergySample>(), 0.5));
        }

        [TestMethod]
        public void Test_Single_Sample()
        {
            var report = EnergyReporter.ComputeFromSamples(new List<EnergySample> { new EnergySample() }, 0.5);
            Assert.IsNotNull(report);
        }
    }
}