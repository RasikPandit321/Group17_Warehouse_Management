using System;
using System.Linq;
using System.Text;

namespace LogService
{
    public class Logs
    {
        // Always use a fixed file named "alarm.txt" in the app working directory
        public string FilePath { get; }

        public Logs()
        {
            FilePath = Path.GetFullPath("logs.txt");
        }
        // Append a Log message to the file. 
        public void Archive(string message)
        {
            // If the caller passed null, treat it as an empty string
            message ??= string.Empty;

            // Make sure the directory that will contain the file exists
            EnsureDirectory();

            // Always append to the fixed file
            var mode = FileMode.Append;

            // Open the file for writing. FileShare.Read allows other code to read the file
            // while we have it open, but prevents other writers from opening it for write.
            using var fs = new FileStream(FilePath, mode, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs, Encoding.UTF8);
            // Prepend a timestamp (customize format as needed)
            sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            Console.WriteLine(message + " was logged");
        }
        // Print current alarms to the console. Prints a message if none exist.
        public void PrintLogs()
        {
            var lines = ReadAll();
            if (lines.Length == 0)
            {
                Console.WriteLine("(no logs)");
                return;
            }

            foreach (var line in lines)
                Console.WriteLine(line);
        }
        // Read all logs; returns empty array if file does not exist
        public string[] ReadAll()
        {
            if (!File.Exists(FilePath))
                return Array.Empty<string>();
            return File.ReadAllLines(FilePath, Encoding.UTF8);
        }

        private void EnsureDirectory()
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (string.IsNullOrEmpty(dir)) return;
            Directory.CreateDirectory(dir);
        }

    }
    public static class Log
    {
        private static readonly Logs _default = new Logs();
        public static void Archive(string message) => _default.Archive(message);
        public static string[] ReadAll() => _default.ReadAll();
        public static void PrintLogs() => _default.PrintLogs();
    }
}