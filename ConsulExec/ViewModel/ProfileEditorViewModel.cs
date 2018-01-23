using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using ConsulExec.Domain;
using Splat;

namespace ConsulExec.ViewModel
{
    public interface IProfileEditorViewModel<out T>
    {
        IProfileEditorViewModel<T> HandlingOk(Action<T> Handler);
        IProfileEditorViewModel<T> HandlingCancel(Action<T> Handler);
        IProfileEditorViewModel<T> HandlingDelete(Action<T> Handler);
    }


    public class ProfileEditorViewModel : ReactiveObject, IProfileEditorViewModel<ProfileViewModel<StartupOptions>>
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
            ProfileViewModel<StartupOptions> Options,
            IActivatingViewModel Activator,
            IObservable<string[]> NodesSource)
        {
            options = Options;
            activator = Activator;

            Map(Options.Options);

            okCommand = ReactiveCommand.Create(() =>
            {
                MapBack(Options.Options);
                Deactivate();
            }, this.WhenAnyValue(v => v.Name, s => !string.IsNullOrWhiteSpace(s)));

            cancelCommand = ReactiveCommand.Create(() =>
            {
                Map(Options.Options);
                Deactivate();
            });

            deleteCommand = ReactiveCommand.Create(Deactivate, canDelete);

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
        private readonly ReactiveCommand<Unit, Unit> deleteCommand;

        public IProfileEditorViewModel<ProfileViewModel<StartupOptions>> HandlingOk(Action<ProfileViewModel<StartupOptions>> Handler) =>
            AddHandler(okCommand, Handler);

        public IProfileEditorViewModel<ProfileViewModel<StartupOptions>> HandlingCancel(Action<ProfileViewModel<StartupOptions>> Handler) =>
            AddHandler(cancelCommand, Handler);

        public IProfileEditorViewModel<ProfileViewModel<StartupOptions>> HandlingDelete(Action<ProfileViewModel<StartupOptions>> Handler)
        {
            canDelete.OnNext(true);
            return AddHandler(deleteCommand, Handler);
        }

        private readonly ProfileViewModel<StartupOptions> options;
        private readonly IDisposable namesSubscription;
        private readonly BehaviorSubject<bool> canDelete = new BehaviorSubject<bool>(false);
        private readonly IActivatingViewModel activator;

        private void Map(StartupOptions Options)
        {
            Name = Options.Name;
            Nodes = new ObservableCollection<NodeSelectorViewModel>(
                (Options.Nodes ?? Enumerable.Empty<string>())
                    .Select(n => new NodeSelectorViewModel(n) { IsChecked = true }));
        }

        private void MapBack(StartupOptions Options)
        {
            Options.Name = Name;
            ((SequentialStartupOptions)Options).SetNodes(Nodes.Where(n => n.IsChecked).Select(n => n.Name).ToArray());
        }

        private ProfileEditorViewModel AddHandler(IObservable<Unit> Command, Action<ProfileViewModel<StartupOptions>> Handler)
        {
            Command.Subscribe(_ => Handler(options));
            return this;
        }

        private void Deactivate()
        {
            namesSubscription.Dispose();
            activator?.Deactivate(this);
        }
    }
}