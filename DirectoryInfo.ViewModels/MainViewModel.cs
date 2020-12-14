using DirectoryInfo.Core;
using DirectoryInfo.Models;
using DirectoryInfo.Models.Contracts;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

namespace DirectoryInfo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        GetInfoService infoService;
        public MainViewModel()
        {
            this.GetInfoCommand = new RelayCommand(this.GetInfo, (o) => !this.IsScanning);
            this.SelectFolderCommand = new RelayCommand(this.SelectFolder);
            this.SelectOutputFileCommand = new RelayCommand(this.SelectOutputFile);
            this.StopCommand = new RelayCommand(this.StopProcesses);
            this.FileSystemItems = new ObservableCollection<IFileSystemItem>();
            this.infoService = new GetInfoService();
        }

        public bool IsScanning { get; set; }
        public string SelectedFolderPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public string OutputFilePath { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Output.xml");
        public string DirectoryTextInfo { get; private set; }
        public IRelayCommand GetInfoCommand { get; private set; }
        public IRelayCommand StopCommand { get; private set; }
        public IRelayCommand SelectFolderCommand { get; private set; }
        public IRelayCommand SelectOutputFileCommand { get; private set; }

        public ObservableCollection<IFileSystemItem> FileSystemItems { get; set; }

        private void GetInfo()
        {
            FileSystemItems.Clear();
            var rootItem = new FileSystemItem() { Path  = SelectedFolderPath, Type = ItemType.Folder};

            FileSystemItems.Add(new FileSystemItemHeader());
            FileSystemItems.Add(rootItem);

            IsScanning = true;
            Notify(() => IsScanning);
            this.infoService.StartGettingInfo(rootItem, () => ScanCompleted(), OutputFilePath);
            this.GetInfoCommand.UpdateCanExecuteState();
        }

        private void StopProcesses()
        {
            this.infoService.StopGettingInfo();
        }

        private void ScanCompleted()
        {
            IsScanning = false;
            Notify(() => IsScanning);
            this.GetInfoCommand.UpdateCanExecuteState();
        }

        private void SelectFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SelectedFolderPath = fbd.SelectedPath;
                    Notify(() => this.SelectedFolderPath);
                }
            }
        }

        private void SelectOutputFile()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "XML (*.xml)|*.xml";
                var fInfo = new FileInfo(OutputFilePath);
                sfd.InitialDirectory = fInfo.Directory.FullName;
                sfd.FileName = fInfo.Name;
                
                DialogResult result = sfd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
                {
                    OutputFilePath = Path.Combine(sfd.InitialDirectory, sfd.FileName);
                    Notify(() => this.OutputFilePath);
                }
            }
        }
    }
}
