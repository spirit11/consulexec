﻿using System.Diagnostics;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    [DebuggerDisplay("Node: {" + nameof(Name) + "} IsAbsent={" + nameof(IsAbsent) + "}")]
    public class NodeSelectorViewModel : ReactiveObject
    {
        public NodeSelectorViewModel(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; }

        public bool IsChecked
        {
            get { return isChecked; }
            set { this.RaiseAndSetIfChanged(ref isChecked, value); }
        }
        private bool isChecked;

        public bool IsAbsent
        {
            get { return isAbsent; }
            set { this.RaiseAndSetIfChanged(ref isAbsent, value); }
        }
        private bool isAbsent;
    }
}