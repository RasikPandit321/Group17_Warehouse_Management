namespace Warehouse_Management_Test;
using AlarmService;
using LogService;
using System;
using System.Runtime.InteropServices;
using System.Text;
using static AlarmService.Alarm;

[TestClass]
[DoNotParallelize]
public class EstopTests
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

    // --- Estop Alarm test (1) ---
    [TestMethod]
    public void Estop_ValidMessage_RaisesAlarm()
    {
        // Arrange
        var reason = "Fault detected";

        // Act
        EmergencyStop.Estop(reason);

        // Assert alarm raised
        var result = ReadAll();

        Assert.AreEqual(1, result.Length);
        StringAssert.Contains(result[0], reason);
    }
    // --- Estop Log test (1) ---
    [TestMethod]
    public void Estop_ValidMessage_ArchivesLog()
    {
        // Arrange
        var reason = "Fault detected";

        // Act
        EmergencyStop.Estop(reason);

        // Assert alarm raised
        var result = Log.ReadAll();

        StringAssert.Contains(result[0], reason);
    }
}
