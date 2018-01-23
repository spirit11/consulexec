using System;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public class ProfileViewModel<T> : ReactiveObject
    {
        public ProfileViewModel(T Profile, Func<T, string> NameFormatter)
        {
            nameFormatter = NameFormatter;
            Options = Profile;
        }

        public T Options
        {
            get { return options; }
            set { options = value; this.RaisePropertyChanged(nameof(Name)); }
        }
        private T options;

        public string Name => nameFormatter(Options);
        private readonly Func<T, string> nameFormatter;
    }
}