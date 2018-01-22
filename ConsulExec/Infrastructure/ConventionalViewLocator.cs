using System;
using ReactiveUI;
using Splat;

namespace ConsulExec.Infrastructure
{
    public class ConventionalViewLocator : IViewLocator
    {
        public IViewFor ResolveView<T>(T ViewModel, string Contract = null) where T : class
        {
            var viewModelName = ViewModel.GetType().FullName;
            var viewTypeName = viewModelName.TrimEnd("Model".ToCharArray()).Replace(".ViewModel.", ".View.");

            try
            {
                var viewType = Type.GetType(viewTypeName);
                if (viewType == null)
                {
                    this.Log().Error($"Could not find the view {viewTypeName} for view model {viewModelName}.");
                    return null;
                }
                return Activator.CreateInstance(viewType) as IViewFor;
            }
            catch (Exception)
            {
                this.Log().Error($"Could not instantiate view {viewTypeName}.");
                throw;
            }
        }
    }
}