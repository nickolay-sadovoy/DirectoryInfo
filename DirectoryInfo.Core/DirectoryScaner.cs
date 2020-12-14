using DirectoryInfo.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;

namespace DirectoryInfo.Core
{
    internal class DirectoryScaner
    {
        Type accouuntType = typeof(System.Security.Principal.NTAccount);
        Queue<FileSystemItem> ScanQueue = new Queue<FileSystemItem>();
        Stack<FileSystemItem> LoadingStack = new Stack<FileSystemItem>();
        private EventWaitHandle ItemScaned;
        private EventWaitHandle ItemWritten;
        private bool IsStoped;

        public DirectoryScaner(EventWaitHandle itemScaned, EventWaitHandle itemWritten)
        {
            this.ItemScaned = itemScaned;
            this.ItemWritten = itemWritten;
        }

        internal void ScanDirectory(FileSystemItem fileSystemItem, Action<FileSystemItem> ItemUpdateAction)
        {
            IsStoped = false;

            ScanQueue.Enqueue(fileSystemItem);
            LoadingStack.Push(fileSystemItem);
            FillItemInfo(fileSystemItem);

            while (ScanQueue.Count > 0 && !IsStoped)
            {
                var scanningDir = ScanQueue.Dequeue();
                ScanQueueDirectory(scanningDir);

                UpdateItem(scanningDir);
            }

            while (ScanQueue.Count > 0)
            {
                var scanningDir = ScanQueue.Dequeue();
                SkipItem(scanningDir);
            }

            while (LoadingStack.Count > 0 && !IsStoped)
            {
                var dir = LoadingStack.Pop();
                FillFolderSize(dir);

                UpdateItem(dir);
            }
            
            while (LoadingStack.Count > 0)
            {
                var dir = LoadingStack.Pop();
                SkipItem(dir);
            }

            void UpdateItem(FileSystemItem item)
            {
                ItemUpdateAction(item);
                ItemScaned.Set();
                ItemWritten.WaitOne();
            }
            
            void SkipItem(FileSystemItem item)
            {
                item.IsSizeCalculated = true;
                ItemScaned.Set();
                ItemWritten.WaitOne();
            }
        }
        internal void StopScaning()
        {
            IsStoped = true;
        }

        private void ScanQueueDirectory(FileSystemItem fileSystemItem)
        {
            var dirs = Directory.GetDirectories(fileSystemItem.Path).OrderBy(x => x);
            foreach (var dir in dirs)
            {
                var newDirItem = new FileSystemItem(fileSystemItem.Items.SynchronizationContext) { Path = dir, Name = Path.GetFileName(dir), Type = ItemType.Folder };
                FillItemInfo(newDirItem);
                fileSystemItem.Items.Add(newDirItem);
                ScanQueue.Enqueue(newDirItem);
                LoadingStack.Push(newDirItem);
            }

            var files = Directory.GetFiles(fileSystemItem.Path).OrderBy(x => x);
            foreach (var filePath in files)
            {
                var fileItemInfo = GetFileByPath(filePath, fileSystemItem.Items.SynchronizationContext);
                fileSystemItem.Items.Add(fileItemInfo);
            }

        }

        private FileSystemItem GetFileByPath(string filePath, SynchronizationContext synchronizationContext)
        {
            var item = new FileSystemItem(synchronizationContext)
            {
                Path = filePath,
                Type = ItemType.File 
            };

            FillItemInfo(item);

            return item;
        }

        private void FillItemInfo(FileSystemItem fileSystemItem)
        {
            var fileInfo = new FileInfo(fileSystemItem.Path);
            var fileAccessInfo = fileInfo.GetAccessControl();
            var owner = fileAccessInfo.GetOwner(accouuntType).ToString();
            var currentUser = $"{(string.IsNullOrEmpty(Environment.UserDomainName) ? string.Empty : $"{Environment.UserDomainName}\\" )}{Environment.UserName}";
            var permissions = string.Empty;
            var currentUserRule= fileAccessInfo.GetAccessRules(true, true, accouuntType).Cast<FileSystemAccessRule>().FirstOrDefault(rule => rule.IdentityReference.Value.Equals(currentUser));
            
            if(currentUserRule != null)
            {
                permissions += currentUserRule.FileSystemRights.ToString() + " : ";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl) ? "f" : "-";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write) ? "w" : "-";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read) ? "r" : "-";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.AppendData) == FileSystemRights.AppendData) ? "a" : "-";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify) ? "m" : "-";
                permissions += ((currentUserRule.FileSystemRights & FileSystemRights.ExecuteFile) == FileSystemRights.ExecuteFile) ? "e" : "-";
            }

            fileSystemItem.Attributes = fileInfo.Attributes.ToString();
            fileSystemItem.DateAccessed = fileInfo.LastAccessTime;
            fileSystemItem.DateCreated = fileInfo.CreationTime;
            fileSystemItem.DateModified = fileInfo.LastWriteTime;
            fileSystemItem.Name = string.IsNullOrEmpty(fileSystemItem.Name) ? fileInfo.Name : fileSystemItem.Name;
            if (fileSystemItem.Type == ItemType.File)
            {
                fileSystemItem.Size = fileInfo.Length;
                fileSystemItem.IsSizeCalculated = true;
            }
            fileSystemItem.Owner = owner;
            fileSystemItem.Permission = permissions;
        }

        private void FillFolderSize(FileSystemItem fileSystemItem)
        {
            fileSystemItem.Size = fileSystemItem.Items.Select(x => x.Size).Sum();
            fileSystemItem.IsSizeCalculated = true;
        }

        private string GetCurrentUserPermissions(string scanPath)
        {
            string permissionShort = string.Empty;
            DirectorySecurity dSecurity = Directory.GetAccessControl(scanPath);
            

            return permissionShort;
        }

    }
}
