using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using mCleaner.Properties;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;

namespace mCleaner.ViewModel
{
    public class ViewModel_Preferences : ViewModelBase
    {
        #region vars

        #endregion

        #region properties
        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
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

        private int _TabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _TabIndex; }
            set
            {
                if (_TabIndex != value)
                {
                    _TabIndex = value;
                    base.RaisePropertyChanged("SelectedTabIndex");
                }
            }
        }

        private string _ProxyAddress = string.Empty;
        public string ClamWin_Proxy_Address
        {
            get { return _ProxyAddress; }
            set
            {
                if (_ProxyAddress != value)
                {
                    _ProxyAddress = value;
                    base.RaisePropertyChanged("ProxyAddress");
                }
            }
        }

        private string _ProxyPort = string.Empty;
        public string ProxyPort
        {
            get { return _ProxyPort; }
            set
            {
                if (_ProxyPort != value)
                {
                    _ProxyPort = value;
                    base.RaisePropertyChanged("ProxyPort");
                }
            }
        }

        private string _ProxyUsername = string.Empty;
        public string ProxyUsername
        {
            get { return _ProxyUsername; }
            set
            {
                if (_ProxyUsername != value)
                {
                    _ProxyUsername = value;
                    base.RaisePropertyChanged("ProxyUsername");
                }
            }
        }

        private string _ProxyPassword = string.Empty;
        public string ProxyPassword
        {
            get { return _ProxyPassword; }
            set
            {
                if (_ProxyPassword != value)
                {
                    _ProxyPassword = value;
                    base.RaisePropertyChanged("ProxyPassword");
                }
            }
        }

        private string _DatabaseMirror = string.Empty;
        public string DatabaseMirror
        {
            get { return _DatabaseMirror; }
            set
            {
                if (_DatabaseMirror != value)
                {
                    _DatabaseMirror = value;
                    base.RaisePropertyChanged("DatabaseMirror");
                }
            }
        }

        private bool _AutoUpdateDBAtStartup = false;
        public bool AutoUpdateDBAtStartup
        {
            get { return _AutoUpdateDBAtStartup; }
            set
            {
                if (_AutoUpdateDBAtStartup != value)
                {
                    _AutoUpdateDBAtStartup = value;
                    base.RaisePropertyChanged("AutoUpdateDBAtStartup");
                }
            }
        }

        private bool _HideIrrelevantCleaners = true;
        public bool HideIrrelevantCleaners
        {
            get { return _HideIrrelevantCleaners; }
            set
            {
                if (_HideIrrelevantCleaners != value)
                {
                    _HideIrrelevantCleaners = value;
                    base.RaisePropertyChanged("HideIrrelevantCleaners");
                }
            }
        }

        private bool _ShredFiles = false;
        public bool ShredFiles
        {
            get { return _ShredFiles; }
            set
            {
                if (_ShredFiles != value)
                {
                    _ShredFiles = value;
                    base.RaisePropertyChanged("ShredFiles");
                }
            }
        }

        private bool _StartWhenSystemStarts = false;
        public bool StartWhenSystemStarts
        {
            get { return _StartWhenSystemStarts; }
            set
            {
                if (_StartWhenSystemStarts != value)
                {
                    _StartWhenSystemStarts = value;
                    base.RaisePropertyChanged("StartWhenSystemStarts");
                }
            }
        }

        private ObservableCollection<string> _Whitelist = new ObservableCollection<string>();
        public ObservableCollection<string> Whitelist
        {
            get { return _Whitelist; }
            set
            {
                if (_Whitelist != value)
                {
                    _Whitelist = value;
                    base.RaisePropertyChanged("Whitelist");
                }
            }
        }

        private ObservableCollection<string> _CustomLocationList = new ObservableCollection<string>();
        public ObservableCollection<string> CustomLocationList
        {
            get { return _CustomLocationList; }
            set
            {
                if (_CustomLocationList != value)
                {
                    _CustomLocationList = value;
                    base.RaisePropertyChanged("CustomLocationList");
                }
            }
        }

        private ObservableCollection<string> _ClamWin_ScanLocations = new ObservableCollection<string>();
        public ObservableCollection<string> ClamWinScanLocations
        {
            get { return _ClamWin_ScanLocations; }
            set
            {
                if (_ClamWin_ScanLocations != value)
                {
                    _CustomLocationList = value;
                    base.RaisePropertyChanged("ClamWinScanLocations");
                }
            }
        }

        #region DupChecker 
        private ObservableCollection<string> _DupCheckerLocations = new ObservableCollection<string>();
        public ObservableCollection<string> DupCheckerLocations
        {
            get { return _DupCheckerLocations; }
            set
            {
                if (_DupCheckerLocations != value)
                {
                    _DupCheckerLocations = value;
                }
            }
        }

        private int _DupChecker_MinSize = 0;
        public int DupChecker_MinSize
        {
            get { return _DupChecker_MinSize; }
            set
            {
                if (_DupChecker_MinSize != value)
                {
                    _DupChecker_MinSize = value;
                    base.RaisePropertyChanged("DupChecker_MinSize");
                }
            }
        }

        private int _DupChecker_MaxSize = 0;
        public int DupChecker_MaxSize
        {
            get { return _DupChecker_MaxSize; }
            set
            {
                if (_DupChecker_MaxSize != value)
                {
                    _DupChecker_MaxSize = value;
                    base.RaisePropertyChanged("DupChecker_MaxSize");
                }
            }
        }

        private string _DupChecker_FileContaining = string.Empty;
        public string DupChecker_FileContaining
        {
            get { return _DupChecker_FileContaining; }
            set
            {
                if (_DupChecker_FileContaining != value)
                {
                    _DupChecker_FileContaining = value;
                    base.RaisePropertyChanged("DupChecker_FileContaining");
                }
            }
        }

        private string _DupChecker_FileExtensions = "*.*";
        public string DupChecker_FileExtensions
        {
            get { return _DupChecker_FileExtensions; }
            set
            {
                if (_DupChecker_FileExtensions != value)
                {
                    _DupChecker_FileExtensions = value;
                    base.RaisePropertyChanged("DupChecker_FileExtensions");
                }
            }
        }

        private string _DupChecker_DuplicateFolderPath = string.Empty;
        public string DupChecker_DuplicateFolderPath
        {
            get { return _DupChecker_DuplicateFolderPath; }
            set
            {
                if (_DupChecker_DuplicateFolderPath != value)
                {
                    _DupChecker_DuplicateFolderPath = value;
                    base.RaisePropertyChanged("DupChecker_DuplicateFolderPath");
                }
            }
        }
        #endregion
        #endregion

        #region commands
        public ICommand Command_OK { get; set; }
        public ICommand Command_CloseWindow { get; set; }
        public ICommand Command_Menu_Preferences { get; set; }
        public ICommand Command_CustomLocation_AddFile { get; set; }
        public ICommand Command_CustomLocation_AddFolder { get; set; }
        public ICommand Command_CustomLocation_RemoveSelected { get; set; }
        public ICommand Command_Whitelist_AddFile { get; set; }
        public ICommand Command_Whitelist_AddFolder { get; set; }
        public ICommand Command_Whitelist_RemoveSelected { get; set; }
        public ICommand Command_ClamAV_ScanLocation_AddFile { get; set; }
        public ICommand Command_ClamAV_ScanLocation_AddFolder { get; set; }
        public ICommand Command_ClamAV_ScanLocation_RemoveSelected { get; set; }
        public ICommand Command_DupChecker_AddFolder { get; set; }
        public ICommand Command_DupChecker_RemoveSelected { get; set; }
        public ICommand Command_DupChecker_BrowseFolder { get; set; }
        #endregion

        #region ctor
        public ViewModel_Preferences()
        {
            if (base.IsInDesignMode)
            {
                this.ShowWindow = false;

                this.CustomLocationList.Add("One");
                this.CustomLocationList.Add("Two");
                this.CustomLocationList.Add("Three");
                this.CustomLocationList.Add("Four");
            }
            else
            {
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                Command_Menu_Preferences = new RelayCommand(Command_Menu_Preferences_Click);

                Command_CustomLocation_AddFile = new RelayCommand(Command_CustomLocation_AddFile_Click);
                Command_CustomLocation_AddFolder = new RelayCommand(Command_CustomLocation_AddFolder_Click);
                Command_CustomLocation_RemoveSelected = new RelayCommand<string>(Command_CustomLocation_RemoveSelected_Click);

                Command_Whitelist_AddFile = new RelayCommand(Command_Whitelist_AddFile_Click);
                Command_Whitelist_AddFolder = new RelayCommand(Command_Whitelist_AddFolder_Click);
                Command_Whitelist_RemoveSelected = new RelayCommand<string>(Command_Whitelist_RemoveSelected_Click);

                Command_ClamAV_ScanLocation_AddFile = new RelayCommand(Command_ClamAV_ScanLocation_AddFile_Click);
                Command_ClamAV_ScanLocation_AddFolder = new RelayCommand(Command_ClamAV_ScanLocation_AddFolder_Click);
                Command_ClamAV_ScanLocation_RemoveSelected = new RelayCommand<string>(Command_ClamAV_ScanLocation_RemoveSelected_Click);

                Command_DupChecker_AddFolder = new RelayCommand(Command_DupChecker_AddFolder_Click);
                Command_DupChecker_RemoveSelected = new RelayCommand<string>(Command_DupChecker_RemoveSelected_Click);
                Command_DupChecker_BrowseFolder = new RelayCommand(Command_DupChecker_BrowseFolder_Click);

                ReadSettings();
            }
        }
        #endregion

        #region command methods
        void Command_OK_Click()
        {
            WriteSettings();
            this.CleanerML.RefreshSystemCleaners();
            this.ShowWindow = false;
        }
        void Command_CloseWindow_Click()
        {
            this.ShowWindow = false;
            ReadSettings();
        }
        void Command_Menu_Preferences_Click()
        {
            this.ShowWindow = true;
        }

        void Command_CustomLocation_AddFile_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Select file to add",
                Filter = "All files|*.*",
                Multiselect = true
            };
            ofd.ShowDialog();
            if (ofd.FileName != string.Empty && ofd.FileNames.Length > 0)
            {
                foreach (string s in ofd.FileNames)
                {
                    if (!this.CustomLocationList.Contains(s))
                    {
                        this.CustomLocationList.Add(s);
                    }
                }
            }
        }
        void Command_CustomLocation_AddFolder_Click()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                if (!this.CustomLocationList.Contains(fbd.SelectedPath))
                {
                    this.CustomLocationList.Add(fbd.SelectedPath);
                }
            }
        }
        void Command_CustomLocation_RemoveSelected_Click(string selected)
        {
            if (this.CustomLocationList.Contains(selected))
            {
                this.CustomLocationList.Remove(selected);
            }
        }

        void Command_Whitelist_AddFile_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Select file to add",
                Filter = "All files|*.*",
                Multiselect = true
            };
            ofd.ShowDialog();
            if (ofd.FileName != string.Empty && ofd.FileNames.Length > 0)
            {
                foreach (string s in ofd.FileNames)
                {
                    if (!this.Whitelist.Contains(s))
                    {
                        this.Whitelist.Add(s);
                    }
                }
            }
        }
        void Command_Whitelist_AddFolder_Click()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                if (!this.Whitelist.Contains(fbd.SelectedPath))
                {
                    this.Whitelist.Add(fbd.SelectedPath);
                }
            }
        }
        void Command_Whitelist_RemoveSelected_Click(string selected)
        {
            if (this.Whitelist.Contains(selected))
            {
                this.Whitelist.Remove(selected);
            }
        }

        void Command_ClamAV_ScanLocation_AddFile_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Select file to add",
                Filter = "All files|*.*",
                Multiselect = true
            };
            ofd.ShowDialog();
            if (ofd.FileName != string.Empty && ofd.FileNames.Length > 0)
            {
                foreach (string s in ofd.FileNames)
                {
                    if (!this.ClamWinScanLocations.Contains(s))
                    {
                        this.ClamWinScanLocations.Add(s);
                    }
                }
            }
        }
        void Command_ClamAV_ScanLocation_AddFolder_Click()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                if (!this.ClamWinScanLocations.Contains(fbd.SelectedPath))
                {
                    this.ClamWinScanLocations.Add(fbd.SelectedPath);
                }
            }
        }
        void Command_ClamAV_ScanLocation_RemoveSelected_Click(string selected)
        {
            if (this.ClamWinScanLocations.Contains(selected))
            {
                this.ClamWinScanLocations.Remove(selected);
            }
        }

        void Command_DupChecker_AddFolder_Click()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                if (!this.DupCheckerLocations.Contains(fbd.SelectedPath))
                {
                    this.DupCheckerLocations.Add(fbd.SelectedPath);
                }
            }
        }
        void Command_DupChecker_RemoveSelected_Click(string selected)
        {
            if (this.DupCheckerLocations.Contains(selected))
            {
                this.DupCheckerLocations.Remove(selected);
            }
        }
        void Command_DupChecker_BrowseFolder_Click()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                this.DupChecker_DuplicateFolderPath = fbd.SelectedPath;
            }
        }
        #endregion

        #region methods
        void ReadSettings()
        {
            if (Settings.Default.ClamWin_Proxy_Address != string.Empty)
            {
                string[] proxy = Settings.Default.ClamWin_Proxy_Address.Split(':');
                this.ClamWin_Proxy_Address = proxy[0];
                this.ProxyPort = proxy[1];
            }
            if (Settings.Default.ClamWin_Proxy_UserPass != string.Empty)
            {
                string[] userpass = Settings.Default.ClamWin_Proxy_UserPass.Split(':');
                this.ProxyUsername = userpass[0];
                this.ProxyPassword = userpass[1];
            }
            if (Settings.Default.ClamWin_DatabaseMirror != string.Empty)
            {
                this.DatabaseMirror = Settings.Default.ClamWin_DatabaseMirror;
            }
            this.AutoUpdateDBAtStartup      = Settings.Default.ClamWin_UpdateDBAtStartup;
            this.HideIrrelevantCleaners     = Settings.Default.HideIrrelevantCleaners;
            this.ShredFiles                 = Settings.Default.ShredFiles;
            this.StartWhenSystemStarts      = Settings.Default.StartWhenSystemStarts;
            //this.Whitelist                  = Settings.Default.WhitelistCollection;
            //this.CustomLocationList         = Settings.Default.CustomLocationForDeletion;
            if (Settings.Default.WhitelistCollection != null)
            {
                this.Whitelist.Clear();
                foreach (string s in Settings.Default.WhitelistCollection)
                {
                    this.Whitelist.Add(s);
                }
            }

            if (Settings.Default.CustomLocationForDeletion != null)
            {
                this.CustomLocationList.Clear();
                foreach (string s in Settings.Default.CustomLocationForDeletion)
                {
                    this.CustomLocationList.Add(s);
                }
            }

            if (Settings.Default.ClamWin_ScanLocations != null)
            {
                this.ClamWinScanLocations.Clear();
                foreach (string s in Settings.Default.ClamWin_ScanLocations)
                {
                    this.ClamWinScanLocations.Add(s);
                }
            }

            if (Settings.Default.DupChecker_CustomPath != null)
            {
                this.DupCheckerLocations.Clear();
                foreach (string s in Settings.Default.DupChecker_CustomPath)
                {
                    this.DupCheckerLocations.Add(s);
                } 
            }

            this.DupChecker_MinSize = Settings.Default.DupChecker_MinSize;
            this.DupChecker_MaxSize = Settings.Default.DupChecker_MaxSize;
            this.DupChecker_FileExtensions = Settings.Default.DupChecker_FileExtensions;
            this.DupChecker_DuplicateFolderPath = Settings.Default.DupChecker_DuplicateFolderPath;
        }

        void WriteSettings()
        {
            if (this.ClamWin_Proxy_Address != string.Empty && this.ProxyPort != string.Empty)
            {
                Settings.Default.ClamWin_Proxy_Address = this.ClamWin_Proxy_Address + ":" + this.ProxyPort;
            }
            else if (this.ClamWin_Proxy_Address == string.Empty && this.ProxyPort == string.Empty)
            {
                Settings.Default.ClamWin_Proxy_Address = string.Empty;
            }
            else
            {
                MessageBox.Show("Incomplete proxy address", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (this.ProxyUsername != string.Empty && this.ProxyPassword != string.Empty)
            {
                Settings.Default.ClamWin_Proxy_UserPass = this.ProxyUsername + ":" + this.ProxyPassword;
            }
            else if (this.ProxyUsername == string.Empty && this.ProxyPassword == string.Empty)
            {
                Settings.Default.ClamWin_Proxy_UserPass = string.Empty;
            }
            else
            {
                MessageBox.Show("Incomplete proxy login", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Settings.Default.ClamWin_UpdateDBAtStartup  = this.AutoUpdateDBAtStartup;
            Settings.Default.HideIrrelevantCleaners     = this.HideIrrelevantCleaners;
            Settings.Default.ShredFiles                 = this.ShredFiles;
            Settings.Default.StartWhenSystemStarts      = this.StartWhenSystemStarts;

            if (Settings.Default.CustomLocationForDeletion == null) Settings.Default.CustomLocationForDeletion = new StringCollection();
            Settings.Default.CustomLocationForDeletion.Clear();
            Settings.Default.CustomLocationForDeletion.AddRange(this.CustomLocationList.ToArray());

            if (Settings.Default.WhitelistCollection == null) Settings.Default.WhitelistCollection = new StringCollection();
            Settings.Default.WhitelistCollection.Clear();
            Settings.Default.WhitelistCollection.AddRange(this.Whitelist.ToArray());

            if (Settings.Default.ClamWin_ScanLocations == null) Settings.Default.ClamWin_ScanLocations = new StringCollection();
            Settings.Default.ClamWin_ScanLocations.Clear();
            Settings.Default.ClamWin_ScanLocations.AddRange(this.ClamWinScanLocations.ToArray());

            if (Settings.Default.DupChecker_CustomPath == null) Settings.Default.DupChecker_CustomPath = new StringCollection();
            Settings.Default.DupChecker_CustomPath.Clear();
            Settings.Default.DupChecker_CustomPath.AddRange(this.DupCheckerLocations.ToArray());

            Settings.Default.DupChecker_MinSize = this.DupChecker_MinSize;
            Settings.Default.DupChecker_MaxSize = this.DupChecker_MaxSize;
            Settings.Default.DupChecker_FileExtensions = this.DupChecker_FileExtensions;
            Settings.Default.DupChecker_DuplicateFolderPath = this.DupChecker_DuplicateFolderPath;

            RegistryHelper.I.RegisterStartup(this.StartWhenSystemStarts);

            Settings.Default.Save();
        }
        #endregion
    }
}