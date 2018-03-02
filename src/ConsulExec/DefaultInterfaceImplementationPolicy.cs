using System;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace ConsulExec
{
    internal class DefaultInterfaceImplementationPolicy : IFamilyPolicy
    {
        public PluginFamily Build(Type Type)
        {
            var typeName = Type.FullName;
            var nameIndex = Math.Max(typeName.LastIndexOf('.'), typeName.LastIndexOf('+')) + 1;

            var t = Type.GetType(typeName.Remove(nameIndex, 1)); // removing 'I' Namespace.IFoo => Namespace.Foo
            if (t != null)
            {
                var pluginFamily = new PluginFamily(Type);
                pluginFamily.SetDefault(new ConstructorInstance(t));
                return pluginFamily;
            }

            return null;
        }

        public bool AppliesToHasFamilyChecks => false;
    }
}