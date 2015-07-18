using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Logics.Commands;
using mCleaner.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

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
        #endregion

        #region commands
        public ICommand Command_Start { get; internal set; }
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
                this.Command_Start = new RelayCommand(Command_Start_Click);
            }
        }
        #endregion

        #region command methods
        public async void Command_Start_Click()
        {
            await Task.Run(() => CommandLogic_DuplicateChecker.I.Start(this.DupplicateCollection, IsMove ? 1 : 0));
            
        }
        #endregion

        #region methods

        #endregion
    }
}
