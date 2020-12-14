using System;
using System.Threading;
using System.Windows.Threading;

namespace DirectoryInfo.Core
{
    internal class UICollectionUpdater
    {
        private EventWaitHandle itemScaned;
        private EventWaitHandle itemUpdated;

        public UICollectionUpdater(EventWaitHandle itemScaned, EventWaitHandle itemUpdated)
        {
            this.itemScaned = itemScaned;
            this.itemUpdated = itemUpdated;
        }

        internal void UpdateCollection(Thread parentThread, Action updateAction)
        {
            itemScaned.WaitOne();
            Dispatcher.FromThread(parentThread).Invoke(updateAction);
            itemUpdated.Set();
        }
    }
}
