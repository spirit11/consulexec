using ConsulExec.Domain;
using ConsulExec.ViewModel;
using NUnit.Framework;
using StructureMap;

namespace ConsulExec.Tests.EndToEnd
{
    [TestFixture]
    public class IntegrationTests : AssertionHelper
    {
        [TestCase(3, 2)]
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        public void ViewModelsCollectionsAreInSyncWithModelConfiguration(int ConnectionsCount, int ProfilesCount)
        {
            var container = new Container(new RuntimeRegistry());
            var connections = container.GetInstance<ConnectionProfilesViewModel>();
            var startups = container.GetInstance<StartupOptionsProfilesViewModel>();

            connections.List.Clear();
            for (int i = 0; i < ConnectionsCount; i++)
                connections.List.Add(ProfileViewModelsFactory.Create(new ConnectionOptions()));

            startups.List.Clear();
            for (int i = 0; i < ProfilesCount; i++)
                startups.List.Add(ProfileViewModelsFactory.Create(new SequentialStartupOptions(new string[0])));

            Expect(container.GetInstance<Configuration>().Connections.Count, EqualTo(ConnectionsCount));
            Expect(container.GetInstance<Configuration>().Startups.Count, EqualTo(ProfilesCount));
        }
    }
}