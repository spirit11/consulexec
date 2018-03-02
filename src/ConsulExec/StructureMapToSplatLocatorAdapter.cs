using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Splat;
using StructureMap;

namespace ConsulExec
{
    internal class StructureMapToSplatLocatorAdapter : IMutableDependencyResolver
    {
        private readonly Container locator;
        private readonly IDependencyResolver current;

        public StructureMapToSplatLocatorAdapter(Container Container, IDependencyResolver Current)
        {
            locator = Container;
            current = Current;
        }

        public void Dispose()
        {
            current.Dispose();
            locator.Dispose();
        }

        public object GetService(Type serviceType, string contract = null)
        {
            Debug.WriteLine("Getting " + serviceType);
            var v1 = contract == null
                ? locator.TryGetInstance(serviceType)
                : locator.TryGetInstance(serviceType, contract);
            return v1 ?? current.GetService(serviceType, contract);
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null)
        {
            if (!string.IsNullOrEmpty(contract))
                throw new NotSupportedException("contract should not be set");
            return locator.GetAllInstances(serviceType).Cast<object>();
        }

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            Debug.WriteLine(serviceType);
            locator.Configure(x => x.For(serviceType).Use(factory()));
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback)
        {
            throw new NotImplementedException();
        }
    }
}