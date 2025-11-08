using AlarmService;
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

        // --- Raise tests (2) ---
        [TestMethod]
        public void Raise_AppendsMultipleLines_InOrder()
        {
            // Arrange
            var a = "First";
            var b = "Second";

            // Act
            Raise(a);
            Raise(b);

            // Assert
            var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
            // Assert.AreEqual(2, lines.Length, "Two lines should be present after two Raise calls");
            StringAssert.Contains(lines[0], a);
            StringAssert.Contains(lines[1], b);
        }

        [TestMethod]
        public void Raise_SingleLine()
        {
            // Arrange
            var a = "Test";

            // Act
            Raise(a);

            // Assert
            var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
            // Assert.AreEqual(2, lines.Length, "Two lines should be present after two Raise calls");
            StringAssert.Contains(lines[0], a);
        }

        // --- Clear tests (3) ---

        [TestMethod]
        public void Clear_RemovesMatchingLines_CaseSensitive()
        {
            // Arrange
            Raise("KeepThis");
            Raise("RemoveThis");
            Raise("KeepThisToo");

            // Act
            Clear("RemoveThis");

            // Assert
            var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
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

            // Assert
            var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
            Assert.AreEqual(1, lines.Length);
            StringAssert.Contains(lines[0], "OnlyOne");
        }

        [TestMethod]
        public void Clear_NoMatch_LeavesFileUnchanged()
        {
            // Arrange
            Raise("A");
            Raise("B");
            var before = File.ReadAllLines(_filePath, Encoding.UTF8);

            // Act
            Clear("NoSuchText");

            // Assert
            var after = File.ReadAllLines(_filePath, Encoding.UTF8);
            CollectionAssert.AreEqual(before, after);
        }

        // --- NoAlarms tests (3) ---

        [TestMethod]
        public void NoAlarms_TrueWhenFileMissingOrWhitespaceOnly()
        {
            // File missing due to TestInitialize
            Assert.IsTrue(NoAlarms(), "NoAlarms should be true when no file exists");

            // Create a file with only whitespace lines
            File.WriteAllLines(_filePath, new[] { "   ", "\t" }, Encoding.UTF8);
            Assert.IsTrue(NoAlarms(), "NoAlarms should be true when file contains only whitespace lines");
        }

        [TestMethod]
        public void NoAlarms_FalseAfterRaise()
        {
            // Act
            Raise("AlarmNow");

            // Assert
            Assert.IsFalse(NoAlarms());
        }

        [TestMethod]
        public void NoAlarms_TrueAfterClearingAllMatchingLines()
        {
            // Arrange
            Raise("RemoveAll1");
            Raise("RemoveAll2");

            // Act
            Clear("RemoveAll");

            // Assert
            Assert.IsTrue(NoAlarms());
        }
    }
}
