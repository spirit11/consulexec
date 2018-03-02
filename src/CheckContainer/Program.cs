using System;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Modules;
using Ninject.Syntax;
using StructureMap;
using StructureMap.Graph;

namespace CheckContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Container(x =>
            {
                x.Scan(_ =>
                {
                    _.AssembliesFromApplicationBaseDirectory();
                    _.AddAllTypesOf<IExt>();
                });
                x.For<IExt>().Use<Ext1>();
            });
            foreach (var ext in c.GetInstance<ExtFabric>().AllExt)
            {
                Console.WriteLine(ext.Name);
            }

            Console.WriteLine(c.GetInstance<UseExt>().Ext.Value.GetType());

            var kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            kernel.Bind<IExt>().To<Ext1>();
            var res = (IResolutionRoot)kernel;

            foreach (var ext in res.Get<ExtFabric>().AllExt)
            {
                Console.WriteLine(ext.Name);
            }
            Console.WriteLine(res.Get<UseExt>().Ext.Value.GetType());

            Console.ReadLine();
        }
    }

    public class Mod : NinjectModule
    {
        public override void Load()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IExt))))
                Bind<IExt>().To(type);
        }
    }


    class Ext<T>
    {
        public Ext(T obj)
        {

        }
    }

    class UseExt
    {
        public Lazy<IExt> Ext { get; }

        public UseExt(Lazy<IExt> IExt)
        {
            Ext = IExt;
        }
    }

    interface IExt
    {
        string Name { get; }
    }

    public class Ext1 : IExt
    {
        public string Name => "e1";
    }

    public class Ext2 : IExt
    {
        public string Name => "e2";
    }

    class ExtFabric
    {
        public IExt[] AllExt { get; }

        public ExtFabric(IExt[] AllExt)
        {
            this.AllExt = AllExt;
        }
    }
}
