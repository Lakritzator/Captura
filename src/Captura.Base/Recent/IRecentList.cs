using System;
using System.Collections.ObjectModel;

namespace Captura.Base.Recent
{
    public interface IRecentList : IDisposable
    {
        void Add(IRecentItem recentItem);

        ReadOnlyObservableCollection<IRecentItem> Items { get; }

        void Clear();
    }
}