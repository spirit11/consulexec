using System.Linq;
using System.Reactive.Subjects;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using NUnit.Framework;

namespace ConsulExec.Tests.ViewModel
{
    [TestFixture]
    public class StartupOptionsEditorViewModelTests : AssertionHelper
    {
        [Test]
        public void NodesInfoHandledProperly()
        {
            var nodes = new Subject<string[]>();
            var target =
                new ProfileEditorViewModel(new ProfileViewModel(new SequentialStartupOptions(new string[0]) { Name = "opt" }) , null, NodesSource: nodes);

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
    }


    [TestFixture]
    public class StartupOptionsEditorViewModelCommandsTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
            profileViewModel = new ProfileViewModel(new SequentialStartupOptions(nodeNames) {Name = OldName} );
            nodesSource =
                new BehaviorSubject<string[]>(nodeNames.Where(nn => !absentNodeNames.Contains(nn)).ToArray());
            target = new ProfileEditorViewModel(profileViewModel,
                null,
                NodesSource: nodesSource);

            foreach (var node in target.Nodes)
                node.IsChecked = selectedNodeNames.Contains(node.Name);
            target.Name = NewName;
        }

        [Test]
        public void AbsentNodesMarkedAndUpdated()
        {
            Expect(target.Nodes.Where(n => absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.True);
            Expect(target.Nodes.Where(n => !absentNodeNames.Contains(n.Name)).Select(n => n.IsAbsent), All.False);
            nodesSource.OnNext(new string[0]);
            Expect(target.Nodes.Select(n => n.IsAbsent), All.True);
            nodesSource.OnNext(nodeNames);
            Expect(target.Nodes.Select(n => n.IsAbsent), All.False);
        }

        [Test]
        public void CancelDiscardChanges()
        {
            target.CancelCommand.Execute(null);

            Expect(target.Name, EqualTo(OldName));
            Expect(profileViewModel.Options.Nodes, EquivalentTo(nodeNames));
        }

        [Test]
        public void OkApplyChanges()
        {
            target.OkCommand.Execute(null);

            Expect(target.Name, EqualTo(NewName));
            Expect(profileViewModel.Options.Nodes, EquivalentTo(selectedNodeNames));
        }

        private const string OldName = "old";
        private const string NewName = "new";

        private readonly string[] nodeNames = { "1", "2", "3", "4" };
        private readonly string[] selectedNodeNames = { "1", "3" };
        private readonly string[] absentNodeNames = { "1", "2" };

        private ProfileViewModel profileViewModel;
        private ProfileEditorViewModel target;
        private BehaviorSubject<string[]> nodesSource;
    }
}