namespace WareHouse_Management.Conveyor_and_Motor
{
    // Interface for controlling the motor hardware
    public interface IMotorDriver
    {
        bool IsRunning { get; }
        void StartForward();
    }

    // Interface for safety conditions (EStop and Fault)
    public interface ISafetyInputs
    {
        bool EStop { get; }
        bool Fault { get; }
    }

    // Main class that controls motor behavior
    public class MotorController
    {
        private readonly IMotorDriver _driver;
        private readonly ISafetyInputs _safety;

        // Constructor to connect motor driver and safety inputs
        public MotorController(IMotorDriver driver, ISafetyInputs safety)
        {
            _driver = driver;
            _safety = safety;
        }

        // Starts the motor only if EStop and Fault are not active
        public bool Start()
        {
            if (_safety.EStop || _safety.Fault)
                return false;

            _driver.StartForward();
            return true;
        }
    }
}
