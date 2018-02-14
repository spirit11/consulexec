using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        public void RunCommandOnLocalServer()
        {
            using (application = new Application(Path.Combine(TestContext.CurrentContext.TestDirectory, "ConsulExec.exe")))
            {
                Task.Delay(1000).Wait();

                application.AddNewProfile();

                application.AddNewConnection();

                application.EnterLocalhostAddress($"http://localhost:{HttpApiPort}");

                var checkboxes = application.WaitForNodesCheckboxes();

                Expect(() => checkboxes = application.WaitForNodesCheckboxes(), 
                    Length.GreaterThan(0).After(3000).PollEvery(100),
                    "No checkboxes for nodes are found");

                application.SelectAllNodes(checkboxes);

                application.AcceptSettings();

                application.EnterCommand($"echo {EchoString}");

                application.ClickExecute();

                Expect(() => application.GetAllTextBoxesText(), 
                    Some.Contains("Completed").And.Some.Contains(EchoString).After(3000).PollEvery(100),
                    "No results are repoted in UI");

                application.Close();
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

        private Process consulProcess;
        private Application application;
    }
}
