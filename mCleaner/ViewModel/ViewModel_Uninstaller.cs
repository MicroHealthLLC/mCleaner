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
using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace mCleaner.ViewModel
{
    public class ViewModel_Uninstaller : ViewModelBase
    {
        #region properties
        private ObservableCollection<Model_WindowsUninstaller> _ProgramCollection = new ObservableCollection<Model_WindowsUninstaller>();
        public ObservableCollection<Model_WindowsUninstaller> ProgramCollection
        {
            get { return _ProgramCollection; }
            set
            {
                if (_ProgramCollection != value)
                {
                    _ProgramCollection = value;
                    base.RaisePropertyChanged("ProgramCollection");
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

        public string LblTotalPrograms
        {
            get { return "Total Installed Programs"; }
           
        } 

        private bool _BtnUninstall = false;
        public bool BtnUninstall
        {
            get { return _BtnUninstall; }
            set
            {
                if (_BtnUninstall != value)
                {
                    _BtnUninstall = value;
                    base.RaisePropertyChanged("BtnUninstall");
                }
            }
        }

        private Model_WindowsUninstaller _SelectedProgramDetails = new Model_WindowsUninstaller();
        public Model_WindowsUninstaller SelectedProgramDetails
        {
            get { return _SelectedProgramDetails; }
            set
            {
                if (_SelectedProgramDetails != value)
                {
                    _SelectedProgramDetails = value;
                    base.RaisePropertyChanged("Selected");
                }
            }
        }
        #endregion

        #region commands
        public ICommand Command_Refesh { get; internal set; }
        public ICommand Command_UninstallProgram { get; internal set; }

        public ICommand Command_ShowUninstaller { get; internal set; }

        public ICommand Command_CloseWindow { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_Uninstaller()
        {

            this.Command_ShowUninstaller = new RelayCommand(Command_ShowUninstaller_Click);
            this.Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
            this.Command_Refesh = new RelayCommand(Command_Refresh_Click);
            this.Command_UninstallProgram = new RelayCommand(Command_UninstallProgram_Click);
            

        }

        private void Command_UninstallProgram_Click()
        {
            if (SelectedProgramDetails != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };

                process.Start();
                process.StandardInput.WriteLine(SelectedProgramDetails.ProgramDetails.UninstallString);
                process.StandardInput.WriteLine("exit");

                process.WaitForExit();


            }
        }


        #endregion

        #region command methods

        public void Command_ShowUninstaller_Click()
        {
            this.ShowWindow = true;
            ListOfWindowsPorgrams();
        }

        public void ListOfWindowsPorgrams()
        {
            ProgramCollection.Clear();
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            var displayName = sk.GetValue("DisplayName");
                            var size = sk.GetValue("EstimatedSize");
                            var Publisher = sk.GetValue("Publisher",string.Empty);
                            var strUninstallString=sk.GetValue("UninstallString",string.Empty);
                           var strVersion=sk.GetValue("DisplayVersion", string.Empty); 
                            
                            if (!string.IsNullOrEmpty(Convert.ToString( displayName)) && !string.IsNullOrEmpty(Convert.ToString(strUninstallString)))
                            {
                                Model_WindowsUninstaller e = new Model_WindowsUninstaller();
                                e.ProgramDetails = new Model_Uninstaller_ProgramDetails()
                                {
                                    ProgramName = displayName.ToString(),
                                    EstimatedSize=size.ToString(),
                                    PublisherName = Publisher.ToString(),
                                    Version=strVersion.ToString(),
                                    UninstallString = strUninstallString.ToString(),
                                };
                                ProgramCollection.Add(e);
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
        }

        public void Command_CloseWindow_Click()
        {
            this.ShowWindow = false;
        }

        public void Command_Refresh_Click()
        {
            ListOfWindowsPorgrams();
            BtnUninstall = false;
        }
        #endregion

        #region methods

        #endregion

        private bool _Cancel = false;
        public bool Cancel
        {
            get { return _Cancel; }
            set
            {
                if (_Cancel != value)
                {
                    _Cancel = value;
                }
            }
        }
    }
}
