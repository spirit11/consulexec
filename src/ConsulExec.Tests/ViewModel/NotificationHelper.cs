using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ConsulExec.Tests.ViewModel
{
    public class NotificationHelper<T>
        where T : EventArgs
    {
        public NotificationHelper(object Target, string EventName)
        {
            observable = Observable.FromEventPattern<T>(Target, EventName);
        }

        public ReadOnlyCollection<T> Args { get; private set; }
        public ReadOnlyCollection<object> Senders { get; set; }

        public IDisposable Run()
        {
            var completionTrigger = new Subject<bool>();
            var o = observable.TakeUntil(completionTrigger).Replay();
            o.Connect();
            return Disposable.Create(() =>
            {
                completionTrigger.OnNext(true);
                var eventPatterns = o.ToList().Wait();
                Args = new ReadOnlyCollection<T>(eventPatterns.Select(ep => ep.EventArgs).ToList());
                Senders = new ReadOnlyCollection<object>(eventPatterns.Select(ep => ep.Sender).ToList());
            });
        }

        private readonly IObservable<EventPattern<T>> observable;
    }
}