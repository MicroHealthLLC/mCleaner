using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_Clam : ViewModelBase
    {
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

        public ViewModel_Clam()
        {
            if (base.IsInDesignMode)
            {
                this.ShowClamWinVirusUpdateWindow = false;
                this.VirusDefUpdateLog = "Log goes here";
            }
            else
            {
                Command_UpdateVirusDefinition = new RelayCommand(Command_UpdateVirusDefinition_Click);
                Command_ScanForVirus = new RelayCommand(Command_ScanForVirus_Click);
                Command_CancelUpdate = new RelayCommand(Command_CancelUpdate_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                Command_ScanMemory = new RelayCommand(Command_ScanMemory_Click);
            }
        }

        public ICommand Command_UpdateVirusDefinition { get; internal set; }
        public ICommand Command_ScanForVirus { get; internal set; }
        public ICommand Command_CancelUpdate { get; internal set; }
        public ICommand Command_CloseWindow { get; internal set; }
        public ICommand Command_ScanMemory { get; internal set; }

        public void Command_UpdateVirusDefinition_Click()
        {
            this.ShowClamWinVirusUpdateWindow = true;
            CommandLogic_Clam.I.LaunchUpdater();
        }

        public void Command_ScanForVirus_Click()
        {
            MessageBox.Show("Scheduled for Sprint 3");
        }

        public void Command_CancelUpdate_Click()
        {
            CommandLogic_Clam.I.CancelUpdate();
        }

        public void Command_CloseWindow_Click()
        {
            CommandLogic_Clam.I.CancelUpdate();
            this.ShowClamWinVirusUpdateWindow = false;
        }

        public void Command_ScanMemory_Click()
        {
            //this.ShowClamWinVirusUpdateWindow = true;
            CommandLogic_Clam.I.LaunchScanner(SEARCH.clamscan_memory, string.Empty, true);
        }
    }
}
