namespace WareHouse_Management.Interfaces
{
    public interface ITransportService
    {
        // Requests the system to start movement for a package.
        // <returns>True if the system started; false if blocked by a fault.</returns>
        bool RequestMovement(string packageId);
        // Commands the system to halt movement (e.g., reached a sorting gate).
        void HoldMovement();

        // --- Status Checks (Properties) ---
        // True if the line is currently idle and ready to accept a new package.
        bool IsAvailableForTransport { get; }
        // True if the motor/conveyor system is currently running.
        bool IsRunning { get; }
    }
}