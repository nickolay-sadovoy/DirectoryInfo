using DirectoryInfo.Models.Contracts;
using System;
using System.Threading;

namespace DirectoryInfo.Models
{
    public class FileSystemItem : NotifyPropertyChangedModel, IFileSystemItem
    {
        public FileSystemItem(SynchronizationContext synchronizationContext = null)
        {
            Id = Guid.NewGuid();
            this.Items = new AsyncObservableCollection<FileSystemItem>(synchronizationContext);
        }

        public Guid Id { get; }
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateAccessed { get; set; }
        public string Attributes { get; set; }
        public long Size { get; set; }
        public string Owner { get; set; }
        public string Permission { get; set; }
        public ItemType Type { get; set; }
        public bool IsSizeCalculated { get; set; } = false;
        public AsyncObservableCollection<FileSystemItem> Items { get; set; }

        public void NotifyItemChanged()
        {
            Notify(() => this.DateAccessed);
            Notify(() => this.DateModified);
            Notify(() => this.DateCreated);
            Notify(() => this.Size);
            Notify(() => this.IsSizeCalculated);
        }
    }
}
