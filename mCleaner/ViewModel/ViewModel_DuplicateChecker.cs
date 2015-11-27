using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Logics.Commands;
using mCleaner.Model;
using mCleaner.Properties;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.ViewModel
{
    public class ViewModel_DuplicateChecker : ViewModelBase
    {
        #region properties
        private ObservableCollection<Model_DuplicateChecker> _dupplicationCollection = new ObservableCollection<Model_DuplicateChecker>();
        public ObservableCollection<Model_DuplicateChecker> DupplicateCollection
        {
            get { return _dupplicationCollection; }
            set
            {
                if (_dupplicationCollection != value)
                {
                    _dupplicationCollection = value;
                    base.RaisePropertyChanged("DupplicateCollection");
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

        public ViewModel_Preferences Prefs
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Preferences>();
            }
        }

        private bool _isMove = false;
        public bool IsMove
        {
            get { return _isMove; }
            set
            {
                if (_isMove != value)
                {
                    _isMove = value;
                    base.RaisePropertyChanged("IsMove");
                }
            }
        }

        private bool _isDelete = false;
        public bool IsDelete
        {
            get { return _isDelete; }
            set
            {
                if (_isDelete != value)
                {
                    _isDelete = value;
                    base.RaisePropertyChanged("IsDelete");
                }
            }
        }

        private bool _fileOperationPanelShow = false;
        public bool FileOperationPanelShow
        {
            get { return _fileOperationPanelShow; }
            set
            {
                if (_fileOperationPanelShow != value)
                {
                    _fileOperationPanelShow = value;
                    base.RaisePropertyChanged("FileOperationPanelShow");
                }
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

        private bool _enableSelectFolder = false;
        public bool EnableSelectFolder
        {
            get { return _enableSelectFolder; }
            set
            {
                if (_enableSelectFolder != value)
                {
                    _enableSelectFolder = value;
                    base.RaisePropertyChanged("EnableSelectFolder");
                }
            }
        }

        private bool _enableScanFolder = false;
        public bool EnableScanFolder
        {
            get { return _enableScanFolder; }
            set
            {
                if (_enableScanFolder != value)
                {
                    _enableScanFolder = value;
                    base.RaisePropertyChanged("EnableScanFolder");
                }
            }
        }

        private bool _enableRemoveDuplicates = false;
        public bool EnableRemoveDuplicates
        {
            get { return _enableRemoveDuplicates; }
            set
            {
                if (_enableRemoveDuplicates != value)
                {
                    _enableRemoveDuplicates = value;
                    base.RaisePropertyChanged("EnableRemoveDuplicates");
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

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
               _cancel = value;
            }
        }


        #endregion

        #region commands
        public ICommand Command_Start { get; internal set; }
        public ICommand Command_CheckDuplicate { get; internal set; }

        public ICommand Command_Cancel { get; internal set; }

        public ICommand Command_ShowDupTab { get; internal set; }
        public ICommand Command_ShowPrefWindow { get; internal set; }
        public ICommand Command_CloseWindow { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_DuplicateChecker()
        {
            this.IsMove = true;

            if (base.IsInDesignMode)
            {
                FileOperationPanelShow = true;

                DupplicateCollection.Add(new Model_DuplicateChecker()
                {
                    FileDetails = new Model_DuplicateChecker_FileDetails() { Filename = "win.com", Fullfilepath = "c:\\windows\\win.com", ParentDirectory = "c:\\windows" },
                    Hash = "12345"
                });
                DupplicateCollection.Add(new Model_DuplicateChecker()
                {
                    FileDetails = new Model_DuplicateChecker_FileDetails() { Filename = "ackack.com", Fullfilepath = "c:\\windows\\ackack.com", ParentDirectory = "c:\\windows\\system32" },
                    Hash = "12345"
                });
                DupplicateCollection.Add(new Model_DuplicateChecker()
                {
                    FileDetails = new Model_DuplicateChecker_FileDetails() { Filename = "command.com", Fullfilepath = "c:\\windows\\command.com", ParentDirectory = "c:\\windows" },
                    Hash = "67890"
                });
                DupplicateCollection.Add(new Model_DuplicateChecker()
                {
                    FileDetails = new Model_DuplicateChecker_FileDetails() { Filename = "com.com", Fullfilepath = "c:\\windows\\com.com", ParentDirectory = "c:\\windows\\system32" },
                    Hash = "67890"
                });

                ProgressText = "Status Text";
            }
            else
            {
                this.EnableSelectFolder = true;
                this.EnableScanFolder = true;
                this.EnableRemoveDuplicates = true;

                FileOperationPanelShow = true;
                this.Command_Start = new RelayCommand(Command_Start_Click);
                this.Command_CheckDuplicate = new RelayCommand(Command_CheckDuplicate_Click);
                this.Command_ShowDupTab = new RelayCommand(Command_ShowDupTab_Click);
                this.Command_ShowPrefWindow = new RelayCommand(Command_ShowPrefWindow_Click);
                this.Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                this.Command_Cancel = new RelayCommand(Command_Cancel_Click);
            }
        }
        #endregion

        #region command methods
        public async void Command_Start_Click()
        {
            if (this.DupplicateCollection.Where(dc => dc.Selected == true).Count() == this.DupplicateCollection.Count())
            {
                if(MessageBox.Show("Are you about to delete all the files. you sure that you want to continue?","mCleaner",MessageBoxButton.YesNo,MessageBoxImage.Warning)==MessageBoxResult.No)
                    return;

            }
            await Task.Run(() => CommandLogic_DuplicateChecker.I.Start(this.DupplicateCollection, IsMove ? 1 : 0));

            DupplicateCollection.Clear();
            this.EnableRemoveDuplicates = false;
        }

        public void Command_CheckDuplicate_Click()
        {
            Trace.WriteLine("Check Duplicates Started.");
            CommandLogic_DuplicateChecker.I.CheckDuplicates();

            this.EnableRemoveDuplicates = false;
            this.EnableScanFolder = false;
            this.EnableSelectFolder = false;
        }

        public void Command_ShowDupTab_Click()
        {
            CleanerML.Run = false;
            CleanerML.ShowCleanerDescription = false;
            CleanerML.btnCleanNowPreviousState = CleanerML.btnPreviewCleanEnable;
            CleanerML.btnPreviewCleanEnable = false;
            CleanerML.btnCleaningOptionsEnable = false;
            CleanerML.ShowFrontPage = false;
            this.EnableRemoveDuplicates = false;
            if (Settings.Default.DupChecker_CustomPath != null && Settings.Default.DupChecker_CustomPath.Count > 0)
                this.EnableScanFolder = true;
            else
                this.EnableScanFolder = false;

            DupplicateCollection.Clear();
            this.ShowWindow = true;

            ProgressText = string.Empty;
        }

        public void Command_ShowPrefWindow_Click()
        {
            this.Prefs.ShowWindow = true;
            this.Prefs.SelectedTabIndex = 3;
        }

        public void Command_CloseWindow_Click()
        {
            this.ShowWindow = false;
            CleanerML.Run = false;
            CleanerML.ShowCleanerDescription = false;
            CleanerML.ShowFrontPage = true;
            CleanerML.btnPreviewCleanEnable = CleanerML.btnCleanNowPreviousState;
            CleanerML.btnCleaningOptionsEnable = true;
        }

        public void Command_Cancel_Click()
        {
            this.Cancel = true;
        }
        #endregion

        #region methods

        #endregion
    }
}
