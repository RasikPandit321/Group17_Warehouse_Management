// Purpose: Conveyor controller with jam detection

namespace WareHouse_Management.Conveyor_and_Motor
{
    // Jam sensor abstraction (real sensor or fake in tests).
    public interface IJamSensor
    {
        bool JamDetected { get; }
    }

    // Coordinates motor controller with jam logic.
    public class ConveyorController
    {
        private readonly MotorController _motor;
        private readonly IJamSensor _jamSensor;

        // True when a jam has been detected.
        public bool JamActive { get; private set; }

        public ConveyorController(MotorController motor, IJamSensor jamSensor)
        {
            _motor = motor;
            _jamSensor = jamSensor;
            JamActive = false;
        }

        // Start conveyor only if no jam is active.
        public bool Start()
        {
            if (JamActive)
            {
                return false; // cannot start during jam
            }

            // MotorController already checks EStop and Fault.
            return _motor.Start();
        }

        // Stop conveyor (just stop the motor).
        public void Stop()
        {
            _motor.Stop();
        }

        // Check for jam and automatically stop if detected.
        public void CheckJam()
        {
            if (_jamSensor.JamDetected)
            {
                JamActive = true;
                _motor.Stop();
            }
        }

        // Clear jam state so the conveyor can be started again.
        public void ClearJam()
        {
            // Only clear jam if sensor no longer detects jam
            if (!_jamSensor.JamDetected)
            {
                JamActive = false;
            }

        }
    }
}