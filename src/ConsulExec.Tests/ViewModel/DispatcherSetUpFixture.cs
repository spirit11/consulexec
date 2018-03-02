using System.Reactive.Concurrency;
using System.Security.Permissions;
using System.Windows.Threading;
using NUnit.Framework;
using ReactiveUI;

namespace ConsulExec.Tests.ViewModel
{
    /// <summary>
    /// SetUpFixture class to initialize dispatcher. Fix debugging issue for viewmodel tests
    /// </summary>
    [SetUpFixture]
    public class DispatcherSetUpFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            RxApp.MainThreadScheduler = RxApp.TaskpoolScheduler;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dispatcher.InvokeShutdown();
        }

        private Dispatcher dispatcher;
    }


    public static class DispatcherUtil
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}