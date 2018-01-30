using System;
using ConsulExec.Domain;
using ReactiveUI;
using Splat;

namespace ConsulExec.ViewModel
{
    public class ConnectionOptionsEditorViewModel : BaseOptionsEditorViewModel<ProfileViewModel<ConnectionOptions>>
    {
        public ConnectionOptionsEditorViewModel()
            : base(null, null)
        {
            if (!ModeDetector.InDesignMode())
                throw new InvalidOperationException("Design only constructor");
            Name = "Some name";
        }

        public ConnectionOptionsEditorViewModel(ProfileViewModel<ConnectionOptions> Options, IActivatingViewModel Activator)
            : base(Options, Activator)
        {
            Name = Options.Name;
            IsValid = this.WhenAnyValue(v => v.Name, name => !string.IsNullOrEmpty(name));
        }

        public string Name { get { return name; } set { this.RaiseAndSetIfChanged(ref name, value); } }
        private string name;

        protected override void OnDeactivate(bool Canceled)
        {
            if (!Canceled)
                Options.Options.Name = Name;
            base.OnDeactivate(Canceled);
        }
    }
}