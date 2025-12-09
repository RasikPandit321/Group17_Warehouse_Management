using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
    // ===============================
    //       ENERGY REPORTER TESTS
    // ===============================
    [TestClass]
    public class EnergyReporterTests
    {
        [TestMethod]
        public void ComputeFromSamples_ComputesExpectedAveragesAndScore()
        {
            // Arrange: 4 samples, fan ON for 2 of them
            var samples = new List<EnergySample>
            {
                new EnergySample { Temperature = 25.0, FanOn = false },
                new EnergySample { Temperature = 26.0, FanOn = true },
                new EnergySample { Temperature = 27.0, FanOn = true },
                new EnergySample { Temperature = 28.0, FanOn = false },
            };

            double intervalSeconds = 1.0;

            // Act
            var report = EnergyReporter.ComputeFromSamples(samples, intervalSeconds);

            // Assert
            Assert.AreEqual(26.5, report.AverageTemperature, 0.01, "Average temperature should be correct.");
            Assert.AreEqual(2.0, report.TotalFanOnSeconds, 0.01, "Total fan ON time should be 2 seconds.");
            Assert.AreEqual(2.0, report.AverageFanRuntimeSeconds, 0.01, "Average ON burst should be 2 seconds.");
            Assert.AreEqual(50.0, report.FanOnPercent, 0.1, "Fan ON percentage should be 50%.");

            // EnergyScore = 100 - (avgTemp - 22) - (fanPercent / 2)
            //             = 100 - (26.5 - 22) - (50 / 2)
            //             = 100 - 4.5 - 25 = 70.5
            Assert.AreEqual(70.5, report.EnergyScore, 0.1, "Energy score should follow the defined formula.");
        }

        [TestMethod]
        public void ComputeFromSamples_NoFanOn_ResultsInHighScore()
        {
            var samples = new List<EnergySample>
            {
                new EnergySample { Temperature = 23.0, FanOn = false },
                new EnergySample { Temperature = 24.0, FanOn = false },
                new EnergySample { Temperature = 23.5, FanOn = false },
            };

            var report = EnergyReporter.ComputeFromSamples(samples, sampleIntervalSeconds: 1.0);

            Assert.AreEqual(23.5, report.AverageTemperature, 0.01);
            Assert.AreEqual(0.0, report.TotalFanOnSeconds, 0.01);
            Assert.AreEqual(0.0, report.AverageFanRuntimeSeconds, 0.01);
            Assert.AreEqual(0.0, report.FanOnPercent, 0.1);
            Assert.IsTrue(report.EnergyScore > 90.0, "With cool temps and no fan usage, score should be high.");
        }

        [TestMethod]
        public void SaveToCsv_CreatesFileAndWritesData()
        {
            // Arrange
            var report = new EnergyReport
            {
                AverageTemperature = 25.0,
                AverageFanRuntimeSeconds = 2.0,
                TotalFanOnSeconds = 4.0,
                FanOnPercent = 50.0,
                EnergyScore = 80.0,
                Timestamp = new DateTime(2025, 12, 2)
            };

            string directory = Path.GetTempPath();

            // Act
            string path = EnergyReporter.SaveToCsv(report, directory);

            // Assert
            Assert.IsTrue(File.Exists(path), "CSV file should exist after SaveToCsv.");

            try
            {
                string content = File.ReadAllText(path);
                StringAssert.Contains(content, "AvgTempC");
                StringAssert.Contains(content, "25.00");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    // ===============================
    //        FAN CONTROLLER TESTS
    // ===============================
    [TestClass]
    public class FanControllerTests
    {
        // Threshold tests (Task 40)

        [TestMethod]
        public void Fan_TurnsOn_When_TempAboveOnThreshold()
        {
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            fan.UpdateTemperature(31);

            Assert.IsTrue(fan.IsOn, "Fan should turn ON when temperature goes above 30°C.");
        }

        [TestMethod]
        public void Fan_TurnsOff_When_TempBelowOffThreshold()
        {
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            fan.UpdateTemperature(31);  // turn on
            fan.UpdateTemperature(24);  // drop below off threshold

            Assert.IsFalse(fan.IsOn, "Fan should turn OFF when temperature falls below 25°C.");
        }

        [TestMethod]
        public void Fan_DoesNotTurnOn_AtExactlyOnThreshold()
        {
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            fan.UpdateTemperature(30);

            Assert.IsFalse(fan.IsOn, "Fan must stay OFF at exactly 30°C (only ON above).");
        }

        // Rising / falling behaviour (Task 42)

        [TestMethod]
        public void Fan_RespondsCorrectly_ToRisingAndFallingTemperatures()
        {
            var fan = new FanController(onThreshold: 30, offThreshold: 25);

            fan.UpdateTemperature(24);
            Assert.IsFalse(fan.IsOn, "Fan should be OFF at 24°C.");

            fan.UpdateTemperature(29);
            Assert.IsFalse(fan.IsOn, "Fan should still be OFF at 29°C.");

            fan.UpdateTemperature(31);
            Assert.IsTrue(fan.IsOn, "Fan should turn ON at 31°C.");

            fan.UpdateTemperature(28);
            Assert.IsTrue(fan.IsOn, "Fan should remain ON at 28°C due to hysteresis.");

            fan.UpdateTemperature(24);
            Assert.IsFalse(fan.IsOn, "Fan should turn OFF again at 24°C.");
        }
    }

    // ===============================
    //    TEMPERATURE SENSOR TESTS
    // ===============================
    [TestClass]
    public class TemperatureSensorTests
    {
        [TestMethod]
        public void ReadTemperature_ReturnsValueWithinConfiguredRange()
        {
            // Arrange
            double min = 10;
            double max = 40;
            var sensor = new TemperatureSensor(min, max);

            // Act + Assert
            for (int i = 0; i < 50; i++)
            {
                double t = sensor.ReadTemperature();
                Assert.IsTrue(t >= min && t <= max,
                    $"Temperature {t} was outside expected range [{min}, {max}]");
            }
        }

        [TestMethod]
        public void Constructor_Throws_WhenMinIsNotLessThanMax()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new TemperatureSensor(30, 20);
            });
        }

        [TestMethod]
        public void CsvMode_ReadsValuesFromCsvFile()
        {
            // Arrange – create a small temp CSV
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path, new[] { "21.5", "23.0", "24.7" });

                var sensor = TemperatureSensor.FromCsv(path);

                double t1 = sensor.ReadTemperature();
                double t2 = sensor.ReadTemperature();
                double t3 = sensor.ReadTemperature();

                Assert.AreEqual(21.5, t1, 0.01);
                Assert.AreEqual(23.0, t2, 0.01);
                Assert.AreEqual(24.7, t3, 0.01);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
