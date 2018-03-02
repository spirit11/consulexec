using System;
using System.Collections.Generic;

namespace ConsulExec.Infrastructure
{
    public static class DictionaryExtensions
    {
        public static T GetOrAdd<TK, T>(this IDictionary<TK, T> Dictionary, TK Key)
            where T : new() => Dictionary.GetOrAdd(Key, _ => new T());

        public static T GetOrAdd<TK, T>(this IDictionary<TK, T> Dictionary, TK Key, Func<TK, T> Ctor)
        {
            T r;
            if (!Dictionary.TryGetValue(Key, out r))
                Dictionary[Key] = r = Ctor(Key);

            return r;
        }
    }
}
