using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using NUnit.Framework;

namespace ConsulExec.Tests.EndToEnd
{
    [TestFixture]
    public class EndToEndTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Test()
        {
            var app = Application.Launch(Path.Combine(TestContext.CurrentContext.TestDirectory, "ConsulExec.exe"));
            Task.Delay(1000).Wait();

            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation);
                var addButton = window.FindFirstDescendant(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
                addButton.Click();
                Task.Delay(100).Wait();

                var addButton2 = window.FindAllDescendants(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
                addButton2.Last().Click();
                Task.Delay(100).Wait();

                var t = window.FindAllDescendants(c => c.ByControlType(ControlType.Edit)).First(v => v.AsTextBox().Text.Contains("localhost"));
                t.Focus();
                t.AsTextBox().Text = "http://localhost:8282";
                window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();

                Task.Delay(3000).Wait();

                foreach (var cb in window.FindAllDescendants(c => c.ByControlType(ControlType.CheckBox)))
                    cb.AsCheckBox().IsChecked = true;

                window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();
                //window.Close();
            }
        }

    }
}
