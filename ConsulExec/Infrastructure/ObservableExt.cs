using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace ConsulExec.Infrastructure
{
    public static class ObservableExt
    {
        public static IObservable<bool> Heartbeat<T>(this IObservable<T> Source, TimeSpan Timeout,
            bool CompleteWithSource = true) =>
            HeartbeatImpl(Source, Source, Timeout, CompleteWithSource, DefaultScheduler.Instance);

        public static IObservable<bool> Heartbeat<T>(this IObservable<T> Source, TimeSpan Timeout,
            IScheduler Scheduler, bool CompleteWithSource = true) =>
            HeartbeatImpl(Source, Source, Timeout, CompleteWithSource, Scheduler);

        public static IObservable<bool> Heartbeat<T, TO>(this IObservable<T> Source, IObservable<TO> TimeoutSource,
            TimeSpan Timeout, IScheduler Scheduler, bool CompleteWithSource = true) =>
            HeartbeatImpl(Source, TimeoutSource, Timeout, CompleteWithSource, Scheduler ?? DefaultScheduler.Instance);

        public static IObservable<bool> Heartbeat<T, TO>(this IObservable<T> Source, IObservable<TO> TimeoutSource,
            TimeSpan Timeout, bool CompleteWithSource = true) =>
            HeartbeatImpl(Source, TimeoutSource, Timeout, CompleteWithSource, DefaultScheduler.Instance);

        /// <summary>
        /// New values from source are enqueued for late processing until some event derived from result,
        /// notifying that some long running task is completed.
        /// </summary>
        /// <typeparam name="TS">Source values data type</typeparam>
        /// <typeparam name="TR">Result values data type</typeparam>
        /// <typeparam name="TT">Result trigger data type</typeparam>
        /// <param name="Source">Data source</param>
        /// <param name="ResultSelector">Output data fabric</param>
        /// <param name="ContinuationSelector">Selector of output data property, informing of long running task completion</param>
        /// <returns></returns>
        public static IObservable<TR> Process<TS, TR, TT>(
            this IObservable<TS> Source,
            Func<TS, TR> ResultSelector,
            Func<TR, IObservable<TT>> ContinuationSelector)
        {
            return Observable.Create<TR>(o =>
            {
                var bc = new BlockingCollection<TS>();
                var d = Source.Finally(bc.CompleteAdding).Subscribe(bc.Add);

                var cts = new CancellationTokenSource();
                var ct = cts.Token;
                Task.Run(() =>
                {
                    foreach (var t in bc.GetConsumingEnumerable(ct))
                    {
                        var r = default(TR);
                        try
                        {
                            r = ResultSelector(t);
                            o.OnNext(r);
                            ContinuationSelector(r).Wait();
                        }
                        catch (Exception e)
                        {
                            o.OnError(e);
                            return;
                        }
                    }

                    o.OnCompleted();
                }, ct);

                return new CompositeDisposable(bc, d, new CancellationDisposable(cts));
            });
        }

        public static IObservable<TO> SelectNSwitch<TI, TO>(this IObservable<TI> Source, Func<TI, IObservable<TO>> Selector) =>
            Source.Select(Selector).Switch();

        private static IObservable<bool> HeartbeatImpl<T, TO>(this IObservable<T> Source, IObservable<TO> TimeoutSource,
            TimeSpan Timeout, bool CompleteWithSource, IScheduler Scheduler)
        {
            var timeout = TimeoutSource.Throttle(Timeout, Scheduler).Select(_ => false);
            var hasHeartbeat = timeout.Select(_ => false).Merge(Source.Select(_ => true)).DistinctUntilChanged();
            return CompleteWithSource
                ? hasHeartbeat.TakeUntil(TimeoutSource.LastAsync())
                : hasHeartbeat;
        }
    }
}