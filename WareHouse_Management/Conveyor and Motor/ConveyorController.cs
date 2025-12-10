using System;
using WareHouse_Management.Alarm_and_Estop;

namespace WareHouse_Management.Conveyor_and_Motor
{
    public class ConveyorController
    {
        private MotorController _motor;
        private SimulatedHardware _hardware;

        // FIX: Constructor uses concrete types
        public ConveyorController(MotorController motor, SimulatedHardware hardware)
        {
            _motor = motor;
            _hardware = hardware;
        }

        public void Start()
        {
            if (_hardware.JamDetected || _hardware.EStop)
            {
                Console.WriteLine("Cannot start: Jammed or E-Stopped");
                return;
            }
            _motor.Start();
        }

        public void Stop()
        {
            _motor.Stop();
        }

        // FIX: Changed return type to void to fix CS0029
        public void CheckJam()
        {
            if (_hardware.JamDetected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[CONVEYOR] Jam Detected! Stopping Motor.");
                Console.ResetColor();
                _motor.Stop();
                Alarm.Raise("Conveyor Jammed");
            }
        }

        public void ClearJam()
        {
            _hardware.JamDetected = false;
            Console.WriteLine("[CONVEYOR] Jam Cleared.");
        }
    }
}