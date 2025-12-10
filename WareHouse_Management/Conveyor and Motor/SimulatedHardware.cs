using System;

namespace WareHouse_Management.Conveyor_and_Motor
{

    public class SimulatedHardware
    {
        public bool IsRunning { get; set; }
        public bool JamDetected { get; set; }
        public bool EStop { get; set; }

        public SimulatedHardware()
        {
            IsRunning = false;
            JamDetected = false;
            EStop = false;
        }
    }
}