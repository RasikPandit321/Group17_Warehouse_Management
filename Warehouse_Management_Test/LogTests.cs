using LogService;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Text;

namespace Warehouse_Management_Test;

[TestClass]
[DoNotParallelize]
public class LogTests
{
    private readonly string _filePath = Path.GetFullPath("logs.txt");

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

    // --- Archive tests (3) ---
    [TestMethod]
    public void Archive_NullMessage_WritesEmptyString()
    {
        // Act
        Log.Archive(null);
        var lines = Log.ReadAll();

        // Assert
        Assert.AreEqual(1, lines.Length);
        StringAssert.EndsWith(lines[0], ""); // empty message
    }
    [TestMethod]
    public void Archive_LogsMultipleLines()
    {
        // Arrange
        var one = "One";
        var two = "Two";

        // Act
        Log.Archive(one);
        Log.Archive(two);

        var lines = Log.ReadAll();

        // Assert

        StringAssert.Contains(lines[0], one);
        StringAssert.Contains(lines[1], two);
    }

    [TestMethod]
    public void Archive_OneLine()
    {
        // Arrange
        var one = "OneLine";

        // Act
        Log.Archive(one);

        var lines = Log.ReadAll();
        // Assert

        StringAssert.Contains(lines[0], one);
    }
    // --- ReadAll tests (3) ---
    [TestMethod]
    public void ReadAll_AfterClearLogs_ReturnsEmpty()
    {
        // Arrange
        Log.Archive("Some log");

        // Act
        Log.ClearLogs();
        var lines = Log.ReadAll();

        // Assert
        Assert.AreEqual(0, lines.Length);
    }
    [TestMethod]
    public void ReadAll_EmptyArray_ReturnsEmptyArray()
    {
        // Arrange //
        // Not sending anything

        // Act
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(0, result.Length);
    }
    [TestMethod]
    public void ReadAll_FileWithLines_ReturnsLines()
    {
        // Arrange
        Log.Archive("Line 1");
        Log.Archive("Line 2");
        Log.Archive("Line 3");

        var expectedMessages = new[]
        {
        "Line 1",
        "Line 2",
        "Line 3"
    };

        // Act
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(expectedMessages.Length, result.Length);
    }

    // --- ClearLogs test (3) ---
    [TestMethod]
    public void ClearLogs_ThenWriteNewLog_WritesCorrectly()
    {
        // Arrange
        Log.Archive("Old message");
        Log.ClearLogs();

        // Act
        Log.Archive("New message");
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(1, result.Length);
        StringAssert.EndsWith(result[0], "New message");
    }
    [TestMethod]
    public void ClearLogs_EmptiesAllLogs_ReturnsEmptyFile()
    {
        // Arrange //
        Log.Archive("FakeLog1");
        Log.Archive("FakeLog2");

        // Act
        Log.ClearLogs();
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(0, result.Length);
    }
    [TestMethod]
    public void ClearLogs_EmptyFile_ReturnsEmptyFile()
    {
        // Arrange
        // keeping file empty

        // Act
        Log.ClearLogs();
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(0, result.Length);
    }

}
