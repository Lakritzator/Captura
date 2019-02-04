using System;

namespace Captura.Models
{
    public interface IRemoveRequester
    {
        event Action RemoveRequested;
    }
}