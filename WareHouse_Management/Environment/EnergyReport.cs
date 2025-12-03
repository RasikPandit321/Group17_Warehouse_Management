using System;

namespace WareHouse_Management.Environment
{
    public class EnergyReport
    {
        public double AverageTemperature { get; set; }
        public double AverageFanRuntimeSeconds { get; set; }
        public double TotalFanOnSeconds { get; set; }
        public double FanOnPercent { get; set; }
        public double EnergyScore { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
