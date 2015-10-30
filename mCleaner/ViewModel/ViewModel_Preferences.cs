using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using mCleaner.Properties;
using Microsoft.Practices.ServiceLocation;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

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

        public ViewModel_DuplicateChecker DupCheck
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
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

        private int _tabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _tabIndex; }
            set
            {
                if (_tabIndex != value)
                {
                    _tabIndex = value;
                    base.RaisePropertyChanged("SelectedTabIndex");
                }
            }
        }

        private string _proxyAddress = string.Empty;
        public string ClamWin_Proxy_Address
        {
            get { return _proxyAddress; }
            set
            {
                if (_proxyAddress != value)
                {
                    _proxyAddress = value;
                    base.RaisePropertyChanged("ProxyAddress");
                }
            }
        }

        private string _proxyPort = string.Empty;
        public string ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                if (_proxyPort != value)
                {
                    _proxyPort = value;
                    base.RaisePropertyChanged("ProxyPort");
                }
            }
        }

        private string _proxyUsername = string.Empty;
        public string ProxyUsername
        {
            get { return _proxyUsername; }
            set
            {
                if (_proxyUsername != value)
                {
                    _proxyUsername = value;
                    base.RaisePropertyChanged("ProxyUsername");
                }
            }
        }

        private string _proxyPassword = string.Empty;
        public string ProxyPassword
        {
            get { return _proxyPassword; }
            set
            {
                if (_proxyPassword != value)
                {
                    _proxyPassword = value;
                    base.RaisePropertyChanged("ProxyPassword");
                }
            }
        }

        private string _databaseMirror = string.Empty;
        public string DatabaseMirror
        {
            get { return _databaseMirror; }
            set
            {
                if (_databaseMirror != value)
                {
                    _databaseMirror = value;
                    base.RaisePropertyChanged("DatabaseMirror");
                }
            }
        }

        private bool _autoUpdateDbAtStartup = false;
        public bool AutoUpdateDBAtStartup
        {
            get { return _autoUpdateDbAtStartup; }
            set
            {
                if (_autoUpdateDbAtStartup != value)
                {
                    _autoUpdateDbAtStartup = value;
                    base.RaisePropertyChanged("AutoUpdateDBAtStartup");
                }
            }
        }

        private bool _hideIrrelevantCleaners = true;
        public bool HideIrrelevantCleaners
        {
            get { return _hideIrrelevantCleaners; }
            set
            {
                if (_hideIrrelevantCleaners != value)
                {
                    _hideIrrelevantCleaners = value;
                    base.RaisePropertyChanged("HideIrrelevantCleaners");
                }
            }
        }

        private bool _duplicateFilterFileSizeCriteara = true;
        public bool DuplicateFilterFileSizeCriteara
        {
            get { return _duplicateFilterFileSizeCriteara; }
            set
            {
                if (_duplicateFilterFileSizeCriteara != value)
                {
                    _duplicateFilterFileSizeCriteara = value;
                    base.RaisePropertyChanged("DuplicateFilterFileSizeCriteara");
                }
            }
        }
       
        private bool _shredFiles = false;
        public bool ShredFiles
        {
            get { return _shredFiles; }
            set
            {
                if (_shredFiles != value)
                {
                    _shredFiles = value;
                    base.RaisePropertyChanged("ShredFiles");
                }
            }
        }

        private bool _startWhenSystemStarts = false;
        public bool StartWhenSystemStarts
        {
            get { return _startWhenSystemStarts; }
            set
            {
                if (_startWhenSystemStarts != value)
                {
                    _startWhenSystemStarts = value;
                    base.RaisePropertyChanged("StartWhenSystemStarts");
                }
            }
        }

        private ObservableCollection<string> _whitelist = new ObservableCollection<string>();
        public ObservableCollection<string> Whitelist
        {
            get { return _whitelist; }
            set
            {
                if (_whitelist != value)
                {
                    _whitelist = value;
                    base.RaisePropertyChanged("Whitelist");
                }
            }
        }

        private ObservableCollection<string> _customLocationList = new ObservableCollection<string>();
        public ObservableCollection<string> CustomLocationList
        {
            get { return _customLocationList; }
            set
            {
                if (_customLocationList != value)
                {
                    _customLocationList = value;
                    base.RaisePropertyChanged("CustomLocationList");
                }
            }
        }

        private ObservableCollection<string> _clamWinScanLocations = new ObservableCollection<string>();
        public ObservableCollection<string> ClamWinScanLocations
        {
            get { return _clamWinScanLocations; }
            set
            {
                if (_clamWinScanLocations != value)
                {
                    _clamWinScanLocations = value;
                    base.RaisePropertyChanged("ClamWinScanLocations");
                }
            }
        }

        #region DupChecker 
        private ObservableCollection<string> _dupCheckerLocations = new ObservableCollection<string>();
        public ObservableCollection<string> DupCheckerLocations
        {
            get { return _dupCheckerLocations; }
            set { _dupCheckerLocations = value; }
        }

        private int _dupCheckerMinSize = 0;
        public int DupChecker_MinSize
        {
            get { return _dupCheckerMinSize; }
            set
            {
                if (_dupCheckerMinSize != value)
                {
                    _dupCheckerMinSize = value;
                    base.RaisePropertyChanged("DupChecker_MinSize");
                }
            }
        }

        private int _dupCheckerMaxSize = 0;
        public int DupChecker_MaxSize
        {
            get { return _dupCheckerMaxSize; }
            set
            {
                if (_dupCheckerMaxSize != value)
                {
                    _dupCheckerMaxSize = value;
                    base.RaisePropertyChanged("DupChecker_MaxSize");
                }
            }
        }

        private string _dupCheckerFileContaining = string.Empty;
        public string DupChecker_FileContaining
        {
            get { return _dupCheckerFileContaining; }
            set
            {
                if (_dupCheckerFileContaining != value)
                {
                    _dupCheckerFileContaining = value;
                    base.RaisePropertyChanged("DupChecker_FileContaining");
                }
            }
        }

        private string _dupCheckerFileExtensions = "*.*";
        public string DupChecker_FileExtensions
        {
            get { return _dupCheckerFileExtensions; }
            set
            {
                if (_dupCheckerFileExtensions != value)
                {
                    _dupCheckerFileExtensions = value;
                    base.RaisePropertyChanged("DupChecker_FileExtensions");
                }
            }
        }

        private string _dupCheckerDuplicateFolderPath = string.Empty;
        public string DupChecker_DuplicateFolderPath
        {
            get { return _dupCheckerDuplicateFolderPath; }
            set
            {
                if (_dupCheckerDuplicateFolderPath != value)
                {
                    _dupCheckerDuplicateFolderPath = value;
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
            DupCheck.EnableScanFolder = Settings.Default.DupChecker_CustomPath.Count > 0;
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            this.DuplicateFilterFileSizeCriteara = Settings.Default.DuplicateFilterFileSizeCriteara;
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

            Settings.Default.ClamWin_UpdateDBAtStartup              = this.AutoUpdateDBAtStartup;
            Settings.Default.HideIrrelevantCleaners                 = this.HideIrrelevantCleaners;
            Settings.Default.ShredFiles                             = this.ShredFiles;
            Settings.Default.StartWhenSystemStarts                  = this.StartWhenSystemStarts;
            Settings.Default.DuplicateFilterFileSizeCriteara        = this.DuplicateFilterFileSizeCriteara;

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