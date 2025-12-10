using System;

namespace WareHouse_Management.Conveyor_and_Motor
{
    public class MotorController
    {
        private SimulatedHardware _hardware;

        // FIX: Remove 'IJamSensor' and use 'SimulatedHardware'
        // We accept it twice to match your Program.cs call: new MotorController(hardware, hardware)
        public MotorController(SimulatedHardware hardware, SimulatedHardware jamSensor)
        {
            _hardware = hardware;
        }

        public void Start()
        {
            if (_hardware.EStop)
            {
                Console.WriteLine("Cannot start: E-Stop Active");
                return;
            }
            _hardware.IsRunning = true;
        }

        public void Stop()
        {
            _hardware.IsRunning = false;
        }

        public void SetSpeed(int speed)
        {
            if (speed < 0) speed = 0;
            if (speed > 100) speed = 100;
            // Speed logic would go here
        }
    }
}