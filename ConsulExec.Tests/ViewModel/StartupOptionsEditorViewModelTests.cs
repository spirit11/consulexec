using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using Moq;
using NUnit.Framework;
using ReactiveUI;

namespace ConsulExec.Tests.ViewModel
{
    [TestFixture]
    public class StartupOptionsEditorViewModelTests : AssertionHelper
    {
        [Test]
        public void NodesInfoHandledProperly()
        {
            var nodes = new Subject<string[]>();
            var target = CreateTarget(nodes, new string[0]);

            Expect(target.Nodes, Is.Empty);

            nodes.OnNext(new[] { "a", "b" });
            var v1 = target.Nodes.ToArray();

            nodes.OnNext(new[] { "a", "b", "c" });
            var v2 = target.Nodes.ToArray();
            Expect(v1, Is.EqualTo(v2.Take(2)));

            nodes.OnNext(new[] { "c", "a" });

            Expect(target.Nodes.Select(n => n.Name), EqualTo(new[] { "a", "b", "c" }));
            Expect(target.Nodes.First(n => n.Name == "b").IsAbsent, Is.True);
        }

        public static StartupOptionsEditorViewModel CreateTarget(IObservable<string[]> Nodes, string[] InitialNodes)
        {
            var remoteExec = Mock.Of<IRemoteExecution>(ir => ir.Nodes == Nodes);
            var co = Mock.Of<ConnectionOptions>(v => v.Create() == remoteExec);

            var connections = Mock.Of<IProfilesViewModel<ProfileViewModel<ConnectionOptions>>>(
                m => m.List == new ReactiveList<ProfileViewModel<ConnectionOptions>>(new[] { new ProfileViewModel<ConnectionOptions>(co, v => v.Name) }));
            var target = new StartupOptionsEditorViewModel(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(InitialNodes) { Connection = co, Name = "opt" }),
                connections,
                null);

            return target;
        }
    }


    [TestFixture]
    public class StartupOptionsEditorViewModelCommandsTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
            nodes = new BehaviorSubject<string[]>(nodeNames.Where(nn => !absentNodeNames.Contains(nn)).ToArray());

            var remoteExec = Mock.Of<IRemoteExecution>(ir => ir.Nodes == nodes);
            var co = Mock.Of<ConnectionOptions>(v => v.Create() == remoteExec);
            startupOptionsProfileViewModel = ProfilesViewModelsFactory.Create(new SequentialStartupOptions(nodeNames) { Connection = co, Name = OldName });
            var connections = Mock.Of<IProfilesViewModel<ProfileViewModel<ConnectionOptions>>>(
                m => m.List == new ReactiveList<ProfileViewModel<ConnectionOptions>>(new[] { new ProfileViewModel<ConnectionOptions>(co, f => f.Name) }));
            target =
                new StartupOptionsEditorViewModel(
                startupOptionsProfileViewModel,
                connections,
                null);

            foreach (var node in target.Nodes)
                node.IsChecked = selectedNodeNames.Contains(node.Name);
            target.Name = NewName;
        }

        [Test]
        public void AbsentNodesMarkedAndUpdated()
        {
            Expect(target.Nodes.Where(n => absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.True);
            Expect(target.Nodes.Where(n => !absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.False);
            nodes.OnNext(new string[0]);
            Expect(target.Nodes.Select(n => n.IsAbsent), All.True);
            nodes.OnNext(nodeNames);
            Expect(target.Nodes.Select(n => n.IsAbsent), All.False);
        }

        [Test]
        public void OkApplyChanges()
        {
            target.OkCommand.Execute(null);

            Expect(target.Name, EqualTo(NewName));
            Expect(startupOptionsProfileViewModel.Options.Nodes, EquivalentTo(selectedNodeNames));
        }

        private const string OldName = "old";
        private const string NewName = "new";

        private readonly string[] nodeNames = { "1", "2", "3", "4" };
        private readonly string[] selectedNodeNames = { "1", "3" };
        private readonly string[] absentNodeNames = { "1", "2" };

        private StartupOptionsEditorViewModel target;
        private BehaviorSubject<string[]> nodes;
        private ProfileViewModel<StartupOptions> startupOptionsProfileViewModel;
    }
}