using System;

namespace Captura.Base.Services
{
    public interface IModule : IDisposable
    {
        void OnLoad(IBinder binder);
    }
}