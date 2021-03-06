﻿using System;
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
            ServerAddress = "http://www.www.www";
        }

        public ConnectionOptionsEditorViewModel(ProfileViewModel<ConnectionOptions> Options,
            ConnectionOptionsFactoryDelegate ConnectionOptionsFactory,
            IActivatingViewModel Activator)
            : base(Options, Activator)
        {
            connectionOptionsFactory = ConnectionOptionsFactory;
            Map(Options.Options);
            IsValid = this.WhenAnyValue(v => v.Name,
                v => v.ServerAddress,
                (name, addr) => !string.IsNullOrEmpty(name) && IsValidUri(addr));

        }

        private static bool IsValidUri(string addr)
        {
            Uri r;
            return Uri.TryCreate(addr, UriKind.Absolute, out r);
        }

        public string Name { get { return name; } set { this.RaiseAndSetIfChanged(ref name, value); } }
        private string name;

        public string ServerAddress { get { return serverAddress; } set { this.RaiseAndSetIfChanged(ref serverAddress, value); } }
        private string serverAddress;

        protected override void OnDeactivate(bool Canceled)
        {
            if (!Canceled)
            {
                Options.Options = connectionOptionsFactory(Name);
                Options.Options.ServerAddress = ServerAddress;
            }

            base.OnDeactivate(Canceled);
        }

        private readonly ConnectionOptionsFactoryDelegate connectionOptionsFactory;

        private void Map(ConnectionOptions ConnectionOptions)
        {
            Name = ConnectionOptions.Name;
            ServerAddress = ConnectionOptions.ServerAddress;
        }
    }
}