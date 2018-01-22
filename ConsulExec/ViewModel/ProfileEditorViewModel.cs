using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ConsulExec.Domain;
using Splat;

namespace ConsulExec.ViewModel
{
    public class ProfileEditorViewModel : ReactiveObject
    {
        public ProfileEditorViewModel()
        {
            if (!ModeDetector.InDesignMode())
                throw new InvalidOperationException("Design only constructor");
            Name = "Some name";
            Nodes.Add(new NodeSelectorViewModel("Avail"));
            Nodes.Add(new NodeSelectorViewModel("Absent") { IsAbsent = true });
        }

        public ProfileEditorViewModel(
            ProfileViewModel Options,
            IActivatingViewModel Activator,
            IObservable<string[]> NodesSource,
            Action<ProfileViewModel> OnDelete = null)
        {
            options = Options;

            Map(Options.Options);

            okCommand = ReactiveCommand.Create(() =>
            {
                MapBack(Options.Options);
                Deactivate(Activator);
            }, this.WhenAnyValue(v => v.Name, s => !string.IsNullOrWhiteSpace(s)));

            cancelCommand = ReactiveCommand.Create(() =>
            {
                Map(Options.Options);
                Deactivate(Activator);
            });

            deleteCommand = ReactiveCommand.Create(() =>
            {
                Deactivate(Activator);
                OnDelete?.Invoke(Options);
            }, Observable.Return(OnDelete != null));

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

        public ObservableCollection<NodeSelectorViewModel> Nodes { get; private set; } = new ObservableCollection<NodeSelectorViewModel>();

        public string Name { get { return name; } set { this.RaiseAndSetIfChanged(ref name, value); } }
        private string name;

        public ICommand OkCommand => okCommand;
        private readonly ReactiveCommand<Unit, Unit> okCommand;

        public ICommand CancelCommand => cancelCommand;
        private readonly ReactiveCommand<Unit, Unit> cancelCommand;

        public ICommand DeleteCommand => deleteCommand;
        private readonly ReactiveCommand deleteCommand;

        public ProfileEditorViewModel HandlingOk(Action<ProfileViewModel> Handler) =>
            AddHandler(okCommand, Handler);

        public ProfileEditorViewModel HandlingCancel(Action<ProfileViewModel> Handler) =>
            AddHandler(cancelCommand, Handler);

        private readonly ProfileViewModel options;
        private readonly IDisposable namesSubscription;

        private void Map(SequentialStartupOptions Options)
        {
            Name = Options.Name;
            Nodes = new ObservableCollection<NodeSelectorViewModel>(
                (Options.Nodes ?? Enumerable.Empty<string>())
                    .Select(n => new NodeSelectorViewModel(n) { IsChecked = true }));
        }

        private void MapBack(SequentialStartupOptions Options)
        {
            Options.Name = Name;
            Options.SetNodes(Nodes.Where(n => n.IsChecked).Select(n => n.Name).ToArray());
        }

        private ProfileEditorViewModel AddHandler(IObservable<Unit> Command, Action<ProfileViewModel> Handler)
        {
            Command.Subscribe(_ => Handler(options));
            return this;
        }

        private void Deactivate(IActivatingViewModel Activator)
        {
            namesSubscription.Dispose();
            Activator?.Deactivate(this);
        }
    }
}