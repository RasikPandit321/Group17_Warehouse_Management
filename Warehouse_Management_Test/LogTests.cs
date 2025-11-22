using System.Text;
using LogService;

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

    // --- Archive tests (2) ---
    [TestMethod]
    public void Archive_LogsMultipleLines()
    {
        // Arrange
        var one = "One";
        var two = "Two";

        // Act
        Log.Archive(one);
        Log.Archive(two);

        // Assert
        var lines = File.ReadAllLines(_filePath, Encoding.UTF8);

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

        // Assert
        var lines = File.ReadAllLines(_filePath, Encoding.UTF8);

        StringAssert.Contains(lines[0], one);
    }
    // --- ReadAll tests (2) ---
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
        var expected = new[] { "Line 1", "Line 2", "Line 3" };

        File.WriteAllLines(_filePath, expected, Encoding.UTF8);

        // Act
        var result = Log.ReadAll();

        // Assert
        Assert.AreEqual(expected.Length, result.Length);
        CollectionAssert.AreEqual(expected, result);
    }
}
