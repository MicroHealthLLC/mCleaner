using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Model;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;

namespace mCleaner.ViewModel
{
    public class ViewModel_Uninstaller : ViewModelBase
    {
        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath,
           out ushort lpiIcon);
        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        #region properties
        private ObservableCollection<Model_WindowsUninstaller> _programCollection = new ObservableCollection<Model_WindowsUninstaller>();
        public ObservableCollection<Model_WindowsUninstaller> ProgramCollection
        {
            get { return _programCollection; }
            set
            {
                if (_programCollection != value)
                {
                    _programCollection = value;
                    base.RaisePropertyChanged("ProgramCollection");
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

        public string LblTotalPrograms
        {
            get { return "Total Installed Programs"; }
           
        } 

        private bool _btnUninstall = false;
        public bool BtnUninstall
        {
            get { return _btnUninstall; }
            set
            {
                if (_btnUninstall != value)
                {
                    _btnUninstall = value;
                    base.RaisePropertyChanged("BtnUninstall");
                }
            }
        }

        private Model_WindowsUninstaller _selectedProgramDetails = new Model_WindowsUninstaller();
        public Model_WindowsUninstaller SelectedProgramDetails
        {
            get { return _selectedProgramDetails; }
            set
            {
                if (_selectedProgramDetails != value)
                {
                    _selectedProgramDetails = value;
                    base.RaisePropertyChanged("SelectedProgramDetails");
                }
            }
        }

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        #endregion

        #region commands
        public ICommand Command_Refresh { get; internal set; }
        public ICommand Command_UninstallProgram { get; internal set; }

        public ICommand Command_ShowUninstaller { get; internal set; }

        public ICommand Command_CloseWindow { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_Uninstaller()
        {
            this.Command_ShowUninstaller = new RelayCommand(Command_ShowUninstaller_Click);
            this.Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
            this.Command_Refresh = new RelayCommand(Command_Refresh_Click);
            this.Command_UninstallProgram = new RelayCommand(Command_UninstallProgram_Click);
        }

        public void Command_UninstallProgram_Click()
        {
            if (SelectedProgramDetails != null)
            {
                bool blnIsmCleanerNeedsToClose = false;
                if (SelectedProgramDetails.ProgramDetails.ProgramName.Equals("mCleaner",StringComparison.CurrentCultureIgnoreCase))
                {
                    if (MessageBox.Show("mCleaner needs to close to be uninstalled. Do you want to continue?", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        blnIsmCleanerNeedsToClose = true;
                    }
                    else
                        return;

                }

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
                if (!SelectedProgramDetails.ProgramDetails.UninstallString.StartsWith("MsiExec.exe",StringComparison.InvariantCultureIgnoreCase) && !SelectedProgramDetails.ProgramDetails.UninstallString.StartsWith("\"") && !SelectedProgramDetails.ProgramDetails.UninstallString.EndsWith("\""))
                    process.StandardInput.WriteLine("\""+SelectedProgramDetails.ProgramDetails.UninstallString+"\"");
                else
                    process.StandardInput.WriteLine(SelectedProgramDetails.ProgramDetails.UninstallString);
                process.StandardInput.WriteLine("exit");

                process.WaitForExit();

                if(blnIsmCleanerNeedsToClose)
                    Application.Current.Shutdown();
            }
        }


        #endregion

        #region command methods

        public void Command_ShowUninstaller_Click()
        {
            BtnUninstall = false;
            this.ShowWindow = true;
            CleanerML.Run = false;
            CleanerML.ShowCleanerDescription = false;
            CleanerML.btnCleanNowPreviousState = CleanerML.btnPreviewCleanEnable;
            CleanerML.btnPreviewCleanEnable = false;
            CleanerML.btnCleaningOptionsEnable = false;
            CleanerML.ShowFrontPage = false;
           
            GetInstalledPrograms();
        }

         const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

         public void GetInstalledPrograms()
         {
             ProgramCollection.Clear();
             GetInstalledProgramsFromRegistry(RegistryView.Registry32);
             GetInstalledProgramsFromRegistry(RegistryView.Registry64);
         }

    private void GetInstalledProgramsFromRegistry(RegistryView registryView)
    {

        using (RegistryKey key = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView).OpenSubKey(RegistryKey))
        {
            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using (RegistryKey sk = key.OpenSubKey(subkeyName))
                {
                    if (IsProgramVisible(sk))
                    {
                        var displayName = sk.GetValue("DisplayName");
                        var size = sk.GetValue("EstimatedSize",string.Empty);
                        var publisher = sk.GetValue("Publisher", string.Empty);
                        var strUninstallString = sk.GetValue("UninstallString", string.Empty);
                        var strVersion = sk.GetValue("DisplayVersion", string.Empty);
                        if (!string.IsNullOrEmpty(Convert.ToString(displayName)) && !string.IsNullOrEmpty(Convert.ToString(strUninstallString)))
                        {
                            Model_WindowsUninstaller e = new Model_WindowsUninstaller();
                            e.ProgramDetails = new Model_Uninstaller_ProgramDetails()
                            {
                                ProgramName = displayName.ToString(),
                                EstimatedSize = size.ToString(),
                                PublisherName = publisher.ToString(),
                                Version = strVersion.ToString(),
                                UninstallString = strUninstallString.ToString(),
                            };
                            ProgramCollection.Add(e);
                        }
                    }
                }
            }
        }
    }

    

    private static bool IsProgramVisible(RegistryKey subkey)
    {
        var name = (string)subkey.GetValue("DisplayName");
        var releaseType = (string)subkey.GetValue("ReleaseType");
        var systemComponent = subkey.GetValue("SystemComponent");
        var parentName = (string)subkey.GetValue("ParentDisplayName");

        return
            !string.IsNullOrEmpty(name)
            && string.IsNullOrEmpty(releaseType)
            && string.IsNullOrEmpty(parentName)
            && (systemComponent == null);
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

        public void Command_Refresh_Click()
        {
            GetInstalledPrograms();
            BtnUninstall = false;
        }

        #endregion

        #region methods

        #endregion

       
    }
}
