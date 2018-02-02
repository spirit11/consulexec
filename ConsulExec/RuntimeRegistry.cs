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

            For<ConnectionProfilesViewModel.OptionsFactoryDelegate>()
                .Use(new ConnectionProfilesViewModel.OptionsFactoryDelegate(Name => new ConnectionOptions { Name = Name }));
            For<IEditorsFabric>().Use<EditorsFabric>();

            For<StartupOptionsProfilesViewModel.OptionsFabricDelegate>()
                .Use(new StartupOptionsProfilesViewModel.OptionsFabricDelegate(Name => new SequentialStartupOptions(Array.Empty<string>()) { Name = Name }));

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
                .OnCreation(Model => FillTestData(Model));
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
        private static void FillTestData(CommandStartupViewModel CommandStartupViewModel)
        {
            CommandStartupViewModel.Command = "echo ok";
            CommandStartupViewModel.RecentCommands.Add("echo ok");
            CommandStartupViewModel.RecentCommands.Add("ping ya.ru");

            var connectionOptions = new ConnectionOptions { Name = "serv2", ServerAddress = "http://serv2" };
            CommandStartupViewModel.ConnectionProfiles.List.Add(
                new ProfileViewModel<ConnectionOptions>(new ConnectionOptions { Name = "serv1", ServerAddress = "http://serv1" }, f => f.Name));
            CommandStartupViewModel.ConnectionProfiles.List.Add(new ProfileViewModel<ConnectionOptions>(connectionOptions, f => f.Name));

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt", Connection = connectionOptions }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];
        }
    }
}