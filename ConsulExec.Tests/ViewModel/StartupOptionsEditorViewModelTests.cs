using System;
using System.Linq;
using System.Reactive.Linq;
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

            var connections = new ConnectionProfilesViewModel((o, e) => { },
                    new UndoListViewModel(),
                    new ReactiveList<ProfileViewModel<ConnectionOptions>>(new[] { ProfilesViewModelsFactory.Create(co) }));

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
        public void SwitchSubscriptionWhenOtherConnectionSelected()
        {
            CreateTarget(new BehaviorSubject<string[]>(new[] { "a", "b" }), new string[0]);

            var nodes1 = new NodesRequestState(new[] { "c", "d", "e" });
            var nodes2 = new NodesRequestState(new[] { "c2", "d2", "e2" });

            nodes1.SetProfile(Target.Connections);
            nodes2.SetProfile(Target.Connections);
            nodes1.NextNodes(new[] { "c" });

            Expect(nodes1.RequestsCount, EqualTo(1));
            Expect(nodes2.RequestsCount, EqualTo(1));
            Expect(Target.Nodes, Count.EqualTo(8));

            Mock.Get(nodes1.Options).VerifyAll();
            Mock.Get(nodes2.Options).VerifyAll();
        }


        private class NodesRequestState
        {
            public NodesRequestState(string[] NodeNames)
            {
                nodes = new BehaviorSubject<string[]>(NodeNames);
                var observable = nodes.Do(_ => { RequestsCount++; });
                var fakeConnection = Mock.Of<IRemoteExecution>(o => o.Nodes == observable);
                Options = Mock.Of<ConnectionOptions>(o => o.Create() == fakeConnection);
            }

            public int RequestsCount { get; private set; }

            public ConnectionOptions Options { get; }

            public void SetProfile(IProfilesViewModel<ProfileViewModel<ConnectionOptions>> Profiles)
            {
                var connectionProfile = new ProfileViewModel<ConnectionOptions>(Options, f => "Fake");
                Profiles.List.Add(connectionProfile);
                Profiles.Profile = connectionProfile;
            }

            public void NextNodes(string[] Strings) => nodes.OnNext(Strings);

            private readonly BehaviorSubject<string[]> nodes;
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

        [Test]
        public void OkInvokedOptionsUpdated()
        {
            var nodes = new Subject<string[]>();
            CreateTarget(nodes, new string[0]);

            var oldOptions = StartupOptionsProfileViewModel.Options;
            Target.Name = "newname";
            Target.OkCommand.Execute(null);
            Expect(oldOptions, Not.SameAs(StartupOptionsProfileViewModel.Options));
        }

        private const string NewName = "new";

        private readonly string[] nodeNames = { "1", "2", "3", "4" };
        private readonly string[] selectedNodeNames = { "1", "3" };
        private readonly string[] absentNodeNames = { "1", "2" };

        private BehaviorSubject<string[]> nodes;
    }
}