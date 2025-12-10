using System;
using System.IO;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class Log
    {
        private static string logFile = "logs.txt";

        public static void Write(string message)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine($"[{DateTime.Now}] {message}");
                }
            }
            catch { /* Ignore logging errors in demo */ }
        }
    }
}