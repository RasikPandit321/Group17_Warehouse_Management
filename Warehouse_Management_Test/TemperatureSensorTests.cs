using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
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
            Assert.ThrowsException<System.ArgumentException>(() =>
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
