using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AlarmService
{
    public class Alarms
    {
        // Always use a fixed file named "alarm.txt" in the app working directory
        public string FilePath { get; }

        public Alarms()
        {
            FilePath = Path.GetFullPath("alarm.txt");
        }

        // Append an alarm message to the file. Caller should include newline if desired.
        public void Raise(string message)
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

            sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            Console.Beep(450, 500);
        }

        // Read all alarm lines; returns empty array if file does not exist
        public string[] ReadAll()
        {
            if (!File.Exists(FilePath))
                return Array.Empty<string>();

            return File.ReadAllLines(FilePath, Encoding.UTF8);
        }

        // Return true if there are no alarms (no non-whitespace lines) in the file.
        public bool AnyAlarms()
        {
            if (!File.Exists(FilePath))
                return true;

            // Treat lines that are only whitespace as empty
            foreach (var line in File.ReadLines(FilePath, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    return false;
            }

            return true;
        }

        // Print current alarms to the console. Prints a message if none exist.
        public void PrintAllAlarms()
        {
            var lines = ReadAll();
            if (lines.Length == 0)
            {
                Console.WriteLine("(no alarms)");
                return;
            }

            foreach (var line in lines)
                Console.WriteLine(line);
        }

        // Remove any lines that contain the match string (case-sensitive) and overwrite the file.
        // If match is null or empty, no changes are made.
        public void Clear(string match)
        {
            if (string.IsNullOrEmpty(match))
                return;

            if (!File.Exists(FilePath))
                return;

            var lines = File.ReadAllLines(FilePath, Encoding.UTF8);
            var filtered = lines.Where(line => !line.Contains(match)).ToArray();

            // If nothing changed, avoid rewriting
            if (filtered.Length == lines.Length)
                return;

            EnsureDirectory();
            File.WriteAllLines(FilePath, filtered, Encoding.UTF8);
        }
        public void ClearAlarms()
        {
            EnsureDirectory();
            File.WriteAllText(FilePath, string.Empty, Encoding.UTF8);
        }



        // makes sure the file exists and creates it if it doesn't
        private void EnsureDirectory()
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (string.IsNullOrEmpty(dir)) return;
            Directory.CreateDirectory(dir);
        }
    }

    // Convenience static helper so callers don't need to new up Alarms.
    // Call with Alarm.Raise("text") or import with `using static AlarmService.Alarm;`
    public static class Alarm
    {
        private static readonly Alarms _default = new Alarms();
        public static void Raise(string message) => _default.Raise(message);
        public static void Clear(string match) => _default.Clear(match);
        public static string[] ReadAll() => _default.ReadAll();
        public static void PrintAllAlarms() => _default.PrintAllAlarms();
        public static bool AnyAlarms() => _default.AnyAlarms();
        public static void ClearAlarms() => _default.ClearAlarms();
    }
}
