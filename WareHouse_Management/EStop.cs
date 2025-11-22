using System;
using LogService;
using AlarmService;
using static AlarmService.Alarm;

public static class EmergencyStop
{
    public static void Estop(string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrWhiteSpace(message)) return;

        // Call the static Raise method on the external class
        Raise(message);
        Log.Archive(message);
        // call conveyor stop

        return;
    }
}