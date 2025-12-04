using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WareHouse_Management; // sensor namespace

namespace Warehouse_Management_Test
{
    [TestClass]
    internal class BarcodeScannerSensorTests
    {
        [TestMethod]
        public void Scanner_StartScanning_DoesNotThrow_WhenFileMissing()
        {
            var sensor = new BarcodeScannerSensor("this_file_does_not_exist.txt");
            // Should not throw; it will print an error message
            sensor.StartScanning();
            Assert.IsTrue(true);
        }
    }
}
