using System;
using System.IO;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class Alarm
    {
        private static string alarmFile = "alarm.txt"; // Log file path

        public static void Raise(string message)
        {
            try
            {
                // Append alarm message to log file
                using (StreamWriter sw = File.AppendText(alarmFile))
                {
                    sw.WriteLine($"[{DateTime.Now}] ALARM: {message}");
                }

                // Play beep sound on Windows
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(450, 1000); // 1 second beep
                }
            }
            catch { } // Ignore write errors
        }
    }
}
