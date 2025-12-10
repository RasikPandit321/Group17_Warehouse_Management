using System;

namespace WareHouse_Management.Conveyor_and_Motor
{
    public class MotorController
    {
        private SimulatedHardware _hardware;

        // Dependency injection for hardware state
        public MotorController(SimulatedHardware hardware, SimulatedHardware jamSensor)
        {
            _hardware = hardware;
        }

        // Sets the hardware running flag to true if safety permits
        public void Start()
        {
            if (_hardware.EStop)
            {
                Console.WriteLine("Cannot start: E-Stop Active");
                return;
            }
            _hardware.IsRunning = true;
        }

        // Sets the hardware running flag to false
        public void Stop()
        {
            _hardware.IsRunning = false;
        }

        // Updates the speed setting (clamped between 0-100)
        public void SetSpeed(int speed)
        {
            if (speed < 0) speed = 0;
            if (speed > 100) speed = 100;
            // In a real system, this would write to a VFD register
        }
    }
}