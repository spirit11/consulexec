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

            ForConcreteType<ConnectionProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<ConnectionOptions>>.EditProfileDelegate>()
                .Is((p, s) => { });

            ForConcreteType<MainWindowViewModel>().Configure.Singleton();

            For<IActivatingViewModel>()
                .Use(ctxt => ctxt.GetInstance<MainWindowViewModel>());

            ForConcreteType<StartupOptionsProfilesViewModel>()
                .Configure
                .Ctor<ProfilesViewModel<ProfileViewModel<StartupOptions>>.EditProfileDelegate>()
                .Is(ctxt => EditorsFabric.EditStartupOptions(ctxt.GetInstance<IActivatingViewModel>()))
                .Ctor<ReactiveList<ProfileViewModel<StartupOptions>>>()
                .Is(new ReactiveList<ProfileViewModel<StartupOptions>>());

            ForConcreteType<CommandRunViewModel>();

            For<Action<StartupOptions, string>>().Use("executeCommandHandler",
                    ctxt =>
                    {
                        return new Action<StartupOptions, string>((options, command) =>
                        {
                            var executeService = ctxt.GetInstance<IRemoteExecution>();
                            var tasks = options.Construct(executeService, command);
                            var mvm = ctxt.GetInstance<IActivatingViewModel>();
                            mvm.Activate(
                                new CommandRunViewModel(options.Nodes, tasks, mvm)
                            //ctxt.GetInstance<Func<string[], IObservable<ITaskRun>, CommandRunViewModel>>()(options.Nodes, tasks)
                            //ctxt.GetInsta    nce<CommandRunViewModel>()
                            );
                        });
                    })
                .Named("executeCommandHandler");

            ForConcreteType<CommandStartupViewModel>()
                .Configure
                .Ctor<Action<StartupOptions, string>>()
                .IsNamedInstance("executeCommandHandler")
                .OnCreation(Model => FillTestData(Model));
        }


        [Conditional("DEBUG")]
        private static void FillTestData(CommandStartupViewModel CommandStartupViewModel)
        {
            CommandStartupViewModel.Command = "echo ok";
            CommandStartupViewModel.RecentCommands.Add("echo ok");
            CommandStartupViewModel.RecentCommands.Add("ping ya.ru");

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt" }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];

            CommandStartupViewModel.ConnectionProfiles.List.Add(new ProfileViewModel<ConnectionOptions>(new ConnectionOptions { Name = "serv1" }, f => f.Name));
            CommandStartupViewModel.ConnectionProfiles.List.Add(new ProfileViewModel<ConnectionOptions>(new ConnectionOptions { Name = "serv2" }, f => f.Name));
        }
    }
}