using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public class UndoListViewModel : ReactiveObject
    {
        public UndoListViewModel()
        {
            UndoCommand = ReactiveCommand.Create(Undo, notifyListHasItems);
        }

        public ICommand UndoCommand { get; }

        public IDisposable Push(Action Undo)
        {
            undoActions.Add(Undo);
            notifyListHasItems.OnNext(true);
            return Disposable.Create(() => DropUndoUntil(undoActions.Count - 1));
        }

        public void Undo()
        {
            var a = undoActions.LastOrDefault();
            a?.Invoke();
            undoActions.Remove(a);
            notifyListHasItems.OnNext(undoActions.Any());
        }

        private readonly List<Action> undoActions = new List<Action>();
        private readonly BehaviorSubject<bool> notifyListHasItems = new BehaviorSubject<bool>(false);

        private void DropUndoUntil(int Index)
        {
            undoActions.RemoveRange(Index, undoActions.Count - Index);
            notifyListHasItems.OnNext(undoActions.Any());
        }
    }
}