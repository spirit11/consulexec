﻿using System;
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

        public IProfileEditorViewModel<T> HandlingDelete(Action<T> Handler)
        {
            canDelete.OnNext(true);
            return AddHandler(deleteCommand, Handler);
        }

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

        protected T Options { get; }

        protected IObservable<bool> IsValid
        {
            get { return isValid; }
            set { this.RaiseAndSetIfChanged(ref isValid, value); }
        }
        private IObservable<bool> isValid;

        protected virtual void OnDeactivate(bool Canceled)
        {

        }

        protected static void AssertNotNull<TArg>(TArg Argument, string ArgumentName)
            where TArg : class
        {
            if (Argument == null)
                throw new ArgumentNullException(ArgumentName);
        }

        private readonly BehaviorSubject<bool> canDelete = new BehaviorSubject<bool>(false);
        private readonly IActivatingViewModel activator;

        private BaseOptionsEditorViewModel<T> AddHandler(IObservable<Unit> Command, Action<T> Handler)
        {
            Command.Subscribe(_ => Handler(Options));
            return this;
        }

        private void Deactivate(bool canceled)
        {
            activator?.Deactivate(this);
            OnDeactivate(canceled);
        }
    }
}