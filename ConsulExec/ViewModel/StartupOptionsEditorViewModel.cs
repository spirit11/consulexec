using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using ConsulExec.Domain;
using Splat;

namespace ConsulExec.ViewModel
{
    using ConnectionOptionsViewModel = ProfileViewModel<ConnectionOptions>;


    public class StartupOptionsEditorViewModel : BaseOptionsEditorViewModel<ProfileViewModel<StartupOptions>>
    {
        public StartupOptionsEditorViewModel() : base(null, null)
        {
            if (!ModeDetector.InDesignMode())
                throw new InvalidOperationException("Design only constructor");
            Name = "Some name";
            Nodes.Add(new NodeSelectorViewModel("Avail"));
            Nodes.Add(new NodeSelectorViewModel("Absent") { IsAbsent = true });
        }

        public StartupOptionsEditorViewModel(
            ProfileViewModel<StartupOptions> Options,
            IProfilesViewModel<ConnectionOptionsViewModel> Connections,
            IActivatingViewModel Activator,
            IObservable<string[]> NodesSource) : base(Options, Activator)
        {
            AssertNotNull(Options, nameof(Options));
            AssertNotNull(Connections, nameof(Connections));
            AssertNotNull(NodesSource, nameof(NodesSource));

            options = Options.Options;

            this.Connections = Connections;

            Map(Options.Options);

            IsValid = this.WhenAnyValue(v => v.Name,
                v => v.Connections.Profile,
                (s, p) => !string.IsNullOrWhiteSpace(s) && p != null);

            namesSubscription = NodesSource
                .StartWith(Enumerable.Repeat(new string[0], 1)) // initial values until first request is completed
                .Subscribe(names =>
                {
                    var absentNames = Nodes.ToDictionary(n => n.Name, n => n);
                    foreach (var name in names.OrderBy(v => v))
                    {
                        if (!absentNames.ContainsKey(name))
                            Nodes.Add(new NodeSelectorViewModel(name));
                        else
                        {
                            absentNames[name].IsAbsent = false;
                            absentNames.Remove(name);
                        }
                    }
                    foreach (var node in absentNames.Values)
                        node.IsAbsent = true;
                });
        }

        public IProfilesViewModel<ConnectionOptionsViewModel> Connections { get; }

        public ObservableCollection<NodeSelectorViewModel> Nodes { get; private set; } = new ObservableCollection<NodeSelectorViewModel>();

        public string Name { get { return name; } set { this.RaiseAndSetIfChanged(ref name, value); } }
        private string name;

        protected override void OnDeactivate(bool Canceled)
        {
            if (!Canceled)
                MapBack(options);
            namesSubscription.Dispose();
            base.OnDeactivate(Canceled);
        }

        private readonly IDisposable namesSubscription;
        private readonly StartupOptions options;

        private void MapBack(StartupOptions StartupOptions)
        {
            StartupOptions.Name = Name;
            ((SequentialStartupOptions)StartupOptions).SetNodes(Nodes.Where(n => n.IsChecked).Select(n => n.Name).ToArray());
            StartupOptions.Connection = Connections.Profile?.Options;
        }

        private void Map(StartupOptions StartupOptions)
        {
            Name = StartupOptions.Name;
            Nodes = new ObservableCollection<NodeSelectorViewModel>(
                (StartupOptions.Nodes ?? Enumerable.Empty<string>())
                .Select(n => new NodeSelectorViewModel(n) { IsChecked = true }));

            Connections.Profile = Connections.List.FirstOrDefault(p => p.Options == StartupOptions.Connection);
        }

    }
}