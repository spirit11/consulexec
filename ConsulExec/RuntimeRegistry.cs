using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsulExec.Design;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;
using ConsulExec.ViewModel;
using ReactiveUI;
using StructureMap;

namespace ConsulExec
{
    public class RuntimeRegistry : Registry
    {
        public RuntimeRegistry(bool FakeConnection = false)
        {
            Policies.OnMissingFamily(new DefaultInterfaceImplementationPolicy());

            var configuration = new Configuration();
            For<Configuration>().Use(configuration);

            For<ReactiveList<ProfileViewModel<ConnectionOptions>>>()
                .Use(new ReactiveList<ProfileViewModel<ConnectionOptions>>())
                .OnCreation((ctxt, list) => BindCollections(ctxt, list));

            For<ReactiveList<ProfileViewModel<StartupOptions>>>()
                .Use(new ReactiveList<ProfileViewModel<StartupOptions>>())
                .OnCreation((ctxt, list) => BindCollections(ctxt, list));

            var сonnectionOptionsFactoryDelegate = FakeConnection
                ? Name => new FakeConnectionOptions { Name = Name, ServerAddress = "http://localhost:8500" }
                : new ConnectionOptionsFactoryDelegate(Name => new ConnectionOptions { Name = Name, ServerAddress = "http://localhost:8500" });

            For<ConnectionOptionsFactoryDelegate>().Use(сonnectionOptionsFactoryDelegate);

            For<IEditorsFactory>().Use<EditorsFactory>();

            For<StartupOptionsFactoryDelegate>()
                .Use(new StartupOptionsFactoryDelegate(Name => new SequentialStartupOptions(Array.Empty<string>()) { Name = Name }));

            ForConcreteType<ConnectionProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<ConnectionOptions>>.EditProfileDelegate>()
                .Is(ctxt => ctxt.GetInstance<IEditorsFactory>().EditConnectionOptions);

            ForConcreteType<MainWindowViewModel>().Configure.Singleton();

            For<IActivatingViewModel>()
                .Use(ctxt => ctxt.GetInstance<MainWindowViewModel>());

            ForConcreteType<StartupOptionsProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<StartupOptions>>.EditProfileDelegate>()
                .Is(ctxt => ctxt.GetInstance<IEditorsFactory>().EditStartupOptions);

            var executeCommandHandler = For<Action<StartupOptions, string>>()
                .Use(ctxt => StartCommand(ctxt));

            For<ReactiveList<CommandViewModel>>()
                .Use(new ReactiveList<CommandViewModel>())
                .OnCreation((ctxt, list) => BindCollectionsImpl(ctxt, list));

            ForConcreteType<CommandStartupViewModel>()
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .Is(executeCommandHandler)
                .OnCreation((context, Model) => FillTestData(context, Model));
        }

        private static void BindCollections(IContext Ctxt, ReactiveList<ProfileViewModel<StartupOptions>> List) =>
            BindCollectionsImpl(Ctxt, List, ProfileViewModelsFactory.Create, c => c.Starup);

        private static void BindCollections(IContext Ctxt, ReactiveList<ProfileViewModel<ConnectionOptions>> List) =>
            BindCollectionsImpl(Ctxt, List, ProfileViewModelsFactory.Create, c => c.Connections);

        private static void BindCollectionsImpl<T>(IContext Ctxt,
            ReactiveList<ProfileViewModel<T>> List,
            Func<T, ProfileViewModel<T>> ViewModelFactory,
            Func<Configuration, IList<T>> ConfigurationProperty)
        {
            List.BindTo(ConfigurationProperty(Ctxt.GetInstance<Configuration>()),
                ViewModelFactory,
                Model => Model.Options);
        }

        private static void BindCollectionsImpl(IContext ctxt, ReactiveList<CommandViewModel> list) =>
            list.BindTo(ctxt.GetInstance<Configuration>().MruCommands, v => new CommandViewModel(v), v => v.Command);

        private static Action<StartupOptions, string> StartCommand(IContext Context) =>
            (options, command) =>
            {
                var tasks = options.Construct(command);
                var mvm = Context.GetInstance<IActivatingViewModel>();
                mvm.Activate(new CommandRunViewModel(options.Nodes, tasks, mvm)
                //Context.GetInstance<Func<string[], IObservable<ITaskRun>, CommandRunViewModel>>()(options.Nodes, tasks)
                //Context.GetInstance<CommandRunViewModel>()
                );
            };

        [Conditional("DEBUG")]
        private static void FillTestData(IContext Context, CommandStartupViewModel CommandStartupViewModel)
        {
            // don't add test data if some values are loaded from config
            if (!Context.GetInstance<Configuration>().Connections.Any())
            {
                CommandStartupViewModel.AddRecentCommands(new[]
                {
                    "echo ok",
                    "ping ya.ru"
                });

                var factory = Context.GetInstance<ConnectionOptionsFactoryDelegate>();

                var connectionOptions = ConstructConnectionOptions("node01", "http://192.168.1.101:8500", factory);

                CommandStartupViewModel.ConnectionProfiles.List.Add(
                    ProfileViewModelsFactory.Create(ConstructConnectionOptions("unexisting server", "http://serv1",
                        factory)));
                CommandStartupViewModel.ConnectionProfiles.List.Add(ProfileViewModelsFactory.Create(connectionOptions));

                CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                    ProfileViewModelsFactory.Create(
                        new SequentialStartupOptions(new[] { "Val-Pc2" })
                        {
                            Name = "opt",
                            Connection = connectionOptions
                        }));
            }

            CommandStartupViewModel.Command = CommandStartupViewModel.RecentCommands.LastOrDefault();

            CommandStartupViewModel.StartupOptionsProfiles.Profile =
                CommandStartupViewModel.StartupOptionsProfiles.List.FirstOrDefault(); //Maybe use it as vm feature?
        }

        private static ConnectionOptions ConstructConnectionOptions(string Name,
            string ServerAddress,
            ConnectionOptionsFactoryDelegate Factory)
        {
            var result = Factory(Name);
            result.ServerAddress = ServerAddress;
            return result;
        }
    }
}