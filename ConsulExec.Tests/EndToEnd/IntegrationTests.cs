using ConsulExec.Domain;
using ConsulExec.ViewModel;
using NUnit.Framework;
using StructureMap;

namespace ConsulExec.Tests.EndToEnd
{
    [TestFixture]
    public class IntegrationTests : AssertionHelper
    {
        [Test]
        public void CheckConfiguration()
        {
            var container = new Container(new RuntimeRegistry());
            var vm = container.GetInstance<CommandStartupViewModel>();
            vm.ConnectionProfiles.List.Clear();
            vm.StartupOptionsProfiles.List.Clear();

            vm.ConnectionProfiles.List.Add(
                new ProfileViewModel<ConnectionOptions>(new ConnectionOptions(), v => v.Name));

            vm.StartupOptionsProfiles.List.Add(
                new ProfileViewModel<StartupOptions>(new SequentialStartupOptions(new string[0]), v => v.Name));

            Expect(container.GetInstance<Configuration>().Connections.Count, EqualTo(1));
            Expect(container.GetInstance<Configuration>().Starup.Count, EqualTo(1));
        }
    }
}