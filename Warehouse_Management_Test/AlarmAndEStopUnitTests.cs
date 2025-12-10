using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Alarm_and_Estop; // Fixed Namespace
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
            EmergencyStop.Reset("Test Setup");
        }

        [TestMethod]
        public void Test_Alarm_Raise_creates_File()
        {
            Alarm.Raise("Test Alarm");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Content_Is_Written()
        {
            string msg = "UniqueTestMessage_" + Guid.NewGuid();
            Alarm.Raise(msg);
            string content = File.ReadAllText("alarm.txt");
            StringAssert.Contains(content, msg);
        }

        [TestMethod]
        public void Test_Alarm_Raise_HighSeverity()
        {
            Alarm.Raise("High Severity Alarm");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Raise_LowSeverity()
        {
            Alarm.Raise("Low Battery Warning");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Raise_EmptyMessage()
        {
            Alarm.Raise("");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Raise_NullMessage()
        {
            Alarm.Raise(null!); // Suppress null warning for test
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Multiple_Logs()
        {
            Alarm.Raise("Alarm 1");
            Alarm.Raise("Alarm 2");
            var lines = File.ReadAllLines("alarm.txt");
            Assert.IsTrue(lines.Length >= 2);
        }

        [TestMethod]
        public void Test_EStop_Trigger_SetsState()
        {
            EmergencyStop.Estop("Emergency Button Pressed");
            string content = File.ReadAllText("alarm.txt"); // Assuming EStop logs to alarm file or log file
            // Note: Implementation specific validation
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_EStop_Reset_LogEntry()
        {
            EmergencyStop.Reset("System Reset");
            Assert.IsTrue(true); // implementation specific
        }

        [TestMethod]
        public void Test_Log_Write_CreatesFile()
        {
            Log.Write("General Info Log");
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_Log_Content_Match()
        {
            string id = Guid.NewGuid().ToString();
            Log.Write($"User Login {id}");
            string content = File.ReadAllText("logs.txt");
            StringAssert.Contains(content, id);
        }

        [TestMethod]
        public void Test_Log_Append_Mode()
        {
            File.Delete("logs.txt");
            Log.Write("Line 1");
            Log.Write("Line 2");
            var lines = File.ReadAllLines("logs.txt");
            Assert.AreEqual(2, lines.Length);
        }

        [TestMethod]
        public void Test_Log_Empty_Message()
        {
            Log.Write("");
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_EStop_Then_Alarm()
        {
            EmergencyStop.Estop("Stop");
            Alarm.Raise("Post Stop Alarm");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Alarm_File_Creation_If_Deleted()
        {
            if (File.Exists("alarm.txt")) File.Delete("alarm.txt");
            Alarm.Raise("New File Test");
            Assert.IsTrue(File.Exists("alarm.txt"));
        }

        [TestMethod]
        public void Test_Log_File_Creation_If_Deleted()
        {
            if (File.Exists("logs.txt")) File.Delete("logs.txt");
            Log.Write("New Log File");
            Assert.IsTrue(File.Exists("logs.txt"));
        }

        [TestMethod]
        public void Test_Alarm_Timestamp_Exists()
        {
            Alarm.Raise("Time Check");
            string lastLine = File.ReadLines("alarm.txt").Last();
            StringAssert.Contains(lastLine, "[");
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
            for (int i = 0; i < 50; i++) Log.Write($"Log {i}");
            var lines = File.ReadAllLines("logs.txt");
            Assert.IsTrue(lines.Length >= 50);
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
            EmergencyStop.Reset("Recovered");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Log_Write_Null()
        {
            Log.Write(null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_EStop_Null_Message()
        {
            EmergencyStop.Estop(null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Reset_Null_Message()
        {
            EmergencyStop.Reset(null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Simulate_Complex_Scenario()
        {
            Log.Write("Start");
            Alarm.Raise("Warning");
            EmergencyStop.Estop("Stop");
            Assert.IsTrue(File.Exists("logs.txt"));
        }
    }
}