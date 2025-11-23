using System;
using WareHouse_Management.Conveyor_and_Motor;

namespace WareHouse_Management
{
    // This class acts as the physical motor, safety switches, and sensors for the simulation.
    public class SimulatedHardware : IMotorDriver, ISafetyInputs, IJamSensor
    {
        // State
        public bool IsRunning { get; private set; } = false;
        public bool EStop { get; set; } = false;
        public bool Fault { get; set; } = false;
        public bool JamDetected { get; set; } = false;

        // IMotorDriver Implementation
        public void StartForward()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                Console.WriteLine("   [HARDWARE] ⚙️ MOTOR STARTED (Forward)");
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                Console.WriteLine("   [HARDWARE] 🛑 MOTOR STOPPED");
            }
        }
    }
}