using System;
using System.IO;

namespace WareHouse_Management.Alarm_and_Estop
{
    public static class Log
    {
        // File where general log messages will be written
        private static string logFile = "logs.txt";

        // Writes a message to the log file
        public static void Write(string message)
        {
            try
            {
                // Append timestamped log entry to the file
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine($"[{DateTime.Now}] {message}");
                }
            }
            catch
            {
                // Ignore logging errors for this demo
            }
        }
    }
}
