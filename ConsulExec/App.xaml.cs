using System.IO;
using System.Windows;
using ConsulExec.Domain;
using ReactiveUI;
using Splat;
using ConsulExec.Infrastructure;
using ConsulExec.ViewModel;
using StructureMap;

namespace ConsulExec
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IEnableLogger
    {
        protected override void OnStartup(StartupEventArgs E)
        {
            base.OnStartup(E);

            container = new Container(new RuntimeRegistry());

            var dependencyResolver = new StructureMapToSplatLocatorAdapter(container, Locator.Current);
            Locator.Current = dependencyResolver;
            dependencyResolver.InitializeReactiveUI();
            dependencyResolver.InitializeSplat();

            container.Configure(x => x.For<IViewLocator>().ClearAll().Use<ConventionalViewLocator>().Singleton());

            container.Configure(x => x.For<Configuration>().Use(LoadConfiguration()));

            this.Log().Info("Started");

            var mainWindowViewModel = container.GetInstance<MainWindowViewModel>();
            mainWindowViewModel.Activate(container.GetInstance<CommandStartupViewModel>());

            MainWindow = new MainWindow { DataContext = mainWindowViewModel };

            MainWindow.Show();
        }

        private static Configuration LoadConfiguration()
        {
            return new Configuration();
            //Configuration.ReadFrom()
        }

        protected override void OnExit(ExitEventArgs E)
        {
            container.GetInstance<Configuration>().SaveTo(new StringWriter());
            container?.Dispose();
            base.OnExit(E);
        }

        private Container container;
    }
}
