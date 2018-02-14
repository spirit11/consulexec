using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using NUnit.Framework;

namespace ConsulExec.Tests.EndToEnd
{
    [TestFixture]
    public class EndToEndTests : AssertionHelper
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            File.WriteAllText(ConsulConfigFileName, ConfigFileContent);
            var consulExecutable = ConfigurationManager.AppSettings["ConsulExecutable"] ??
                                   Path.Combine(TestContext.CurrentContext.TestDirectory, "consul.exe");
            Expect(File.Exists(consulExecutable), True, "Can't find consul.exe. Add path to consul.exe under ConsulExecutable key in app.config.");
            var arguments = $"agent -dev -config-file=\"{ConsulConfigFileName}\" -http-port={HttpApiPort}";
            consulProcess = Process.Start(consulExecutable, arguments);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(ConsulConfigFileName);
            consulProcess.Kill();
        }

        [Test]
        [Explicit]
        public void Test()
        {
            var app = Application.Launch(Path.Combine(TestContext.CurrentContext.TestDirectory, "ConsulExec.exe"));
            Task.Delay(1000).Wait();

            using (var automation = new UIA3Automation())
            {
                window = app.GetMainWindow(automation);

                AddNewProfile();

                AddNewConnection();

                EnterLocalhostAddress();

                var checkboxes = WaitForNodesCheckboxes();

                Expect(checkboxes?.Length, GreaterThan(0), "No checkboxes for nodes are found");

                SelectAllNodes(checkboxes);

                AcceptSettings();

                window.FindAllDescendants(c => c.ByControlType(ControlType.ComboBox)).Last().AsComboBox().EditableText =
                    $"echo {EchoString}";

                window.FindAllDescendants(c => c.ByName("Execute")).Last().AsButton().Click();

                Task.Delay(3000).Wait();

                var textBoxes = window.FindAllDescendants(t => t.ByControlType(ControlType.Text));

                Expect(textBoxes.Select(t => t.AsLabel().Text),
                    Some.Contains("Completed").And.Some.Contains(EchoString));

                window.Close();
            }
        }


        private const string ConfigFileContent =
            @"{
    ""server"" : true,
    ""bootstrap_expect"" : 1,	
    ""disable_remote_exec"" : false,
    ""log_level"" : ""DEBUG""
}";

        private const int HttpApiPort = 8505; // 8500 is default
        private const string EchoString = "test passed";

        private static string ConsulConfigFileName =>
             Path.Combine(TestContext.CurrentContext.TestDirectory, "test_server_config.json");


        private Window window;
        private Process consulProcess;

        private void AddNewProfile()
        {
            var addButton = window.FindAllDescendants(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
            addButton.First().Click();
            Task.Delay(100).Wait();
        }

        private void AddNewConnection()
        {
            var addButton = window.FindAllDescendants(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
            addButton.Last().Click();
            Task.Delay(100).Wait();
        }

        private void EnterLocalhostAddress()
        {
            var t = window.FindAllDescendants(c => c.ByControlType(ControlType.Edit))
                .First(v => v.AsTextBox().Text.Contains("localhost"));
            t.Focus();
            t.AsTextBox().Text = $"http://localhost:{HttpApiPort}";
            window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();
        }

        private AutomationElement[] WaitForNodesCheckboxes()
        {
            AutomationElement[] checkboxes = null;
            for (int i = 0;
                i < 10
                && (checkboxes = window.FindAllDescendants(c => c.ByControlType(ControlType.CheckBox))).Length == 0;
                i++)
                Task.Delay(1000).Wait();
            return checkboxes;
        }

        private void SelectAllNodes(AutomationElement[] Checkboxes)
        {
            foreach (var cb in Checkboxes)
                cb.AsCheckBox().IsChecked = true;
        }

        private void AcceptSettings()
        {
            window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();
        }
    }
}
