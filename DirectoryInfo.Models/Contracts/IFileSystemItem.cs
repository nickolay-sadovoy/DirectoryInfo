namespace DirectoryInfo.Models.Contracts
{
    public interface IFileSystemItem
    {
        ItemType Type { get; set; }
        void NotifyItemChanged();

    }
}
