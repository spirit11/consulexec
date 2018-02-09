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
        public RuntimeRegistry()
        {
            Policies.OnMissingFamily(new DefaultInterfaceImplementationPolicy());

            ForConcreteType<ReactiveList<ProfileViewModel<ConnectionOptions>>>().Configure.Singleton();

            For<ConnectionOptionsFactoryDelegate>()
                //.Use(new ConnectionOptionsFactoryDelegate(Name => new ConnectionOptions { Name = Name }));
                .Use(new ConnectionOptionsFactoryDelegate(Name => new FakeConnectionOptions { Name = Name }));
            For<IEditorsFabric>().Use<EditorsFabric>();

            For<StartupOptionsFabricDelegate>()
                .Use(new StartupOptionsFabricDelegate(Name => new SequentialStartupOptions(Array.Empty<string>()) { Name = Name }));

            ForConcreteType<ConnectionProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<ConnectionOptions>>.EditProfileDelegate>()
                .Is(ctxt => ctxt.GetInstance<IEditorsFabric>().EditConnectionOptions);

            ForConcreteType<MainWindowViewModel>().Configure.Singleton();

            For<IActivatingViewModel>()
                .Use(ctxt => ctxt.GetInstance<MainWindowViewModel>());

            ForConcreteType<StartupOptionsProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<StartupOptions>>.EditProfileDelegate>()
                .Is(ctxt => ctxt.GetInstance<IEditorsFabric>().EditStartupOptions)
                .Ctor<ReactiveList<ProfileViewModel<StartupOptions>>>()
                .Is(new ReactiveList<ProfileViewModel<StartupOptions>>());

            For<Action<StartupOptions, string>>()
                .Use(ctxt => StartCommand(ctxt))
                .Named("executeCommandHandler");

            ForConcreteType<CommandStartupViewModel>()
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .IsNamedInstance("executeCommandHandler")
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
            CommandStartupViewModel.RecentCommands.Add("echo ok");
            CommandStartupViewModel.RecentCommands.Add("ping ya.ru");

            var fabric = Context.GetInstance<ConnectionOptionsFactoryDelegate>();

            var connectionOptions = ConstructConnectionOptions("serv2", "http://192.168.1.101:8500", fabric);

            CommandStartupViewModel.ConnectionProfiles.List.Add(
                ProfilesViewModelsFactory.Create(ConstructConnectionOptions("serv1", "http://serv1", fabric)));
            CommandStartupViewModel.ConnectionProfiles.List.Add(ProfilesViewModelsFactory.Create(connectionOptions));

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt", Connection = connectionOptions }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];
        }

        private static ConnectionOptions ConstructConnectionOptions(string Name,
            string ServerAddress,
            ConnectionOptionsFactoryDelegate Fabric)
        {
            var result = Fabric(Name);
            result.ServerAddress = ServerAddress;
            return result;
        }
    }
}