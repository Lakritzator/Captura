using System;

namespace Captura.Base.Services
{
    public interface IBinder
    {
        void BindSingleton<T>();
        void Bind<TFrom, TTarget>(bool singleton = true) where TTarget : TFrom;
        void Bind<T>(Func<T> function, bool singleton = true);
    }
}