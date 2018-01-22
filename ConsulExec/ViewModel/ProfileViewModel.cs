using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public class ProfileViewModel : ReactiveObject
    {
        public ProfileViewModel(SequentialStartupOptions SequentialStartupOptions)
        {
            Options = SequentialStartupOptions;
        }

        public SequentialStartupOptions Options
        {
            get { return options; }
            set { options = value; this.RaisePropertyChanged(nameof(Name)); }
        }
        private SequentialStartupOptions options;

        public string Name => Options.Name;
    }
}
