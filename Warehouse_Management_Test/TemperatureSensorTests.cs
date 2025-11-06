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
    }
}
