using DirectoryInfo.Models.Contracts;

namespace DirectoryInfo.Models
{
    public class FileSystemItemHeader : IFileSystemItem
    {
        public ItemType Type { get; set; } = ItemType.Header;

        public void NotifyItemChanged()
        {
        }
    }
}
