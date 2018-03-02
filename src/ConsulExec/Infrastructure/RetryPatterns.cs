using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsulExec.Infrastructure
{
    public static class RetryPatterns
    {
        /// <summary>
        /// Delegate returning <code>true</code> to restart sequence or task, <code>false</code> otherwise
        /// </summary>
        /// <param name="ObservedException">Exception raised by decorated task or sequence</param>
        /// <param name="CancellationToken">Cancelation token to cancel async tasks inside delegate when decorated result is canceled or unsubsribed</param>
        /// <returns><code>true</code> to restart sequence or task, <code>false</code> otherwise</returns>
        public delegate Task<bool> RetryCallbackDelegate(Exception ObservedException, CancellationToken CancellationToken);


        /// <summary>
        /// Repeates subscription on <paramref name="Source"/> exceptions with given <paramref name="Delay"/>
        /// </summary>
        public static IObservable<T> Retry<T>(this IObservable<T> Source, TimeSpan Delay) =>
            Retry(Source, DelayRetryCallback(Delay));

        /// <summary>
        /// Repeates subscription on <paramref name="Source"/> exceptions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source">Original sequence</param>
        /// <param name="RetryCallback">Async callback returning <code>true</code> to continue or <code>false</code> to stop retry cycle</param>
        /// <returns>Decorated sequence</returns>
        public static IObservable<T> Retry<T>(this IObservable<T> Source, RetryCallbackDelegate RetryCallback) =>
            Observable.Create<T>(async (o, ct) =>
            {
                var l = new object();
                TaskCompletionSource<Unit> tcs = null;
                var ctReg = ct.Register(() =>
                  {
                      lock (l)
                          tcs?.SetCanceled();
                  });
                while (!ct.IsCancellationRequested)
                    try
                    {
                        tcs = new TaskCompletionSource<Unit>();
                        Source.Subscribe(o.OnNext, ex =>
                        {
                            lock (l)
                            {
                                ctReg.Dispose();
                                tcs.SetException(ex);
                            }
                        }, () =>
                        {
                            lock (l)
                            {
                                ctReg.Dispose();
                                tcs.SetResult(Unit.Default);
                            }
                            o.OnCompleted();
                        }, ct);
                        await tcs.Task;
                        break;
                    }
                    catch (TaskCanceledException)
                    {
                        LogLine("Canceled.");
                        throw;
                    }
                    catch (Exception e)
                    {
                        LogLine(e.Message);
                        var retry = await RetryCallback(e, ct);
                        if (!retry)
                        {
                            o.OnError(e);
                            break;
                        }
                    }
            });

        /// <summary>
        /// Retry to restart task until successful completion
        /// </summary>
        /// <param name="Func">Async function</param>
        /// <param name="CancellationToken">Token to cancel delay</param>
        /// <param name="Delay">Delay before function restart</param>
        /// <returns>Awaitable Task</returns>
        public static async Task Retry(this Func<Task> Func, CancellationToken CancellationToken, TimeSpan Delay) =>
            await Retry(Func.ReturnUnit(), CancellationToken, Delay);

        /// <summary>
        /// Retry to restart task until successful completion
        /// </summary>
        /// <param name="Func">Async function</param>
        /// <param name="CancellationToken">Token to cancel delay</param>
        /// <param name="Delay">Delay before function restart</param>
        /// <returns>Awaitable Task</returns>
        public static async Task<T> Retry<T>(this Func<Task<T>> Func, CancellationToken CancellationToken, TimeSpan Delay) =>
            await Func.Retry(CancellationToken, DelayRetryCallback(Delay));

        /// <summary>
        /// Retry to restart task until successful completion
        /// </summary>
        /// <param name="Func">Async function</param>
        /// <param name="CancellationToken">Token to cancel delay</param>
        /// <param name="RetryCallback">Async callback returning <code>true</code> to continue or <code>false</code> to stop retry cycle</param>
        /// <returns>Awaitable Task</returns>
        public static async Task Retry(this Func<Task> Func, CancellationToken CancellationToken,
            RetryCallbackDelegate RetryCallback)
            => await Retry(Func.ReturnUnit(), CancellationToken, RetryCallback);

        /// <summary>
        /// Retry to restart task until successful completion with custom retry callback
        /// </summary>
        /// <typeparam name="T">Async function result type</typeparam>
        /// <param name="Func">Async function</param>
        /// <param name="CancellationToken">Token to cancel callback</param>
        /// <param name="RetryCallback">Async callback returning <code>true</code> to continue or <code>false</code> to stop retry cycle</param>
        /// <returns></returns>
        public static async Task<T> Retry<T>(this Func<Task<T>> Func, CancellationToken CancellationToken, RetryCallbackDelegate RetryCallback)
        {
            while (true)
            {
                try
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    var r = await Func();
                    return r;
                }
                catch (OperationCanceledException)
                {
                    LogLine("cancel retry");
                    throw;
                }
                catch (Exception e)
                {
                    LogLine("failed");
                    bool retry;
                    try
                    {
                        retry = await RetryCallback(e, CancellationToken);
                    }
                    catch (Exception callbackException)
                    {
                        throw new AggregateException(e, callbackException);
                    }
                    if (!retry)
                        throw;
                }
            }
        }

        private static RetryCallbackDelegate DelayRetryCallback(TimeSpan Delay) =>
            (ex, ct) => Task.Delay(Delay, ct).ContinueWith(_ => true, ct);

        [Conditional("DEBUG")]
        private static void LogLine(string Message) => Debug.WriteLine(Message);
    }
}
