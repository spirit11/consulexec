using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using Moq;
using NUnit.Framework;
using ReactiveUI;

namespace ConsulExec.Tests.ViewModel
{
    public abstract class StartupOptionsEditorViewModelTestsBase : AssertionHelper
    {
        protected void CreateTarget(IObservable<string[]> Nodes, string[] InitialNodes)
        {
            var remoteExec = Mock.Of<IRemoteExecution>(ir => ir.Nodes == Nodes);
            var co = Mock.Of<ConnectionOptions>(v => v.Create() == remoteExec);

            var connections = Mock.Of<IProfilesViewModel<ProfileViewModel<ConnectionOptions>>>(
                m => m.List == new ReactiveList<ProfileViewModel<ConnectionOptions>>(new[] { ProfilesViewModelsFactory.Create(co) }));

            StartupOptionsProfileViewModel = ProfilesViewModelsFactory.Create(new SequentialStartupOptions(InitialNodes) { Connection = co, Name = OldName });
            Target = new StartupOptionsEditorViewModel(
                StartupOptionsProfileViewModel,
                connections,
                null);
        }

        protected StartupOptionsEditorViewModel Target { get; private set; }
        protected ProfileViewModel<StartupOptions> StartupOptionsProfileViewModel { get; private set; }

        private const string OldName = "old";
    }


    [TestFixture]
    public class StartupOptionsEditorViewModelTests : StartupOptionsEditorViewModelTestsBase
    {
        [Test]
        public void NodesInfoHandledProperly()
        {
            var nodes = new Subject<string[]>();
            CreateTarget(nodes, new string[0]);

            Expect(Target.Nodes, Is.Empty);

            nodes.OnNext(new[] { "a", "b" });
            var v1 = Target.Nodes.ToArray();

            nodes.OnNext(new[] { "a", "b", "c" });
            var v2 = Target.Nodes.ToArray();
            Expect(v1, Is.EqualTo(v2.Take(2)));

            nodes.OnNext(new[] { "c", "a" });

            Expect(Target.Nodes.Select(n => n.Name), EqualTo(new[] { "a", "b", "c" }));
            Expect(Target.Nodes.First(n => n.Name == "b").IsAbsent, Is.True);
        }

        [Test]
        public void ReconnectingWhenNewServerSetUp()
        {
            var nodes = new BehaviorSubject<string[]>(new[] { "a", "b" });
            CreateTarget(nodes, new string[0]);

            var fakeConnection = Mock.Of<IRemoteExecution>(
                o => o.Nodes == new BehaviorSubject<string[]>(new[] { "c", "d", "e" }));

            var options = Mock.Of<ConnectionOptions>(
                o => o.Create() == fakeConnection);

            var connectionProfile = new ProfileViewModel<ConnectionOptions>(options, f => "Fake");
            Target.Connections.List.Add(connectionProfile);
            Target.Connections.Profile = connectionProfile;

            Expect(Target.Nodes.Count, Is.EqualTo(3));

            Mock.Get(options).VerifyAll();
        }
    }


    [TestFixture]
    public class StartupOptionsEditorViewModelCommandsTests : StartupOptionsEditorViewModelTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            nodes = new BehaviorSubject<string[]>(nodeNames.Where(nn => !absentNodeNames.Contains(nn)).ToArray());

            CreateTarget(nodes, nodeNames);

            foreach (var node in Target.Nodes)
                node.IsChecked = selectedNodeNames.Contains(node.Name);
            Target.Name = NewName;
        }

        [Test]
        public void AbsentNodesMarkedAndUpdated()
        {
            Expect(Target.Nodes.Where(n => absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.True);
            Expect(Target.Nodes.Where(n => !absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.False);
            nodes.OnNext(new string[0]);
            Expect(Target.Nodes.Select(n => n.IsAbsent), All.True);
            nodes.OnNext(nodeNames);
            Expect(Target.Nodes.Select(n => n.IsAbsent), All.False);
        }

        [Test]
        public void OkApplyChanges()
        {
            Target.OkCommand.Execute(null);

            Expect(Target.Name, EqualTo(NewName));
            Expect(StartupOptionsProfileViewModel.Options.Nodes, EquivalentTo(selectedNodeNames));
        }

        private const string NewName = "new";

        private readonly string[] nodeNames = { "1", "2", "3", "4" };
        private readonly string[] selectedNodeNames = { "1", "3" };
        private readonly string[] absentNodeNames = { "1", "2" };

        private BehaviorSubject<string[]> nodes;
    }
}