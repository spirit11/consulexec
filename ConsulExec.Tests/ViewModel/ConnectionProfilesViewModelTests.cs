using System;
using System.Collections.Generic;
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

    [TestFixture]
    public class ConnectionProfilesViewModelTests : AssertionHelper
    {
        [Test]
        public void CantDeleteUsedConnection()
        {
            var connections = new ReactiveList<ProfileViewModel<ConnectionOptions>>(new[]
                {
                    ProfileViewModelsFactory.Create(new ConnectionOptions()) ,
                    ProfileViewModelsFactory.Create(new ConnectionOptions()),
                    ProfileViewModelsFactory.Create(new ConnectionOptions())
                });

            ProfileViewModelsFactory.Create(
                new SequentialStartupOptions(Array.Empty<string>()) { Connection = connections[0].Options });


            var target = new ConnectionProfilesViewModel((Profile, Setup) => { },
                new UndoListViewModel(),
                connections)
            {
                Profile = connections.First()
            };

            Expect(target.DeleteCommand.CanExecute(null), Is.True);
        }
    }
}