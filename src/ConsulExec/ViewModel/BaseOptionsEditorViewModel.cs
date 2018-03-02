using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public abstract class BaseOptionsEditorViewModel<T> : ReactiveObject, IProfileEditorViewModel<T>
    {
        public ICommand OkCommand => okCommand;
        private readonly ReactiveCommand<Unit, Unit> okCommand;

        public ICommand CancelCommand => cancelCommand;
        private readonly ReactiveCommand<Unit, Unit> cancelCommand;

        public ICommand DeleteCommand => deleteCommand;
        private readonly ReactiveCommand<Unit, Unit> deleteCommand;

        public IProfileEditorViewModel<T> HandlingOk(Action<T> Handler) =>
            AddHandler(okCommand, Handler);

        public IProfileEditorViewModel<T> HandlingCancel(Action<T> Handler) =>
            AddHandler(cancelCommand, Handler);

        public IProfileEditorViewModel<T> HandlingDelete(Action<T> Handler, IObservable<bool> CanDelete = null)
        {
            IObservable<bool> src = new BehaviorSubject<bool>(true);
            if (canDeleteSubscription != null)
                throw new InvalidOperationException("HandlingDelete already set up");
            canDeleteSubscription = (CanDelete ?? src).Subscribe(canDelete);

            return AddHandler(deleteCommand, Handler);
        }

        protected T Options { get; }

        protected BaseOptionsEditorViewModel(T Options, IActivatingViewModel Activator)
        {
            this.Options = Options;
            activator = Activator;

            okCommand = ReactiveCommand.Create(() =>
            {
                Deactivate(false);
            }, this.WhenAnyObservable(v => v.IsValid));

            cancelCommand = ReactiveCommand.Create(() =>
            {
                Deactivate(true);
            });

            deleteCommand = ReactiveCommand.Create(() => Deactivate(true), canDelete);
        }

        protected IObservable<bool> IsValid
        {
            get { return isValid; }
            set { this.RaiseAndSetIfChanged(ref isValid, value); }
        }
        private IObservable<bool> isValid;

        protected virtual void OnDeactivate(bool Canceled)
        {
            canDeleteSubscription?.Dispose();
        }

        protected static void AssertNotNull<TArg>(TArg Argument, string ArgumentName)
            where TArg : class
        {
            if (Argument == null)
                throw new ArgumentNullException(ArgumentName);
        }

        private readonly BehaviorSubject<bool> canDelete = new BehaviorSubject<bool>(false);
        private readonly IActivatingViewModel activator;
        private IDisposable canDeleteSubscription;

        private BaseOptionsEditorViewModel<T> AddHandler(IObservable<Unit> Command, Action<T> Handler)
        {
            Command.Subscribe(_ => Handler(Options));
            return this;
        }

        private void Deactivate(bool Canceled)
        {
            activator?.Deactivate(this);
            OnDeactivate(Canceled);
        }
    }
}