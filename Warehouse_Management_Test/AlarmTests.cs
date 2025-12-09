using AlarmService;
using LogService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static AlarmService.Alarm;

namespace Warehouse_Management_Test
{
    [TestClass]
    [DoNotParallelize]
    public class AlarmTests
    {
        private readonly string _filePath = Path.GetFullPath("alarm.txt");

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        // --- Raise tests ---

        [TestMethod]
        public void Raise_NullMessage_WritesEmptyString()
        {
            // Act
            Alarm.Raise(null);
            var lines = Alarm.ReadAll();

            // Assert
            Assert.AreEqual(1, lines.Length);
            StringAssert.EndsWith(lines[0], "");
        }

        [TestMethod]
        public void Raise_OneLine()
        {
            // Arrange
            var message = "TestAlarm1";

            // Act
            Raise(message);

            var lines = Alarm.ReadAll();

            Assert.AreEqual(1, lines.Length);
            StringAssert.Contains(lines[0], message);
        }

        [TestMethod]
        public void Raise_AppendsMultipleLines_InOrder()
        {
            // Arrange
            var a = "First";
            var b = "Second";

            // Act
            Raise(a);
            Raise(b);

            var lines = Alarm.ReadAll();

            Assert.AreEqual(2, lines.Length);
            StringAssert.Contains(lines[0], a);
            StringAssert.Contains(lines[1], b);
        }

        // --- Clear tests ---

        [TestMethod]
        public void Clear_RemovesMatchingLines_CaseSensitive()
        {
            // Arrange
            Raise("KeepThis");
            Raise("RemoveThis");
            Raise("KeepThisToo");

            // Act
            Clear("RemoveThis");

            var lines = Alarm.ReadAll();

            Assert.IsFalse(lines.Any(l => l.Contains("RemoveThis")));
            Assert.AreEqual(2, lines.Length);
        }

        [TestMethod]
        public void Clear_NullOrEmpty_DoesNothing()
        {
            // Arrange
            Raise("OnlyOne");

            // Act
            Clear(null);
            Clear(string.Empty);

            var lines = Alarm.ReadAll();

            Assert.AreEqual(1, lines.Length);
            StringAssert.Contains(lines[0], "OnlyOne");
        }

        [TestMethod]
        public void Clear_NoMatch_LeavesFileUnchanged()
        {
            // Arrange
            Raise("A");
            Raise("B");

            var before = Alarm.ReadAll();

            // Act
            Clear("NoSuchText");

            var after = Alarm.ReadAll();

            CollectionAssert.AreEqual(before, after);
        }

        // --- ClearAlarms tests ---

        [TestMethod]
        public void ClearAll_ClearsAllAlarms_ReturnsEmptyFile()
        {
            // Arrange
            Raise("ToBeErased");

            // Act
            ClearAlarms();
            var result = ReadAll();

            // Assert
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void ClearAll_EmptyFile_ReturnsEmptyFile()
        {
            // Act
            ClearAlarms();
            var result = ReadAll();

            Assert.AreEqual(0, result.Length);
        }

        // --- AnyAlarms tests ---

        [TestMethod]
        public void AnyAlarms_TrueWhenFileMissingOrWhitespaceOnly()
        {
            // File missing due to TestInitialize
            Assert.IsTrue(AnyAlarms());

            File.WriteAllLines(_filePath, new[] { "   " }, Encoding.UTF8);

            Assert.IsTrue(AnyAlarms());
        }

        [TestMethod]
        public void AnyAlarms_FalseAfterRaise()
        {
            // Act
            Raise("AlarmNow");

            // Assert
            Assert.IsFalse(AnyAlarms());
        }

        [TestMethod]
        public void AnyAlarms_TrueAfterClearingAllMatchingLines()
        {
            // Arrange
            Raise("RemoveAll1");
            Raise("RemoveAll2");

            // Act
            Clear("RemoveAll");

            // Assert
            Assert.IsTrue(AnyAlarms());
        }
    }
}
