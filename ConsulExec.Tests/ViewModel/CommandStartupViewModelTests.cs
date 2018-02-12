using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using NUnit.Framework;
using ReactiveUI;

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
            var target = new CommandStartupViewModel(new ConnectionProfilesViewModel(null, null, null),
                new StartupOptionsProfilesViewModel(null, null, null, new ReactiveList<ProfileViewModel<StartupOptions>>()), (o, cmd) => { });

            target.ClearRecentCommands();
            target.Command = "42";

            var notifications = new NotificationHelper<NotifyCollectionChangedEventArgs>(target.RecentCommands, 
                nameof(INotifyCollectionChanged.CollectionChanged));
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

        [Test]
        public void CallbackOnExecute()
        {
            var cmds = new List<dynamic>();
            var options = new SequentialStartupOptions(new string[0]);
            var startupOptionsProfilesViewModel = new StartupOptionsProfilesViewModel(null, null, null, new ReactiveList<ProfileViewModel<StartupOptions>>())
            {
                Profile = ProfileViewModelsFabric.Create(options)
            };
            var target = new CommandStartupViewModel(new ConnectionProfilesViewModel(null, null, null),
                startupOptionsProfilesViewModel,
                (o, cmd) => cmds.Add(new { o, cmd }));

            target.ClearRecentCommands();
            target.Command = "42";
            target.ExecuteCommand.Execute(null);

            Expect(cmds.Select(v => v.cmd), EqualTo(new[] { "42" }));
            Expect(cmds.First().o, EqualTo(options));
        }
    }
}
