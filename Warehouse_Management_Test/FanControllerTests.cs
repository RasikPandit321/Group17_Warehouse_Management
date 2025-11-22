using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Environment;

namespace Warehouse_Management_Test
{
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
}
