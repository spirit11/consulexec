using System;
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
        public RuntimeRegistry(Configuration Configuration = null, bool FakeConnection = false)
        {
            Policies.OnMissingFamily(new DefaultInterfaceImplementationPolicy());

            var configuration = Configuration ?? new Configuration();
            For<Configuration>().Use(configuration);

            var connections = new ReactiveList<ProfileViewModel<ConnectionOptions>>();
            connections.BindTo(configuration.Connections,
                ProfileViewModelsFactory.Create,
                Model => Model.Options);
            For<ReactiveList<ProfileViewModel<ConnectionOptions>>>().
                Use(connections); //cfg

            var startups = new ReactiveList<ProfileViewModel<StartupOptions>>();
            startups.BindTo(configuration.Starup,
                ProfileViewModelsFactory.Create,
                Model => Model.Options);

            var mruCommands = new ReactiveList<string>();
            //mruCommands.BindTo(configuration.MruCommands,
            //    v => v,
            //    v => v);

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
                .Is(startups); //cfg

            var executeCommandHandler = For<Action<StartupOptions, string>>()
                .Use(ctxt => StartCommand(ctxt));

            ForConcreteType<CommandStartupViewModel>() //cfg
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .Is(executeCommandHandler)
                .Ctor<ReactiveList<string>>()
                .Is(mruCommands)
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

            // don't add test data if some values are loaded from config
            if (Context.GetInstance<Configuration>().Connections.Any())
                return;

            var factory = Context.GetInstance<ConnectionOptionsFactoryDelegate>();

            var connectionOptions = ConstructConnectionOptions("node01", "http://192.168.1.101:8500", factory);

            CommandStartupViewModel.ConnectionProfiles.List.Add(
                ProfileViewModelsFactory.Create(ConstructConnectionOptions("unexisting server", "http://serv1", factory)));
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