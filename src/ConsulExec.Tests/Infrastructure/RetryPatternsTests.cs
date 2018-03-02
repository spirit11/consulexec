using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ConsulExec.Infrastructure;

namespace ConsulExec.Tests.Infrastructure
{
    [TestFixture]
    public class RetryPatternsObservableTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
            var cnt = 0;
            target = Observable.Create<int>(o =>
            {
                if (cnt == 5)
                {
                    o.OnNext(42);
                    o.OnCompleted();
                }
                else
                {
                    cnt++;
                    if (cnt == 1)
                        o.OnNext(41);
                    o.OnError(new Exception("42"));
                }

                return Disposable.Empty;
            });
        }

        [Test]
        [Timeout(1000)]
        public async Task RetryWithDelay()
        {
            var l = await target.Retry(TimeSpan.FromMilliseconds(10)).ToList();
            Expect(l, EqualTo(new[] { 41, 42 }));
        }

        [Test]
        public async Task RetryWithCallback()
        {
            var l = await target.Retry((ex, ct) => Task.FromResult(true)).ToList();
            Expect(l, EqualTo(new[] { 41, 42 }));
        }

        [Test]
        public async Task RetryWithInterruption()
        {
            var retryCount = 0;
            var l = await target.Retry((ex, ct) => Task.FromResult(++retryCount < 2)).Materialize().ToList();
            Expect(l.Where(v => v.HasValue).Select(v => v.Value), EqualTo(new[] { 41 }));
            Expect(l.Last().Exception, TypeOf<Exception>().With.Message.EqualTo("42"));
            Expect(retryCount, EqualTo(2));
        }

        [Test]
        [Timeout(1000)]
        public void RetryWithRetryCallbackCancelation()
        {
            var retryCount = 0;
            var l = new List<int>();
            var stuckInDelay = false;
            Exception exeption = null;
            var completed = false;

            var subscription = target.Retry(async (ex, ct) =>
              {
                  if (++retryCount == 2)
                  {
                      stuckInDelay = true;
                      await Task.Delay(10000, ct);
                  }
                  return await Task.FromResult(true);
              }).Subscribe(l.Add, ex => exeption = ex, () => completed = true);


            Expect(stuckInDelay, True.After(100));
            subscription.Dispose();
            Expect(l, EqualTo(new[] { 41 }));
            Expect(exeption, Null);
            Expect(completed, False);
        }

        private IObservable<int> target;
    }


    [TestFixture]
    public class RetryPatternsTaskTests : AssertionHelper
    {
        private int cnt;

        [SetUp]
        public void SetUp()
        {
            cnt = 0;
        }

        [Test]
        [Timeout(1000)]
        public async Task RetryWithDelay()
        {
            var result = await Target()
                .Retry(CancellationToken.None, TimeSpan.FromMilliseconds(10));

            Expect(result, EqualTo(42));
            Expect(cnt, EqualTo(5));
        }

        [Test]
        public async Task RetryWithCallback()
        {
            var result = await Target()
                .Retry(CancellationToken.None, (ex, ct) => Task.FromResult(true));

            Expect(result, EqualTo(42));
            Expect(cnt, EqualTo(5));
        }

        [Test]
        public void RetryWithInterruption()
        {
            var retryCount = 0;

            Expect(async () => await Target().Retry(CancellationToken.None, (ex, ct) => Task.FromResult(++retryCount < 2)),
                Throws.Exception.TypeOf<Exception>().With.Message.EqualTo("42"));

            Expect(retryCount, EqualTo(2));
        }


        [Test]
        [Timeout(1000)]
        public async Task RetryWithRetryCallbackCancelation()
        {
            var retryCount = 0;
            Task<int> target;

            using (var cts = new CancellationTokenSource())
            {
                var stuckInDelay = false;
                target = Target()
                    .Retry(cts.Token, async (ex, ct) =>
                    {
                        if (++retryCount == 2)
                        {
                            stuckInDelay = true;
                            await Task.Delay(10000, ct);
                        }

                        return await Task.FromResult(true);
                    });

                Expect(stuckInDelay, True.After(100));
                cts.Cancel();
            }

            try
            {
                await target;
                Assert.Fail("Exception expected");
            }
            catch (Exception e)
            {
                Expect(e, TypeOf<AggregateException>().With.Property("InnerExceptions").Count.EqualTo(2));
                Expect(e, Property("InnerExceptions").Some.Message.EqualTo("42"));
                Expect(e, Property("InnerExceptions").Some.TypeOf<TaskCanceledException>());
            }
        }

        [Test]
        [Timeout(1000)]
        public void RetryWithCancellation()
        {
            using (var cts = new CancellationTokenSource(100))
            {
                Func<Task> target = async () => await Task.Delay(TimeSpan.FromDays(1), cts.Token);
                Expect(async () => await target.Retry(cts.Token, TimeSpan.FromDays(1)), Throws.Exception.TypeOf<TaskCanceledException>());
            }
        }

        private Func<Task<int>> Target()
        {
            return () =>
            {
                if (cnt == 5)
                    return Task.FromResult(42);
                cnt++;
                throw new Exception("42");
            };
        }
    }
}
