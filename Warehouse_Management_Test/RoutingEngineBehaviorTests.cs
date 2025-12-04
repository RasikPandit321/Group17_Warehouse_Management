using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warehouse;

namespace Warehouse.Tests
{
    [TestClass]
    public class RoutingEngineBehaviorTests
    {
        // Use the interface type but instantiate the concrete class
        private readonly IRoutingEngine engine = new RoutingEngine();

        [TestMethod] public void WeightUnder5_GoesToLane1() => Assert.AreEqual("Lane1", engine.Route("A", 4.9).TargetLane);
        [TestMethod] public void WeightExactly5_GoesToLane2() => Assert.AreEqual("Lane2", engine.Route("A", 5.0).TargetLane);
        [TestMethod] public void WeightUnder10_GoesToLane2() => Assert.AreEqual("Lane2", engine.Route("A", 9.9).TargetLane);
        [TestMethod] public void WeightExactly10_GoesToLane3() => Assert.AreEqual("Lane3", engine.Route("A", 10.0).TargetLane);
        [TestMethod] public void WeightOver10_GoesToLane3() => Assert.AreEqual("Lane3", engine.Route("A", 15.0).TargetLane);
        [TestMethod] public void WeightOverOverweightLimit_IsBlocked() => Assert.AreEqual("BLOCKED", engine.Route("A", 60.0).TargetLane);
    }
}
