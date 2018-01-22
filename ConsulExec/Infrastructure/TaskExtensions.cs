using System;
using System.Reactive;
using System.Threading.Tasks;

namespace ConsulExec.Infrastructure
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Transform async function with no result to function returning Unit
        /// </summary>
        /// <param name="Func"></param>
        /// <returns></returns>
        public static Func<Task<Unit>> ReturnUnit(this Func<Task> Func) => async () =>
        {
            await Func();
            return Unit.Default;
        };
    }
}