using System;

namespace WareHouse_Management.Environment
{
    public class EnergySample
    {
        public double Temperature { get; set; }
        public bool FanOn { get; set; }
        // FIX: Added Timestamp property to resolve CS0117
        public DateTime Timestamp { get; set; }
    }
}