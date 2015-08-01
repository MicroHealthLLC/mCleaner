using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        private ObservableCollection<Model_DuplicateChecker> _DupplicationCollection = new ObservableCollection<Model_DuplicateChecker>();
        public ObservableCollection<Model_DuplicateChecker> DupplicateCollection
        {
            get { return _DupplicationCollection; }
            set
            {
                if (_DupplicationCollection != value)
                {
                    _DupplicationCollection = value;
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

        private bool _IsMove = false;
        public bool IsMove
        {
            get { return _IsMove; }
            set
            {
                if (_IsMove != value)
                {
                    _IsMove = value;
                    base.RaisePropertyChanged("IsMove");
                }
            }
        }

        private bool _IsDelete = false;
        public bool IsDelete
        {
            get { return _IsDelete; }
            set
            {
                if (_IsDelete != value)
                {
                    _IsDelete = value;
                    base.RaisePropertyChanged("IsDelete");
                }
            }
        }

        private bool _FileOperationPanelShow = false;
        public bool FileOperationPanelShow
        {
            get { return _FileOperationPanelShow; }
            set
            {
                if (_FileOperationPanelShow != value)
                {
                    _FileOperationPanelShow = value;
                    base.RaisePropertyChanged("FileOperationPanelShow");
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

        private bool _EnableSelectFolder = false;
        public bool EnableSelectFolder
        {
            get { return _EnableSelectFolder; }
            set
            {
                if (_EnableSelectFolder != value)
                {
                    _EnableSelectFolder = value;
                    base.RaisePropertyChanged("EnableSelectFolder");
                }
            }
        }

        private bool _EnableScanFolder = false;
        public bool EnableScanFolder
        {
            get { return _EnableScanFolder; }
            set
            {
                if (_EnableScanFolder != value)
                {
                    _EnableScanFolder = value;
                    base.RaisePropertyChanged("EnableScanFolder");
                }
            }
        }

        private bool _EnableRemoveDuplicates = false;
        public bool EnableRemoveDuplicates
        {
            get { return _EnableRemoveDuplicates; }
            set
            {
                if (_EnableRemoveDuplicates != value)
                {
                    _EnableRemoveDuplicates = value;
                    base.RaisePropertyChanged("EnableRemoveDuplicates");
                }
            }
        }
        #endregion

        #region commands
        public ICommand Command_Start { get; internal set; }
        public ICommand Command_CheckDuplicate { get; internal set; }
        public ICommand Command_ShowDupTab { get; internal set; }
        public ICommand Command_ShowPrefWindow { get; internal set; }
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
            }
        }
        #endregion

        #region command methods
        public async void Command_Start_Click()
        {
            await Task.Run(() => CommandLogic_DuplicateChecker.I.Start(this.DupplicateCollection, IsMove ? 1 : 0));
        }

        public void Command_CheckDuplicate_Click()
        {
            CommandLogic_DuplicateChecker.I.CheckDuplicates();

            this.EnableRemoveDuplicates = false;
            this.EnableScanFolder = false;
            this.EnableSelectFolder = false;
        }

        public void Command_ShowDupTab_Click()
        {
            //CleanerML.Run = true;
            //CleanerML.ShowFrontPage = false;
            //CleanerML.ShowCleanerDescription = false;
            CleanerML.SelectedTabIndex = 1;
            //this.CleanerML.TextLog = "Check duplicates";
            //this.FileOperationPanelShow = true;
        }

        public void Command_ShowPrefWindow_Click()
        {
            this.Prefs.ShowWindow = true;
            this.Prefs.SelectedTabIndex = 3;
        }
        #endregion

        #region methods

        #endregion
    }
}
