using System;
using System.Diagnostics;
using ConsulExec.Design;
using ConsulExec.Domain;
using ConsulExec.ViewModel;
using ReactiveUI;
using StructureMap;

namespace ConsulExec
{
    internal class RuntimeRegistry : Registry
    {
        public RuntimeRegistry(bool FakeConnection = false)
        {
            Policies.OnMissingFamily(new DefaultInterfaceImplementationPolicy());

            For<ReactiveList<ProfileViewModel<ConnectionOptions>>>().
                Use(new ReactiveList<ProfileViewModel<ConnectionOptions>>()); //cfg

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
                .Is(ctxt => ctxt.GetInstance<IEditorsFactory>().EditStartupOptions)
                .Ctor<ReactiveList<ProfileViewModel<StartupOptions>>>()
                .Is(new ReactiveList<ProfileViewModel<StartupOptions>>()); //cfg

            var executeCommandHandler = For<Action<StartupOptions, string>>()
                .Use(ctxt => StartCommand(ctxt));

            ForConcreteType<CommandStartupViewModel>() //cfg
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .Is(executeCommandHandler)
                .OnCreation((context, Model) => FillTestData(context, Model));
        }


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
            CommandStartupViewModel.Command = "echo ok";
            CommandStartupViewModel.AddRecentCommands(new[]
            {
                "echo ok",
                "ping ya.ru"
            });

            var factury = Context.GetInstance<ConnectionOptionsFactoryDelegate>();

            var connectionOptions = ConstructConnectionOptions("node01", "http://192.168.1.101:8500", factury);

            CommandStartupViewModel.ConnectionProfiles.List.Add(
                ProfileViewModelsFactory.Create(ConstructConnectionOptions("unexisting server", "http://serv1", factury)));
            CommandStartupViewModel.ConnectionProfiles.List.Add(ProfileViewModelsFactory.Create(connectionOptions));

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfileViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt", Connection = connectionOptions }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];
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