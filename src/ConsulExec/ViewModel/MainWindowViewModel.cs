﻿using ReactiveUI;
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
            Content = ViewModel;
            viewModels.Add(Content);
            this.RaisePropertyChanged(nameof(Content));
        }

        public void Deactivate(object ViewModel)
        {
            if (viewModels.Last() != ViewModel) //prevent double deactivation
                return;
            viewModels.Remove(Content);
            Content = viewModels.Last();
            this.RaisePropertyChanged(nameof(Content));
        }

        private readonly List<object> viewModels = new List<object>();
    }
}
