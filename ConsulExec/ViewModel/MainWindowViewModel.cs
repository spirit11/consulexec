using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace ConsulExec.ViewModel
{
    public interface IActivatingViewModel
    {
        void Activate(object ViewModel);
        void Deactivate(object ViewModel);
    }

    public class MainWindowViewModel : ReactiveObject, IActivatingViewModel
    {
        public string Title { get; } = "Consul Exec";

        public object Content { get; private set; }

        public void Activate(object ViewModel)
        {
            if (Content != null)
                viewModels.Add(Content);
            Content = ViewModel;
            this.RaisePropertyChanged(nameof(Content));
        }

        public void Deactivate(object ViewModel)
        {
            Content = viewModels.Last();
            viewModels.Remove(Content);
            this.RaisePropertyChanged(nameof(Content));
        }

        private readonly List<object> viewModels = new List<object>();
    }
}
