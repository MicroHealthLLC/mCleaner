using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using mCleaner.Model;
using Microsoft.Practices.ServiceLocation;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace mCleaner.ViewModel
{
    public class ViewModel_Shred : ViewModelBase
    {
        private string _WindowTitle = string.Empty;
        public string WindowTitle
        {
            get { return _WindowTitle; }
            set
            {
                if (_WindowTitle != value)
                {
                    _WindowTitle = value;
                    base.RaisePropertyChanged("WindowTitle");
                }
            }
        }

        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }
        public  ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }

        private string _strDefaultScanLocation =string.Empty;
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


        private ObservableCollection<Model_Shred> _ShredFilesCollection = new ObservableCollection<Model_Shred>();
        public ObservableCollection<Model_Shred> ShredFilesCollection
        {
            get { return _ShredFilesCollection; }
            set
            {
                if (_ShredFilesCollection != value)
                {
                    _ShredFilesCollection = value;
                    base.RaisePropertyChanged("ShredFilesCollection");
                }
            }
        }

        private int _ProgressMax = 0;
        public int ProgressMax
        {
            get { return _ProgressMax; }
            set
            {
                if (_ProgressMax != value)
                {
                    _ProgressMax = value;
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

        private bool _btnShreddingEnable = true;
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

        private int _ProgressIndex = 0;
        public int ProgressIndex
        {
            get { return _ProgressIndex; }
            set
            {
                if (_ProgressIndex != value)
                {
                    _ProgressIndex = value;
                    base.RaisePropertyChanged("ProgressIndex");
                }
            }
        }

        private string _ProgressText = string.Empty;
        public string ProgressText
        {
            get { return _ProgressText; }
            set
            {
                if (_ProgressText != value)
                {
                    _ProgressText = value;
                    base.RaisePropertyChanged("ProgressText");
                }
            }
        }

        private string _Log = string.Empty;
        public string Log
        {
            get { return _Log; }
            set
            {
                if (_Log != value)
                {
                    _Log = value;
                    base.RaisePropertyChanged("Log");
                }
            }
        }

        private bool _ShowWindow = false;
        public bool ShowWindow
        {
            get { return _ShowWindow; }
            set
            {
                if (_ShowWindow != value)
                {
                    _ShowWindow = value;
                    base.RaisePropertyChanged("ShowWindow");
                }
            }
        }

        public ICommand Command_ShredStart { get; set; }
        public ICommand Command_SelectFiles { get; set; }
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
                        if (!this.ShredFilesCollection.Where(dc=>dc.FilePath==file).Any())
                            this.ShredFilesCollection.Add(new Model_Shred() { FilePath = file });
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
                    this.ShredFilesCollection.Add(new Model_Shred() { FilePath = fbd.SelectedPath });
            }
        }


        public void Command_ShredRecycleBin_Click()
        {

            if (MessageBox.Show("Are you sure you want to shred files that are in recycle bin?", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            btnShreddingEnable = false;
            Wipe Wiper = new Wipe();
            Wiper.WipeErrorEvent += Wiper_WipeErrorEvent;
            Wiper.PassInfoEvent += Wiper_PassInfoEvent;
            Wiper.SectorInfoEvent += Wiper_SectorInfoEvent;
            Wiper.WipeDoneEvent += Wiper_WipeDoneEvent;
            this.ProgressText = "Started to Shreding recyclebin please Wait.";
            Thread.Sleep(2000);
            foreach (string file in Directory.EnumerateFiles("C:\\$RECYCLE.BIN", "*.*", SearchOption.AllDirectories))
            {
                this.ProgressText = "Started to Shred File " + file + " please Wait.";
                Wiper.WipeFile(file, 2);
            }

            this.ProgressText = "Done.";
            btnShreddingEnable = true;

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

        void StartShredding()
        {
            btnShreddingEnable = true;
            Wipe Wiper = new Wipe();
            Wiper.WipeErrorEvent += Wiper_WipeErrorEvent;
            Wiper.PassInfoEvent += Wiper_PassInfoEvent;
            Wiper.SectorInfoEvent += Wiper_SectorInfoEvent;
            Wiper.WipeDoneEvent += Wiper_WipeDoneEvent;
            foreach (Model_Shred MdlShared in ShredFilesCollection)
            {
                //            AddLog("Starting to securely shred " + this.QueueFiles.Count + " files with 5 iterations.");

                string full_param = string.Empty;
                FileAttributes attr = File.GetAttributes(MdlShared.FilePath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    //Directory delete Command "erase dir=\"C:\filename-list.txt\" /quiet"
                    this.ProgressText = "Started to Shred Folder" + MdlShared.FilePath + " Please Wait.";
                    foreach (string file in Directory.EnumerateFiles(MdlShared.FilePath, "*.*", SearchOption.AllDirectories))
                    {
                        this.ProgressText = "Started to Shred File " + file + " inside folder " + MdlShared.FilePath + " Please Wait.";
                        Wiper.WipeFile(file, 2);
                    }
                    Directory.Delete(MdlShared.FilePath, true);
                }
                else
                {
                    // for File Command  erase file="C:\filename-list.txt" /quiet
                    this.ProgressText = "Started to Shred File " + MdlShared.FilePath + " Please Wait.";
                    Wiper.WipeFile(MdlShared.FilePath, 2);
                } 

                this.ProgressIndex++;
            }
            btnShreddingEnable = true;
            this.ProgressText = "Shreding is done you can now close the window.";
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
