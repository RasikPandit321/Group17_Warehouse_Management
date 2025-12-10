using System;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class EmergencyStop
    {
        public static void Estop(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[EMERGENCY STOP] {reason}");
            Console.ResetColor();
            Log.Write($"[E-STOP] {reason}");
        }

        public static void Reset(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SYSTEM RESET] {reason}");
            Console.ResetColor();
            Log.Write($"[RESET] {reason}");
        }
    }
}