using System;

namespace ConsulExec.ViewModel
{
    public interface IProfileEditorViewModel<out T>
    {
        IProfileEditorViewModel<T> HandlingOk(Action<T> Handler);
        IProfileEditorViewModel<T> HandlingCancel(Action<T> Handler);
        IProfileEditorViewModel<T> HandlingDelete(Action<T> Handler, IObservable<bool> CanDelete = null);
    }
}