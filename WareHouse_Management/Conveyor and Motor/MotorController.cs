namespace WareHouse_Management.Conveyor_and_Motor
{
    // Motor driver abstraction
    public interface IMotorDriver
    {
        bool IsRunning { get; }
        void StartForward();
        void Stop();
    }

    // Safety inputs used to decide if starting is allowed.
    public interface ISafetyInputs
    {
        bool EStop { get; }
        bool Fault { get; }
    }

    // Controls the motor with safety checks.
    public class MotorController
    {
        private readonly IMotorDriver _driver;
        private readonly ISafetyInputs _safety;
        public bool IsRunning => _driver.IsRunning;

        public MotorController(IMotorDriver driver, ISafetyInputs safety)
        {
            _driver = driver;
            _safety = safety;
        }

        public bool Start()
        {
            if (_safety.EStop || _safety.Fault)
                return false;

            if (_driver.IsRunning)
                return true;

            _driver.StartForward();
            return true;
        }

        public void Stop() => _driver.Stop();
    }
}