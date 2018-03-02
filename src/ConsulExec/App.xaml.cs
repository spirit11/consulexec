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

            container.Configure(x => x.For<Configuration>().Use(TryLoadConfiguration()));

            this.Log().Info("Started");

            var mainWindowViewModel = container.GetInstance<MainWindowViewModel>();
            mainWindowViewModel.Activate(container.GetInstance<CommandStartupViewModel>());

            MainWindow = new MainWindow { DataContext = mainWindowViewModel };

            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs E)
        {
            TrySaveConfiguration(container.GetInstance<Configuration>());

            container?.Dispose();
            base.OnExit(E);
        }

        private static string configFilename = "config.json";

        private static Configuration TryLoadConfiguration()
        {
            try
            {
                if (File.Exists(configFilename))
                    using (var f = File.OpenText(configFilename))
                    {
                        return Configuration.ReadFrom(f);
                    }
            }
            catch (System.Exception)
            {
            }
            return new Configuration();
        }

        private static void TrySaveConfiguration(Configuration Configuration)
        {
            try
            {
                using (var f = File.CreateText(configFilename))
                {
                    Configuration.SaveTo(f);
                    f.Close();
                }
            }
            catch (System.Exception)
            {
            }
        }

        private Container container;
    }
}
