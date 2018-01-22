using System.Collections.Specialized;
using System.Linq;
using ConsulExec.ViewModel;
using NUnit.Framework;

namespace ConsulExec.Tests.ViewModel
{
    [TestFixture]
    public class CommandStartupViewModelTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void AddRecentCommandWhenExecuted()
        {
            var target = new CommandStartupViewModel(
                new CommandStartupSuccesorsFabric((_1, _2) => new CommandRunViewModel(), new MainWindowViewModel()));

            target.RecentCommands.Clear();
            target.Command = "42";

            var notifications = new NotificationHelper<NotifyCollectionChangedEventArgs>(target.RecentCommands, nameof(target.RecentCommands.CollectionChanged));
            using (notifications.Run())
            { }
            using (notifications.Run())
            {
                target.ExecuteCommand.Execute(null);
                Expect(target.RecentCommands, Is.EquivalentTo(new[] { "42" }));
            }

            var args = notifications.Args;

            Expect(args, Has.Count.EqualTo(1));
            Expect(args.First().Action, EqualTo(NotifyCollectionChangedAction.Add));
            Expect(args.First().NewItems, EqualTo(new[] { "42" }));

            Expect(notifications.Senders, All.EqualTo(target.RecentCommands));
        }
    }
}
