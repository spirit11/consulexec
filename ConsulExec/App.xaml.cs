using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ReactiveUI;
using Splat;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;
using ConsulExec.ViewModel;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace ConsulExec
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IEnableLogger
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureLocator();

            this.Log().Info("Started");

            var mainWindowViewModel = new MainWindowViewModel();

            var undoListViewModel = new UndoListViewModel();

            var connectionProfilesViewModel = new ConnectionProfilesViewModel(null, undoListViewModel);

            var startupOptionsProfilesViewModel = new StartupOptionsProfilesViewModel(CommandStartupSuccesorsFabric.EditProfile(mainWindowViewModel),
                undoListViewModel,
                connectionProfilesViewModel);

            var content = new CommandStartupViewModel(connectionProfilesViewModel,
                startupOptionsProfilesViewModel,
                (options, command) =>
                {
                    var executeService = Locator.Current.GetService<IRemoteExecution>();
                    var tasks = options.Construct(executeService, command);
                    mainWindowViewModel.Activate(new CommandRunViewModel(options.Nodes, tasks, mainWindowViewModel));
                });


            FillTestData(content);

            mainWindowViewModel.Activate(content);

            MainWindow = new MainWindow { DataContext = mainWindowViewModel };
            MainWindow.Show();
        }

        private static void ConfigureLocator()
        {
            var container = new Container(x =>
                {
                    x.Policies.OnMissingFamily(new DefaultImplementationPolicy());
                    //x.For<IRemoteExecution>().Use<RemoteExecution>().Singleton();
                    x.For<IRemoteExecution>().Use<Design.FakeRemoteExecution>().Singleton();
                });
            var dependencyResolver = new MyLocator(container, Locator.Current);
            Locator.Current = dependencyResolver;
            dependencyResolver.InitializeReactiveUI();
            dependencyResolver.InitializeSplat();

            container.Configure(x=> x.For<IViewLocator>().Use<ConventionalViewLocator>().Singleton());
        }

        [Conditional("DEBUG")]
        private static void FillTestData(CommandStartupViewModel CommandStartupViewModel)
        {
            CommandStartupViewModel.Command = "echo ok";
            if (!ModeDetector.InDesignMode())
            {
                CommandStartupViewModel.RecentCommands.Add("echo ok");
                CommandStartupViewModel.RecentCommands.Add("ping ya.ru");
            }

            CommandStartupViewModel.StartupOptionsProfiles.List.Add(
                ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt" }));
            CommandStartupViewModel.StartupOptionsProfiles.Profile = CommandStartupViewModel.StartupOptionsProfiles.List[0];
        }
    }

    internal class DefaultImplementationPolicy : IFamilyPolicy
    {
        public PluginFamily Build(Type type)
        {
            var typeName = type.FullName;
            var dotBeforeName = Math.Max(typeName.LastIndexOf('.'), typeName.LastIndexOf('+'));

            var t = Type.GetType(typeName.Remove(dotBeforeName + 1, 1)); // removing 'I' Namespace.IFoo => Namespace.Foo TODO: +IFoo=>+Foo
            if (t != null)
            {
                var pluginFamily = new PluginFamily(type);
                pluginFamily.SetDefault(new ConstructorInstance(t));
                return pluginFamily;
            }

            return null;
        }

        public bool AppliesToHasFamilyChecks => false;
    }

    internal class MyLocator : IMutableDependencyResolver
    {
        private readonly Container locator;
        private readonly IDependencyResolver current;

        public MyLocator(Container Container, IDependencyResolver Current)
        {
            locator = Container;
            current = Current;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType, string contract = null)
        {
            Debug.WriteLine("Getting " + serviceType);
            var v1 = contract == null
                ? locator.TryGetInstance(serviceType)
                : locator.TryGetInstance(serviceType, contract);
            return v1 ?? current.GetService(serviceType, contract);
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null)
        {
            if (!string.IsNullOrEmpty(contract))
                throw new NotSupportedException("contract should not be set");
            return locator.GetAllInstances(serviceType).Cast<object>();
        }

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            Debug.WriteLine(serviceType);
            locator.Configure(x => x.For(serviceType).Use(factory()));
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback)
        {
            throw new NotImplementedException();
        }
    }

}
