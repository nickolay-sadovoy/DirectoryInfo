using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace DirectoryInfo.Models
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        public SynchronizationContext SynchronizationContext { get; }

        public AsyncObservableCollection(SynchronizationContext synchronizationContext = null)
        {
            SynchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
        }

        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == SynchronizationContext)
            {
                RaiseCollectionChanged(e);
            }
            else
            {
                SynchronizationContext.Send(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == SynchronizationContext)
            {
                RaisePropertyChanged(e);
            }
            else
            {
                SynchronizationContext.Send(RaisePropertyChanged, e);
            }
        }

        private void RaisePropertyChanged(object param)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }
}
