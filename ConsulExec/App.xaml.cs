using System.Windows;
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

            var container = new Container(new RuntimeRegistry());

            var dependencyResolver = new StructureMapToSplatLocatorAdapter(container, Locator.Current);
            Locator.Current = dependencyResolver;
            dependencyResolver.InitializeReactiveUI();
            dependencyResolver.InitializeSplat();

            container.Configure(x => x.For<IViewLocator>().ClearAll().Use<ConventionalViewLocator>().Singleton());

            this.Log().Info("Started");
            var mainWindowViewModel = container.GetInstance<MainWindowViewModel>();
            mainWindowViewModel.Activate(container.GetInstance<CommandStartupViewModel>());

            MainWindow = new MainWindow { DataContext = mainWindowViewModel };
            MainWindow.Show();
        }
    }
}
