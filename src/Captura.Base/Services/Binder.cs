using System;
using Ninject.Modules;

namespace Captura.Base.Services
{
    internal class Binder : NinjectModule, IBinder
    {
        static int _currentIndex;
        readonly int _index;
        readonly IModule _module;

        public Binder(IModule module)
        {
            _index = _currentIndex++;
            _module = module;
        }

        public void BindSingleton<T>()
        {
            Bind<T>().ToSelf().InSingletonScope();
        }

        public void Bind<TFrom, TTarget>(bool singleton = true) where TTarget : TFrom
        {
            var binding = Bind<TFrom>().To<TTarget>();

            if (singleton)
                binding.InSingletonScope();
        }

        public void Bind<T>(Func<T> function, bool singleton = true)
        {
            var binding = Bind<T>().ToMethod(context => function());

            if (singleton)
                binding.InSingletonScope();
        }

        public override void Load()
        {
            _module.OnLoad(this);
        }

        public override string Name => $"Module{_index}";
    }
}