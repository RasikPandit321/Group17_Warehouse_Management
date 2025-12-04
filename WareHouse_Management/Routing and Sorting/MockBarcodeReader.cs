using System;

namespace Warehouse
{
    /// <summary>
    /// Cycles through a preset list of barcodes for testing purposes.
    /// </summary>
    public class MockBarcodeReader
    {
        private readonly string[] _barcodes;
        private int _index;

        public MockBarcodeReader(string[] barcodes)
        {
            _barcodes = barcodes;
        }

        public string Read()
        {
            if (_barcodes.Length == 0)
                return string.Empty;

            var result = _barcodes[_index];
            _index = (_index + 1) % _barcodes.Length;
            return result;
        }
    }
}