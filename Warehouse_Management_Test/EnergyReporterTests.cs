using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
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
}
