using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;

namespace Warehouse.Tests
{
    [TestClass]
    public class MockBarcodeReaderTests
    {
        [TestMethod]
        public void MockReader_ShouldReturnNextBarcode()
        {
            var mock = new MockBarcodeReader(new[] { "PKG1", "PKG2", "PKG3" });

            Assert.AreEqual("PKG1", mock.Read());
            Assert.AreEqual("PKG2", mock.Read());
            Assert.AreEqual("PKG3", mock.Read());
        }

        [TestMethod]
        public void MockReader_ShouldLoop_WhenOutOfBarcodes()
        {
            var mock = new MockBarcodeReader(new[] { "A", "B" });

            mock.Read(); // A
            mock.Read(); // B
            var third = mock.Read(); // loops → A again

            Assert.AreEqual("A", third);
        }
    }
}
