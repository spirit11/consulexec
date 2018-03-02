using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace ConsulExec.Tests.EndToEnd
{
    public class Application : IDisposable
    {
        public Application(string Path)
        {
            automation = new UIA3Automation();
            var app = FlaUI.Core.Application.Launch(Path);
            window = app.GetMainWindow(automation);
        }

        public void AddNewProfile()
        {
            var addButton = window.FindAllDescendants(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
            addButton.First().Click();
            Task.Delay(100).Wait();
        }

        public void AddNewConnection()
        {
            var addButton = window.FindAllDescendants(c => c.ByName("Add").And(c.ByControlType(ControlType.Button)));
            addButton.Last().Click();
            Task.Delay(100).Wait();
        }

        public void EnterLocalhostAddress(string Address)
        {
            var t = window.FindAllDescendants(c => c.ByControlType(ControlType.Edit))
                .First(v => v.AsTextBox().Text.Contains("localhost"));
            t.Focus();
            t.AsTextBox().Text = Address;
            window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();
        }

        public AutomationElement[] WaitForNodesCheckboxes()
        {
            return window.FindAllDescendants(c => c.ByControlType(ControlType.CheckBox));
        }

        public void SelectAllNodes(IEnumerable<AutomationElement> Checkboxes)
        {
            foreach (var cb in Checkboxes)
                cb.AsCheckBox().IsChecked = true;
        }

        public void AcceptSettings()
        {
            window.FindAllDescendants(c => c.ByName("Ok")).Last().AsButton().Click();
        }

        public void EnterCommand(string Command)
        {
            window.FindAllDescendants(c => c.ByControlType(ControlType.ComboBox))
                .Last().AsComboBox().EditableText = Command;
        }

        public void ClickExecute()
        {
            window.FindAllDescendants(c => c.ByName("Execute")).Last().AsButton().Click();
        }

        public IEnumerable<string> GetAllTextBoxesText()
        {
            return window.FindAllDescendants(t => t.ByControlType(ControlType.Text)).Select(t => t.AsLabel().Text);
        }

        public void Close()
        {
            window.Close();
        }

        public void Dispose()
        {
            automation?.Dispose();
        }

        private readonly Window window;
        private readonly UIA3Automation automation;
    }
}