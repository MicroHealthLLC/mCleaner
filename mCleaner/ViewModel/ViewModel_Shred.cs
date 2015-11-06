using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using mCleaner.Model;
using Microsoft.Practices.ServiceLocation;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace mCleaner.ViewModel
{
    public class ViewModel_Shred : ViewModelBase
    {
        private string _windowTitle = string.Empty;

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                if (_windowTitle != value)
                {
                    _windowTitle = value;
                    base.RaisePropertyChanged("WindowTitle");
                }
            }
        }

        public ViewModel_CleanerML CleanerML
        {
            get { return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>(); }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get { return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>(); }
        }

        private string _strDefaultScanLocation = string.Empty;

        public string strDefaultScanLocation
        {
            get { return _strDefaultScanLocation; }
            set
            {
                if (_strDefaultScanLocation != value)
                {
                    _strDefaultScanLocation = value;
                    base.RaisePropertyChanged("strDefaultScanLocation");
                }
            }
        }

        private string _strSelectRecycleBinText = "Select Recycle Bin";

        public string strSelectRecycleBinText
        {
            get { return _strSelectRecycleBinText; }
            set
            {
                if (_strSelectRecycleBinText != value)
                {
                    _strSelectRecycleBinText = value;
                    base.RaisePropertyChanged("strSelectRecycleBinText");
                }
            }
        }




        private ObservableCollection<Model_Shred> _shredFilesCollection = new ObservableCollection<Model_Shred>();

        public ObservableCollection<Model_Shred> ShredFilesCollection
        {
            get { return _shredFilesCollection; }
            set
            {
                if (_shredFilesCollection != value)
                {
                    _shredFilesCollection = value;
                    base.RaisePropertyChanged("ShredFilesCollection");
                }
            }
        }

        private Model_Shred _SelectedShredFile = null;

        public Model_Shred SelectedShredFile
        {
            get { return _SelectedShredFile; }
            set
            {
                if (_SelectedShredFile != value)
                {
                    _SelectedShredFile = value;
                    base.RaisePropertyChanged("SelectedShredFile");
                }
                if (_SelectedShredFile != null)
                    btnIsListViewItemSelected = true;
                else
                    btnIsListViewItemSelected = false;
            }
        }

        private int _progressMax = 0;

        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                if (_progressMax != value)
                {
                    _progressMax = value;
                    base.RaisePropertyChanged("ProgressMax");
                }
            }
        }

        private bool _btnCloseEnable = true;

        public bool btnCloseEnable
        {
            get { return _btnCloseEnable; }
            set
            {
                if (_btnCloseEnable != value)
                {
                    _btnCloseEnable = value;
                    base.RaisePropertyChanged("btnCloseEnable");
                }
            }
        }

        private bool _btnIsListViewItemSelected = false;

        public bool btnIsListViewItemSelected
        {
            get { return _btnIsListViewItemSelected; }
            set
            {
                if (_btnIsListViewItemSelected != value)
                {
                    _btnIsListViewItemSelected = value;
                    base.RaisePropertyChanged("btnIsListViewItemSelected");
                }
            }
        }

        private bool _btnShreddingEnable = false;

        public bool btnShreddingEnable
        {
            get { return _btnShreddingEnable; }
            set
            {
                if (_btnShreddingEnable != value)
                {
                    _btnShreddingEnable = value;
                    base.RaisePropertyChanged("btnShreddingEnable");
                }
            }
        }


        private bool _btnShredRecycleBinEnable = false;

        public bool btnShredRecycleBinEnable
        {
            get { return _btnShredRecycleBinEnable; }
            set
            {
                if (_btnShredRecycleBinEnable != value)
                {
                    _btnShredRecycleBinEnable = value;
                    base.RaisePropertyChanged("btnShredRecycleBinEnable");
                }
            }
        }

        private int _progressIndex = 0;

        public int ProgressIndex
        {
            get { return _progressIndex; }
            set
            {
                if (_progressIndex != value)
                {
                    _progressIndex = value;
                    base.RaisePropertyChanged("ProgressIndex");
                }
            }
        }

        private string _progressText = string.Empty;

        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    base.RaisePropertyChanged("ProgressText");
                }
            }
        }

        private string _log = string.Empty;

        public string Log
        {
            get { return _log; }
            set
            {
                if (_log != value)
                {
                    _log = value;
                    base.RaisePropertyChanged("Log");
                }
            }
        }

        private bool _showWindow = false;

        public bool ShowWindow
        {
            get { return _showWindow; }
            set
            {
                if (_showWindow != value)
                {
                    _showWindow = value;
                    base.RaisePropertyChanged("ShowWindow");
                }
            }
        }

        public ICommand Command_ShredStart { get; set; }
        public ICommand Command_SelectFiles { get; set; }
        public ICommand Command_RemoveFolder { get; set; }
        public ICommand Command_SelectFolders { get; set; }
        public ICommand Command_ShredRecycleBin { get; set; }
        public ICommand Command_ShowWindow { get; set; }
        public ICommand Command_CloseWindow { get; set; }

        public ViewModel_Shred()
        {
            this.WindowTitle = "Shred File & Folder";

            if (base.IsInDesignMode)
            {
                this.Log = "Starting to shred\r\nC:\\Windows\\win.com";
            }
            else
            {
                Command_ShredStart = new RelayCommand(() => Command_ShredStart_Click());
                Command_ShowWindow = new RelayCommand(Command_ShowWindow_Click);
                Command_SelectFiles = new RelayCommand(Command_SelectFiles_Click);
                Command_RemoveFolder = new RelayCommand(Command_RemoveFolder_Click);
                Command_SelectFolders = new RelayCommand(Command_SelectFolder_Click);
                Command_ShredRecycleBin = new RelayCommand(Command_ShredRecycleBin_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
            }
        }




        public async Task<bool> Command_ShredStart_Click()
        {
            btnCloseEnable = false;
            this.ProgressMax = ShredFilesCollection.Count;
            this.ProgressIndex = 0;
            this.ProgressText = "Started to Shred Please Wait.";
            await Task.Run(() => StartShredding());
            btnCloseEnable = true;
            this.ShredFilesCollection.Clear();
            return true;
        }



        public void Command_RemoveFolder_Click()
        {
            if (SelectedShredFile != null)
            {
                ShredFilesCollection.Remove(SelectedShredFile);
                if (ShredFilesCollection.Count <= 0)
                    btnShreddingEnable = false;
            }
        }

        public void Command_SelectFiles_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "All files|*.*",
                Title = "Select a single\\multiple file to shred",
                Multiselect = true
            };
            {
                ofd.ShowDialog();
                if (ofd.FileName != string.Empty || ofd.FileNames.Length > 0)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        if (!this.ShredFilesCollection.Where(dc => dc.FilePath == file).Any())
                        {
                            this.ShredFilesCollection.Add(new Model_Shred() {FilePath = file});
                            btnShreddingEnable = true;
                        }
                    }

                }
            }
        }

        public void Command_SelectFolder_Click()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                if (!this.ShredFilesCollection.Where(dc => dc.FilePath == fbd.SelectedPath).Any())
                {
                    this.ShredFilesCollection.Add(new Model_Shred() {FilePath = fbd.SelectedPath});
                    btnShreddingEnable = true;
                }

            }
        }


        public void Command_ShredRecycleBin_Click()
        {
            btnShredRecycleBinEnable = !btnShredRecycleBinEnable;
            strSelectRecycleBinText = btnShredRecycleBinEnable ? "Remove Recycle Bin" : "Select Recycle Bin";
            btnShreddingEnable = true;
            if (btnShredRecycleBinEnable)
            {
                this.ShredFilesCollection.Add(new Model_Shred() {FilePath = "Recycle Bin"});

            }
            else
            {
                var collectionShredPath = this.ShredFilesCollection.Where(dc => dc.FilePath == "Recycle Bin");
                if (collectionShredPath.Any())
                {
                    var mdlSharedPath = collectionShredPath.First();
                    this.ShredFilesCollection.Remove(mdlSharedPath);

                }
            }

            if (ShredFilesCollection.Count == 0)
                btnShreddingEnable = false;
        }

        public void Command_ShowWindow_Click()
        {
            CleanerML.Run = false;
            CleanerML.ShowCleanerDescription = false;
            DupChecker.ShowWindow = false;
            CleanerML.ShowFrontPage = false;
            CleanerML.btnCleanNowPreviousState = CleanerML.btnPreviewCleanEnable;
            CleanerML.btnPreviewCleanEnable = false;
            CleanerML.btnCleaningOptionsEnable = false;
            this.ShowWindow = true;
        }

        public void Command_CloseWindow_Click()
        {
            CleanerML.Run = false;
            CleanerML.ShowFrontPage = true;
            CleanerML.ShowCleanerDescription = false;
            CleanerML.btnPreviewCleanEnable = CleanerML.btnCleanNowPreviousState;
            CleanerML.btnCleaningOptionsEnable = true;
            this.ShowWindow = false;
        }

        private void StartShredding()
        {
            btnShreddingEnable = true;
            Wipe wiper = new Wipe();
            wiper.WipeErrorEvent += Wiper_WipeErrorEvent;
            wiper.PassInfoEvent += Wiper_PassInfoEvent;
            wiper.SectorInfoEvent += Wiper_SectorInfoEvent;
            wiper.WipeDoneEvent += Wiper_WipeDoneEvent;
            foreach (Model_Shred MdlShared in ShredFilesCollection)
            {

                string full_param = string.Empty;
                if (MdlShared.FilePath != "Recycle Bin")
                {
                    FileAttributes attr = File.GetAttributes(MdlShared.FilePath);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        this.ProgressText = "Started to Shred Folder" + MdlShared.FilePath + " Please Wait.";
                        foreach (string file in Directory.EnumerateFiles(MdlShared.FilePath, "*.*", SearchOption.AllDirectories))
                        {
                            this.ProgressText = "Started to Shred File " + file + " inside folder " + MdlShared.FilePath +
                                                " Please Wait.";
                            wiper.WipeFile(file, 2);
                        }
                        Directory.Delete(MdlShared.FilePath, true);
                    }
                    else
                    {
                        this.ProgressText = "Started to Shred File " + MdlShared.FilePath + " Please Wait.";
                        wiper.WipeFile(MdlShared.FilePath, 2);
                    }
                }

                this.ProgressIndex++;
            }
            if (btnShredRecycleBinEnable)
            {
                this.ProgressText = "Started to Shreding recyclebin please Wait.";

                Thread.Sleep(2000);
                foreach (string file in Directory.EnumerateFiles("C:\\$RECYCLE.BIN", "*.*", SearchOption.AllDirectories)
                    )
                {
                    this.ProgressText = "Started to Shred File " + file + " please Wait.";
                    wiper.WipeFile(file, 2);
                }
            }


            btnShreddingEnable = true;
            this.ProgressText = "Shredding is done.";
        }

        void Wiper_WipeDoneEvent(WipeDoneEventArgs e)
        {
            Console.WriteLine("Wipe is done");
        }

        void Wiper_SectorInfoEvent(SectorInfoEventArgs e)
        {
            
        }

        void Wiper_PassInfoEvent(PassInfoEventArgs e)
        {
            Console.WriteLine("Passinfo Event");
        }

        void Wiper_WipeErrorEvent(WipeErrorEventArgs e)
        {
            Console.WriteLine("Wipe error detail: " + e.WipeError.Message);
        }

        void AddLog(string log, bool addCrLf = true)
        {
            this.Log += log + (addCrLf ? "\r\n" : string.Empty);
        }
    }
}
