using System;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class EmergencyStop
    {
        public static void Estop(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Red; // Set text color to red
            Console.WriteLine($"[EMERGENCY STOP] {reason}"); // Display E-Stop message
            Console.ResetColor(); // Reset console colors
            Log.Write($"[E-STOP] {reason}"); // Log the event
        }

        public static void Reset(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Green; // Set text color to green
            Console.WriteLine($"[SYSTEM RESET] {reason}"); // Display reset message
            Console.ResetColor(); // Reset console colors
            Log.Write($"[RESET] {reason}"); // Log the reset event
        }
    }
}
