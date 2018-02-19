using System.IO;
using System.Linq;
using ConsulExec.Domain;
using ConsulExec.Tests.Util;
using NUnit.Framework;

namespace ConsulExec.Tests.Infrastructure
{
    [TestFixture]
    public class ConfigurationTests : AssertionHelper
    {
        [Test]
        public void ConfigurationIsPersistent()
        {
            var co = new ConnectionOptions { Name = "n", ServerAddress = "s" };
            var co2 = new ConnectionOptions { Name = "n2", ServerAddress = "s2" };
            var connections = new[] { co, co2 };
            var savedTarget = new Configuration
            {
                Connections = connections.ToList(),
                MruCommands = new[] { "cmd1", "cmd2" }.ToList(),
                Starup = new StartupOptions[]
                {
                    new SequentialStartupOptions(new[] { "a", "b" }) { Connection = connections.First() },
                    new SequentialStartupOptions(new[] { "c", "d" }) { Connection = connections.First() },
                    new SequentialStartupOptions(new[] { "e" }) { Connection = connections.Last() },
                }.ToList()
            };

            var loadedTarget = SaveAndLoadTarget(savedTarget);

            Expect(loadedTarget.MruCommands, EqualTo(savedTarget.MruCommands),
                "Recent commands are not saved or loaded");
            Expect(loadedTarget.Connections, CollectionProperties.AreEqual(savedTarget.Connections, v => v.Name),
                "Connection names are not saved or loaded");
            Expect(loadedTarget.Connections, CollectionProperties.AreEqual(savedTarget.Connections, v => v.ServerAddress),
                "Connection addreses are not saved or loaded");

            Expect(loadedTarget.Starup.Select(v => v.Connection).Distinct(), SubsetOf(loadedTarget.Connections),
                "Connections of startup properties are not from Connections collection");

            Expect(loadedTarget.Starup, CollectionProperties.AreEqual(savedTarget.Starup, v => v.Connection.Name),
                "Starup connections are not properly mapped");

            Expect(loadedTarget.Starup, CollectionProperties.AreEqual(savedTarget.Starup, v => v.Name),
                "Starup names are not saved or loaded");
            Expect(loadedTarget.Starup, CollectionProperties.AreEqual(savedTarget.Starup, v => string.Join(", ", v.Nodes)),
                "Starup nodes are not saved or loaded");
        }

        private static Configuration SaveAndLoadTarget(Configuration SavedTarget)
        {
            using (var sw = new StringWriter())
            {
                SavedTarget.SaveTo(sw);
                using (var rd = new StringReader(sw.ToString()))
                    return Configuration.ReadFrom(rd);
            }
        }
    }
}