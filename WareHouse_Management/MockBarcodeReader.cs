namespace Warehouse
{
    public class MockBarcodeReader
    {
        private readonly string[] _barcodes;
        private int _index = 0;

        public MockBarcodeReader(string[] barcodes)
        {
            _barcodes = barcodes ?? new string[0];
        }

        public string Read()
        {
            if (_barcodes.Length == 0) return string.Empty;
            var value = _barcodes[_index];
            _index = (_index + 1) % _barcodes.Length;
            return value;
        }
    }
}
