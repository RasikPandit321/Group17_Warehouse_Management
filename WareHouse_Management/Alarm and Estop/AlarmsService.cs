using System;
using System.IO;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class Alarm
    {
        private static string alarmFile = "alarm.txt";

        public static void Raise(string message)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(alarmFile))
                {
                    sw.WriteLine($"[{DateTime.Now}] ALARM: {message}");
                }

                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(450, 1000); // 1 second beep
                }
            }
            catch { }
        }
    }
}