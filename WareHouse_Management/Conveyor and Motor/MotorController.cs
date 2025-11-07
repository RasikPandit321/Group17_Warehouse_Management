// -----------------------------------------------------------------------------
// Purpose: Start/Stop the motor safely 
// -----------------------------------------------------------------------------

namespace WareHouse_Management.Conveyor_and_Motor
{
    // Motor driver abstraction (real hardware or a fake in tests).
    public interface IMotorDriver
    {
        bool IsRunning { get; }
        void StartForward();
        void Stop();
    }

    // Safety inputs used to decide if starting is allowed.
    public interface ISafetyInputs
    {
        bool EStop { get; }   // Emergency stop pressed?
        bool Fault { get; }   // Any fault active?
    }
    // Controls the motor with safety checks.
    public class MotorController
    {
        private readonly IMotorDriver _driver;
        private readonly ISafetyInputs _safety;

        // Inject driver (hardware) and safety inputs.
        public MotorController(IMotorDriver driver, ISafetyInputs safety)
        {
            _driver = driver;
            _safety = safety;
        }

   
        // Start only when safe. If already running, return true without re-starting.     
        public bool Start()
        {
            // Block start if unsafe
            if (_safety.EStop || _safety.Fault)
                return false;

            // Don't double-start
            if (_driver.IsRunning)
                return true;

            // Safe to start
            _driver.StartForward();
            return true;
        }

        /// Stop motor (safe to call multiple times).
        public void Stop() => _driver.Stop();
    }
}
