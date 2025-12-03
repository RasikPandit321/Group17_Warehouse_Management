using WareHouse_Management.Conveyor_and_Motor;
using WareHouse_Management.Interfaces; // Reference to the interface

namespace WareHouse_Management.SystemIntegration
{
    // implements the corrected ITransportService interface
    public class WarehouseTransportSystem : ITransportService
    {
        private readonly MotorController _motor;
        private readonly ConveyorController _conveyor;

        public WarehouseTransportSystem(MotorController motor, ConveyorController conveyor)
        {
            _motor = motor;
            _conveyor = conveyor;
        }

        // --- Core Technician Control Methods ---

        public bool StartSystem()
        {
            return _conveyor.Start();
        }

        public void StopSystem()
        {
            _conveyor.Stop();
        }

        public void PollSystemForIssues()
        {
            _conveyor.CheckJam();
        }

        public void ClearSystemFault()
        {
            _conveyor.ClearJam();
        }

        // --- HMI Status Properties ---
        public bool IsRunning => _motor.IsRunning;

        public bool IsJamActive => _conveyor.JamActive;

        public string GetSystemStatus()
        {
            if (IsJamActive) return "FAULT: JAM ACTIVE.";
            if (IsRunning) return "RUNNING OK";

            // Check if motor is capable of starting to determine idle vs. safety-blocked
            // temporarily call _motor.Start() just to check its safety condition without starting it.
            if (!_motor.Start()) return "STOPPED: SAFETY LOCK ACTIVE.";

            return "IDLE: Ready to Start.";
        }

        // ----------------------------------------------------------------------
        // --- ITransportService IMPLEMENTATION (For Sorting Module) ---
        // ----------------------------------------------------------------------

        public bool RequestMovement(string packageId)
        {
            // Sorting requests movement. We only proceed if the system is ready.
            if (IsAvailableForTransport) // 
            {
                return StartSystem();
            }
            return false;
        }

        public void HoldMovement()
        {
            StopSystem();
        }

        // Implementation of the ITransportService property
        public bool IsAvailableForTransport
        {
            get
            {
                // System is available if not jammed AND not currently running.
                return !IsJamActive && !IsRunning;
            }
        }
    }
}