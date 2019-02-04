using System;
using System.Threading.Tasks;

namespace Captura.Core.Models.UpdateCheckers
{
    public interface IUpdateChecker
    {
        void GoToDownloadsPage();

        Task<Version> Check();

        string BuildName { get; }
    }
}