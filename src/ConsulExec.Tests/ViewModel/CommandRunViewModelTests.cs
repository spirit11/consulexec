using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ConsulExec.Design;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using NUnit.Framework;

namespace ConsulExec.Tests.ViewModel
{
    [TestFixture]
    public class CommandRunViewModelTests : AssertionHelper
    {
        [Test]
        public void InitialTasksExposedCorrectly()
        {
            var tasksQueue = new Subject<ITaskRun>();

            var nodeNames = new[] { "Node1", "Node2", "Node3" };
            var target = new CommandRunViewModel(nodeNames, tasksQueue, null);
            Expect(target.NodeRuns.Count(), EqualTo(3).After(100));
            Expect(target.NodeRuns, All.Property(nameof(NodeRunViewModel.State)).EqualTo(NodeRunState.Waiting));
            Expect(target.NodeRuns.Select(nr => nr.Name), EquivalentTo(nodeNames));
        }

        [Test]
        public void NewTaskRunsExposed()
        {
            const string output = "123";

            var tr = new FakeTaskRun
            {
                NodeName = "Node",
                Output = output.Select(c => new string(c, 1)).ToObservable(),
                ReturnCode = Observable.Never<int>()
            };
            var tasksQueue = new Subject<ITaskRun>();
            var target = new CommandRunViewModel(new[] { "Node" }, tasksQueue, null);
            tasksQueue.OnNext(tr);
            Expect(target.NodeRuns.Count(), EqualTo(1).After(100));
            Expect(target.NodeRuns.First().Output, EqualTo(output));
        }

        [Test]
        public void CantCloseUntilTasksSourceAndRunningTasksCompleted(
            [Values(true, false)]bool CompleteTasksQueueFirst)
        {
            var returnCode = new AsyncSubject<int>();

            var tr = new FakeTaskRun
            {
                NodeName = "Node",
                Output = Observable.Never<string>(),
                ReturnCode = returnCode
            };

            var tasksQueue = new Subject<ITaskRun>();
            var target = new CommandRunViewModel(new[] { "Node" }, tasksQueue, null);

            var seq = new Action[]
            {
                () =>
                {
                    tasksQueue.OnNext(tr);
                    tasksQueue.OnCompleted();
                },
                () =>
                {
                    returnCode.OnNext(42);
                    returnCode.OnCompleted();
                }
            };

            if (!CompleteTasksQueueFirst)
                Array.Reverse(seq);
            DispatcherUtil.DoEvents();

            Expect(target.CloseCommand.CanExecute(null), False);

            seq.First()();
            DispatcherUtil.DoEvents();

            Expect(target.CloseCommand.CanExecute(null), False);

            seq.Last()();
            DispatcherUtil.DoEvents();

            Expect(target.CloseCommand.CanExecute(null), True.After(100));
        }
    }
}
