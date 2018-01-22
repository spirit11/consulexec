using NUnit.Framework;
using System.Reactive.Linq;
using ConsulExec.Domain;

namespace ConsulExec.Tests
{
    [TestFixture]
    public class ConsulServicesTests : AssertionHelper
    {
        [Test]
        [Explicit("Integration test with running consul agent")]
        public void CheckExecutionReturnsOutputStream()
        {
            var target = new RemoteExecution();
            var l = target.Execute(Observable.Return(new NodeExecutionTask("echo ok")))
                .SelectMany(v => v.Output)
                .ToList().Wait(); //TODO timeout
            Assert.That(l, Has.Some.Contains("ok"));
        }
    }
}
