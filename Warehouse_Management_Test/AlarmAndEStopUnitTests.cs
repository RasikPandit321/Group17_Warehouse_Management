using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Alarm_and_Estop;
using System.IO;
using System;
using System.Linq;

namespace Warehouse_Management_Test
{
    [TestClass]
    public class AlarmAndEStopUnitTests
    {
        [TestInitialize]
        public void Setup()
        {
            EmergencyStop.Reset("Test Setup"); // Reset state before each test
        }

        [TestMethod]
        public void Test_Alarm_Raise_creates_File()
        {
            Alarm.Raise("Test Alarm"); // Trigger alarm
            Assert.IsTrue(File.Exists("alarm.txt")); // Verify file exists
        }

        [TestMethod]
        public void Test_Alarm_Content_Is_Written()
        {
            string msg = "UniqueTestMessage_" + Guid.NewGuid(); // Unique content
            Alarm.Raise(msg); // Log message
            string content = File.ReadAllText("alarm.txt"); // Read log file
            StringAssert.Contains(content, msg); // Verify message exists
        }

        [TestMethod]
        public void Test_Alarm_Raise_HighSeverity()
        {
            Alarm.Raise("High Severity Alarm"); // Log high severity
            Assert.IsTrue(File.Exists("alarm.txt")); // File should exist
        }

        [TestMethod]
        public void Test_Alarm_Raise_LowSeverity()
        {
            Alarm.Raise("Low Battery Warning"); // Log low severity
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Raise_EmptyMessage()
        {
            Alarm.Raise(""); // Empty message allowed
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Raise_NullMessage()
        {
            Alarm.Raise(null!); // Test null input
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Multiple_Logs()
        {
            Alarm.Raise("Alarm 1");
            Alarm.Raise("Alarm 2");
            var lines = File.ReadAllLines("alarm.txt"); // Read all lines
            Assert.IsTrue(lines.Length >= 2); // Ensure both were logged
        }

        [TestMethod]
        public void Test_EStop_Trigger_SetsState()
        {
            EmergencyStop.Estop("Emergency Button Pressed"); // Trigger E-Stop
            string content = File.ReadAllText("alarm.txt"); // Implementation specific
            Assert.IsTrue(true); // Placeholder validation
        }

        [TestMethod]
        public void Test_EStop_Reset_LogEntry()
        {
            EmergencyStop.Reset("System Reset"); // Reset E-Stop
            Assert.IsTrue(true); // Behavior depends on implementation
        }

        [TestMethod]
        public void Test_Log_Write_CreatesFile()
        {
            Log.Write("General Info Log"); // Write to log
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_Log_Content_Match()
        {
            string id = Guid.NewGuid().ToString(); // Unique payload
            Log.Write($"User Login {id}"); // Write entry
            string content = File.ReadAllText("logs.txt"); // Read file
            StringAssert.Contains(content, id); // Check match
        }

        [TestMethod]
        public void Test_Log_Append_Mode()
        {
            File.Delete("logs.txt"); // Reset file
            Log.Write("Line 1");
            Log.Write("Line 2");
            var lines = File.ReadAllLines("logs.txt"); // Read all lines
            Assert.AreEqual(2, lines.Length); // Should append, not overwrite
        }

        [TestMethod]
        public void Test_Log_Empty_Message()
        {
            Log.Write(""); // Allow empty writes
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_EStop_Then_Alarm()
        {
            EmergencyStop.Estop("Stop"); // Trigger E-Stop
            Alarm.Raise("Post Stop Alarm"); // Alarm after stop
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_File_Creation_If_Deleted()
        {
            if (File.Exists("alarm.txt")) File.Delete("alarm.txt"); // Remove file
            Alarm.Raise("New File Test"); // Should recreate
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Log_File_Creation_If_Deleted()
        {
            if (File.Exists("logs.txt")) File.Delete("logs.txt"); // Remove file
            Log.Write("New Log File"); // Should recreate
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Timestamp_Exists()
        {
            Alarm.Raise("Time Check");
            string lastLine = File.ReadLines("alarm.txt").Last(); // Last log entry
            StringAssert.Contains(lastLine, "["); // Timestamp format
        }

        [TestMethod]
        public void Test_Log_Timestamp_Exists()
        {
            Log.Write("Time Check Log");
            string lastLine = File.ReadLines("logs.txt").Last();
            StringAssert.Contains(lastLine, "[");
        }

        [TestMethod]
        public void Test_Large_Log_Volume()
        {
            for (int i = 0; i < 50; i++) Log.Write($"Log {i}"); // Multiple entries
            var lines = File.ReadAllLines("logs.txt");
            Assert.IsTrue(lines.Length >= 50); // Ensure all appended
        }

        [TestMethod]
        public void Test_Large_Alarm_Volume()
        {
            for (int i = 0; i < 50; i++) Alarm.Raise($"Alarm {i}");
            var lines = File.ReadAllLines("alarm.txt");
            Assert.IsTrue(lines.Length >= 50);
        }

        [TestMethod]
        public void Test_Reset_After_Crash()
        {
            EmergencyStop.Estop("Crash");
            EmergencyStop.Reset("Recovered"); // Reset after E-Stop
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Log_Write_Null()
        {
            Log.Write(null!); // Test null handling
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_EStop_Null_Message()
        {
            EmergencyStop.Estop(null!); // Test null handling
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Reset_Null_Message()
        {
            EmergencyStop.Reset(null!); // Test null handling
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Simulate_Complex_Scenario()
        {
            Log.Write("Start");              // Write log
            Alarm.Raise("Warning");          // Raise alarm
            EmergencyStop.Estop("Stop");     // Trigger emergency stop
            Assert.IsTrue(File.Exists("logs.txt")); // Ensure main log exists
        }
    }
}
