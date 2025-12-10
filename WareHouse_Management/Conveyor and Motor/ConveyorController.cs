using System;
using WareHouse_Management.Alarm_and_Estop;

namespace WareHouse_Management.Conveyor_and_Motor
{
    public class ConveyorController
    {
        private MotorController _motor;      // Motor control interface
        private SimulatedHardware _hardware; // Simulated hardware status flags

        // Constructor injects the motor controller and hardware monitoring object
        public ConveyorController(MotorController motor, SimulatedHardware hardware)
        {
            _motor = motor;
            _hardware = hardware;
        }

        // Attempts to start the conveyor only if no jam or emergency stop is active
        public void Start()
        {
            if (_hardware.JamDetected || _hardware.EStop)
            {
                Console.WriteLine("Cannot start: Jammed or E-Stopped");
                return;
            }
            _motor.Start(); // Start the motor
        }

        // Immediately stops the conveyor motor
        public void Stop()
        {
            _motor.Stop();
        }

        // Checks for jams; if detected, stop motor and trigger alarm
        public void CheckJam()
        {
            if (_hardware.JamDetected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[CONVEYOR] Jam Detected! Stopping Motor.");
                Console.ResetColor();

                _motor.Stop();               // Stop the motor
                Alarm.Raise("Conveyor Jammed"); // Raise an alarm
            }
        }

        // Clears the jam flag in the simulated hardware state
        public void ClearJam()
        {
            _hardware.JamDetected = false;
            Console.WriteLine("[CONVEYOR] Jam Cleared.");
        }
    }
}
