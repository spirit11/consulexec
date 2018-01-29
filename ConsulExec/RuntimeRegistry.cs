using System;
using System.Diagnostics;
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
            For<IRemoteExecution>().Use<Design.FakeRemoteExecution>().Singleton();

            ForConcreteType<ReactiveList<ProfileViewModel<ConnectionOptions>>>().Configure.Singleton();

            ForConcreteType<ConnectionProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<ConnectionOptions>>.EditProfileDelegate>()
                .Is(ctxt => EditorsFabric.EditConnectionOptions(ctxt.GetInstance<IActivatingViewModel>()));

            ForConcreteType<MainWindowViewModel>().Configure.Singleton();

            For<IActivatingViewModel>()
                .Use(ctxt => ctxt.GetInstance<MainWindowViewModel>());

            ForConcreteType<StartupOptionsProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<StartupOptions>>.EditProfileDelegate>()
                .Is(ctxt => EditorsFabric.EditStartupOptions(ctxt.GetInstance<IActivatingViewModel>(), ctxt.GetInstance<ConnectionProfilesViewModel>()))
                .Ctor<ReactiveList<ProfileViewModel<StartupOptions>>>()
                .Is(new ReactiveList<ProfileViewModel<StartupOptions>>());

            ForConcreteType<CommandRunViewModel>();

            For<Action<StartupOptions, string>>().Use(ctxt => StartCommand(ctxt))
                .Named("executeCommandHandler");

            ForConcreteType<CommandStartupViewModel>()
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .IsNamedInstance("executeCommandHandler")
                .OnCreation(Model => FillTestData(Model));
        }

        private static Action<StartupOptions, string> StartCommand(IContext Context) =>
            (options, command) =>
            {
                var executeService = Context.GetInstance<IRemoteExecution>();
                var tasks = options.Construct(executeService, command);
                var mvm = Context.GetInstance<IActivatingViewModel>();
                mvm.Activate(new CommandRunViewModel(options.Nodes, tasks, mvm)
                //Context.GetInstance<Func<string[], IObservable<ITaskRun>, CommandRunViewModel>>()(options.Nodes, tasks)
                //Context.GetInstance<CommandRunViewModel>()
                );
            };


        [Conditional("DEBUG")]
        private static void FillTestData(CommandStartupViewModel CommandStartupViewModel)
        {
            CommandStartupViewModel.Command = "echo ok";
            CommandStartupViewModel.RecentCommands.Add("echo ok");
            CommandStartupViewModel.RecentCommands.Add("ping ya.ru");

            var connectionOptions = new ConnectionOptions { Name = "serv2" };
            CommandStartupViewModel.ConnectionProfiles.List.Add(new ProfileViewModel<ConnectionOptions>(new ConnectionOptions { Name = "serv1" }, f => f.Name));
            CommandStartupViewModel.ConnectionProfiles.List.Add(new ProfileViewModel<ConnectionOptions>(connectionOptions, f => f.Name));

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt", Connection = connectionOptions }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];
        }
    }
}