using DirectoryInfo.Models;
using System;
using System.Threading;
using System.Windows.Threading;

namespace DirectoryInfo.Core
{

    public class GetInfoService
    {
        DirectoryScaner Scaner;
        UICollectionUpdater CollectionUpdater;
        InfoWriter InformationWriter;

        object lockCurentItem = new object();
        FileSystemItem currentItem = null;

        bool ScanerCompleted;
        EventWaitHandle ItemScaned = new EventWaitHandle(false, EventResetMode.AutoReset);
        EventWaitHandle ItemUpdated = new EventWaitHandle(false, EventResetMode.AutoReset);
        EventWaitHandle ItemWritten = new EventWaitHandle(false, EventResetMode.AutoReset);

        public GetInfoService()
        {
            Scaner = new DirectoryScaner(ItemScaned,ItemWritten);
            CollectionUpdater = new UICollectionUpdater(ItemScaned, ItemUpdated);
            InformationWriter = new InfoWriter(ItemUpdated, ItemWritten);
        }

        public void StartGettingInfo(FileSystemItem rootItem, Action callBack, string outputFilePath)
        {
            //should be UI thread, but actuall it's parent thread
            var parentThread = Thread.CurrentThread;

            var ScanThread = new Thread(() =>
            {
                ScanerCompleted = false;
                Scaner.ScanDirectory(rootItem, ItemInfoUpdated);
                ScanerCompleted = true;
                ItemScaned.Set();
            });

            var CollectionUpdaterThread = new Thread(() =>
            {
                while (!ScanerCompleted)
                    CollectionUpdater.UpdateCollection(parentThread, UpdateItemInfo);

                ItemUpdated.Set();
            });

            ThreadStart infoWriterThreadStarter = () =>
            {
                InformationWriter.InitWriter();
                while (!ScanerCompleted)
                    InformationWriter.WriteInfo(WriteItemInfo);

                InformationWriter.SaveDocument(outputFilePath);
            };

            infoWriterThreadStarter += () => Dispatcher.FromThread(parentThread).Invoke(() => callBack());

            var InfoWriterThread = new Thread(infoWriterThreadStarter);

            ScanThread.Start();
            CollectionUpdaterThread.Start();
            InfoWriterThread.Start();
            
        }

        public void StopGettingInfo()
        {
            this.Scaner.StopScaning();
        }

        internal void ItemInfoUpdated(FileSystemItem newCurrentItem)
        {
            if (newCurrentItem != null)
                lock(lockCurentItem)
                {
                    currentItem = newCurrentItem;
                }
        }

        internal void UpdateItemInfo()
        {
            if (currentItem != null)
                lock (lockCurentItem)
                {
                    currentItem.NotifyItemChanged();
                }
        }

        internal void WriteItemInfo(Action<FileSystemItem> action)
        {
            if (currentItem != null)
                lock (lockCurentItem)
                {
                    action(currentItem);
                }
        }
    }
}
