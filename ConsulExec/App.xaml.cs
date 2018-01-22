using System.Windows;
using ReactiveUI;
using Splat;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;
using ConsulExec.ViewModel;

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

            ConfigureLocator(Locator.CurrentMutable);

            this.Log().Info("Started");

            var mainWindowViewModel = new MainWindowViewModel();

            var content = new CommandStartupViewModel(
                new CommandStartupSuccesorsFabric((options, command) =>
                {
                    var executeService = Locator.Current.GetService<IRemoteExecution>();
                    var tasks = options.Construct(executeService, command);

                    return new CommandRunViewModel(options.Nodes, tasks, mainWindowViewModel);
                }, mainWindowViewModel)
                , mainWindowViewModel);

            mainWindowViewModel.Activate(content);

            MainWindow = new MainWindow { DataContext = mainWindowViewModel };
            MainWindow.Show();
        }

        private static void ConfigureLocator(IMutableDependencyResolver X)
        {
            //X.InitializeSplat();
            X.InitializeReactiveUI();
            X.RegisterLazySingleton(() => new RemoteExecution(), typeof(IRemoteExecution));
            X.RegisterLazySingleton(() => new Design.FakeRemoteExecution(), typeof(IRemoteExecution));
            
            X.RegisterLazySingleton(() => new ConventionalViewLocator(), typeof(IViewLocator));
        }
    }
}
