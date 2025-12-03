using Microsoft.VisualStudio.TestTools.UnitTesting;
using WareHouse_Management.Conveyor_and_Motor;

namespace Warehouse_Management_Test;

[TestClass]
public class MotorControllerTests
{
    //  Motor should start successfully when safe
    [TestMethod]
    public void Start_WhenSafe_ShouldReturnTrue_AndRunMotor()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety { EStop = false, Fault = false };
        var motor = new MotorController(driver, safety);

        var ok = motor.Start();

        Assert.IsTrue(ok, "Motor should start when no EStop or Fault is active.");
        Assert.IsTrue(driver.IsRunning, "Driver should show motor running.");
    }

    // Motor should not start when EStop is active
    [TestMethod]
    public void Start_WhenEStopActive_ShouldReturnFalse_AndNotRun()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety { EStop = true, Fault = false };
        var motor = new MotorController(driver, safety);

        var ok = motor.Start();

        Assert.IsFalse(ok, "Motor should not start when EStop is pressed.");
        Assert.IsFalse(driver.IsRunning);
    }

    // Motor should not start when Fault is active
    [TestMethod]
    public void Start_WhenFaultActive_ShouldReturnFalse_AndNotRun()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety { EStop = false, Fault = true };
        var motor = new MotorController(driver, safety);

        var ok = motor.Start();

        Assert.IsFalse(ok, "Motor should not start when a fault is active.");
        Assert.IsFalse(driver.IsRunning);
    }

    // Motor should stop after starting
    [TestMethod]
    public void Stop_AfterStart_ShouldLeaveMotorNotRunning()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety();
        var motor = new MotorController(driver, safety);

        motor.Start();
        motor.Stop();

        Assert.IsFalse(driver.IsRunning, "Motor should stop after Stop() is called.");
    }

    //  Stop should not throw when already stopped
    [TestMethod]
    public void Stop_WhenAlreadyStopped_ShouldNotThrow()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety();
        var motor = new MotorController(driver, safety);

        motor.Stop();

        Assert.IsFalse(driver.IsRunning);
    }

    //  Start should only trigger once when already running
    [TestMethod]
    public void Start_Twice_ShouldCallStartForwardOnlyOnce()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety();
        var motor = new MotorController(driver, safety);

        motor.Start();
        motor.Start();

        Assert.AreEqual(1, driver.StartForwardCalls, "StartForward should be called only once.");
    }

    //  Motor can start again after being stopped
    [TestMethod]
    public void Start_AfterStop_ShouldStartAgain()
    {
        var driver = new FakeMotorDriver();
        var safety = new FakeSafety();
        var motor = new MotorController(driver, safety);

        motor.Start();
        motor.Stop();
        motor.Start();

        Assert.AreEqual(2, driver.StartForwardCalls, "Motor should be able to start again after stopping.");
    }
}

// -----------------------------------------------------------------------------
// Fake classes for testing only (simulate motor hardware and safety signals)
// -----------------------------------------------------------------------------
public sealed class FakeMotorDriver : IMotorDriver
{
    public bool IsRunning { get; private set; }
    public int StartForwardCalls { get; private set; }

    public void StartForward()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            StartForwardCalls++;
        }
    }

    public void Stop() => IsRunning = false;
}

public sealed class FakeSafety : ISafetyInputs
{
    public bool EStop { get; set; }
    public bool Fault { get; set; }
}

// Jam sensor fake for conveyor tests
public sealed class FakeJamSensor : IJamSensor
{
    public bool JamDetected { get; set; }
}
