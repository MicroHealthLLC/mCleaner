using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;

namespace mCleaner.ViewModel
{
    public class ViewModel_Clam : ViewModelBase
    {
        #region properties
        private bool _ShowClamWinVirusUpdateWindow = false;
        public bool ShowClamWinVirusUpdateWindow
        {
            get { return _ShowClamWinVirusUpdateWindow; }
            set
            {
                if (_ShowClamWinVirusUpdateWindow != value)
                {
                    _ShowClamWinVirusUpdateWindow = value;
                    base.RaisePropertyChanged("ShowClamWinVirusUpdateWindow");
                }
            }
        }

        private bool _ShowClamWinVirusScanner = false;
        public bool ShowClamWinVirusScanner
        {
            get { return _ShowClamWinVirusScanner; }
            set
            {
                if (_ShowClamWinVirusScanner != value)
                {
                    _ShowClamWinVirusScanner = value;
                    base.RaisePropertyChanged("ShowClamWinVirusScanner");
                }
            }
        }

        private string _VirusDefUpdateLog = string.Empty;
        public string VirusDefUpdateLog
        {
            get { return _VirusDefUpdateLog; }
            set
            {
                if (_VirusDefUpdateLog != value)
                {
                    _VirusDefUpdateLog = value;
                    base.RaisePropertyChanged("VirusDefUpdateLog");
                }
            }
        }

        private string _WindowTitle = "Update Virus Definition Database";
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

        private ObservableCollection<Model_VirusDetails> _InfectedFilesCollection = new ObservableCollection<Model_VirusDetails>();
        public ObservableCollection<Model_VirusDetails> InfectedFilesCollection
        {
            get { return _InfectedFilesCollection; }
            set
            {
                if (_InfectedFilesCollection != value)
                {
                    _InfectedFilesCollection = value;
                    base.RaisePropertyChanged("InfectedFilesCollection");
                }
            }
        }

        private bool _EnableCleanNowButton = false;
        public bool EnableCleanNowButton
        {
            get { return _EnableCleanNowButton; }
            set
            {
                if (_EnableCleanNowButton != value)
                {
                    _EnableCleanNowButton = value;
                    base.RaisePropertyChanged("EnableCleanNowButton");
                }
            }
        }

        private bool _EnableCancelButton = false;
        public bool EnableCancelButton
        {
            get { return _EnableCancelButton; }
            set
            {
                if (_EnableCancelButton != value)
                {
                    _EnableCancelButton = value;
                    base.RaisePropertyChanged("EnableCancelButton");
                }
            }
        }

        private bool _EnableCloseButton = false;
        public bool EnableCloseButton
        {
            get { return _EnableCloseButton; }
            set
            {
                if (_EnableCloseButton != value)
                {
                    _EnableCloseButton = value;
                    base.RaisePropertyChanged("EnableCloseButton");
                }
            }
        }
        #endregion

        #region ctor
        public ViewModel_Clam()
        {
            if (base.IsInDesignMode)
            {
                this.ShowClamWinVirusUpdateWindow = false;
                this.VirusDefUpdateLog = "Log goes here";
                this.ProgressText = "Progress Text Here";
                this.InfectedFilesCollection.Add(new Model_VirusDetails()
                {
                    File = @"C:\Windows\Explorer.exe",
                    VirusName = "Trojan.Delf-11287"
                });

                this.InfectedFilesCollection.Add(new Model_VirusDetails()
                {
                    File = @"G:\Installers\Cracks\VSO.ConvertXtoDVD.3.7.2.188.Incl.Keymaker\Keygen BRD\Keygen.exe",
                    VirusName = "Win.Trojan.Downloader-28238"
                });

                this.InfectedFilesCollection.Add(new Model_VirusDetails()
                {
                    File = @"G:\Installers\Cracks\VSO.ConvertXtoDVD.3.7.2.188.Incl.Keymaker\vsoConvertXtoDVD3_setup.exe",
                    VirusName = "Trojan.Dropper-20708"
                });
            }
            else
            {
                Command_UpdateVirusDefinition = new RelayCommand(Command_UpdateVirusDefinition_Click);
                Command_ScanForVirus = new RelayCommand(Command_ScanForVirus_Click);
                Command_CancelUpdate = new RelayCommand(Command_CancelUpdate_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                Command_ScanMemory = new RelayCommand(Command_ScanMemory_Click);
                Command_CleanNow = new RelayCommand(Command_CleanNow_Click);

                this.EnableCancelButton = true;
                this.EnableCleanNowButton = true;
                this.EnableCloseButton = true;
            }
        }
        #endregion

        #region commands
        public ICommand Command_UpdateVirusDefinition { get; internal set; }
        public ICommand Command_ScanForVirus { get; internal set; }
        public ICommand Command_CancelUpdate { get; internal set; }
        public ICommand Command_CloseWindow { get; internal set; }
        public ICommand Command_ScanMemory { get; internal set; }
        public ICommand Command_CleanNow { get; internal set; }
        #endregion

        #region command methods
        public void Command_UpdateVirusDefinition_Click()
        {
            this.ShowClamWinVirusUpdateWindow = true;
            CommandLogic_Clam.I.isUpdate = true;
            CommandLogic_Clam.I.LaunchUpdater();
        }

        public void Command_ScanForVirus_Click()
        {
            //MessageBox.Show("Scheduled for Sprint 3");
            //this.ShowClamWinVirusUpdateWindow = true;;
            
            CommandLogic_Clam.I.isUpdate = false;
            CommandLogic_Clam.I.LaunchCleaner();
        }

        public void Command_CancelUpdate_Click()
        {
            CommandLogic_Clam.I.CancelUpdate();
        }

        public void Command_CloseWindow_Click()
        {
            CommandLogic_Clam.I.CancelUpdate();
            this.ShowClamWinVirusUpdateWindow = false;
            this.ShowClamWinVirusScanner = false;
        }

        public void Command_ScanMemory_Click()
        {
            //this.ShowClamWinVirusUpdateWindow = true;
            this.ShowClamWinVirusScanner = true;
            CommandLogic_Clam.I.isUpdate = false;
            CommandLogic_Clam.I.LaunchScanner(SEARCH.clamscan_memory, string.Empty, true);
        }

        public void Command_CleanNow_Click()
        {
            CommandLogic_Clam.I.LaunchCleaner(true);
        }
        #endregion
    }
}
