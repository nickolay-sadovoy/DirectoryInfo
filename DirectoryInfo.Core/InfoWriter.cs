using DirectoryInfo.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Xml;

namespace DirectoryInfo.Core
{
    internal class InfoWriter
    {
        private EventWaitHandle itemUpdated;
        private EventWaitHandle itemWritten;
        XmlDocument Document;
        XmlElement Root;

        Dictionary<Guid, XmlElement> Folders;

        public InfoWriter(EventWaitHandle itemUpdated, EventWaitHandle itemWritten)
        {
            this.itemUpdated = itemUpdated;
            this.itemWritten = itemWritten;
        }

        internal void InitWriter()
        {
            Folders = new Dictionary<Guid, XmlElement>();
            Document = new XmlDocument();
            Root = Document.CreateElement("DirectoryInfo");
            Document.AppendChild(Root);
        }

        internal void WriteInfo(Action<Action<FileSystemItem>> action)
        {
            this.itemUpdated.WaitOne();

            action((fileSystemItem) =>
            {
                if (fileSystemItem == null)
                    return;

                if (Folders.ContainsKey(fileSystemItem.Id))
                {
                    UpdateElementInfo(Folders[fileSystemItem.Id], fileSystemItem);
                }
                else
                {
                    var newElement = GetXmlElementFromItemInfo(fileSystemItem);
                    Root.AppendChild(newElement);
                }
            });

            this.itemWritten.Set();
        }

        internal void SaveDocument(string outputPath)
        {
            Document.Save(outputPath);
        }

        private void UpdateElementInfo(XmlElement XmlElement, FileSystemItem fileSystemItem)
        {
            XmlElement.RemoveAllAttributes();
            SetAttributeProperty(() => fileSystemItem.Name);
            SetAttributeProperty(() => fileSystemItem.DateAccessed);
            SetAttributeProperty(() => fileSystemItem.DateCreated);
            SetAttributeProperty(() => fileSystemItem.DateModified);
            SetAttributeProperty(() => fileSystemItem.Owner);
            SetAttributeProperty(() => fileSystemItem.Path);
            SetAttributeProperty(() => fileSystemItem.Permission);
            SetAttributeProperty(() => fileSystemItem.Size);
            SetAttributeProperty(() => fileSystemItem.Type);

            if (XmlElement.ChildNodes.Count == 0)
                AddChildrenFromItemInfo(XmlElement, fileSystemItem);

            void SetAttributeProperty<T>(Expression<Func<T>> selector) 
            {
                var memberExpression = selector.Body as MemberExpression;
                if (memberExpression == null) return;
                var value = selector.Compile().Invoke();
                XmlElement.SetAttribute(memberExpression.Member.Name, value.ToString());
            }
        }

        private XmlElement GetXmlElementFromItemInfo(FileSystemItem fileSystemItem)
        {
            var newElement = Document.CreateElement(fileSystemItem.Type.ToString());
            UpdateElementInfo(newElement, fileSystemItem);

            AddChildrenFromItemInfo(newElement, fileSystemItem);

            if (fileSystemItem.Type == ItemType.Folder)
            {
                Folders.Add(fileSystemItem.Id, newElement);
            }

            return newElement;
        }

        private void AddChildrenFromItemInfo(XmlElement XmlElement, FileSystemItem fileSystemItem)
        {
            foreach (var item in fileSystemItem.Items)
            {
                if (!Folders.ContainsKey(item.Id))
                    XmlElement.AppendChild(GetXmlElementFromItemInfo(item));
                else
                    UpdateElementInfo(Folders[item.Id], item);
            }

        }
    }
}
