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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;

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

        public void Command_UninstallProgram_Click()
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
                if (!SelectedProgramDetails.ProgramDetails.UninstallString.StartsWith("\"") && !SelectedProgramDetails.ProgramDetails.UninstallString.EndsWith("\""))
                    process.StandardInput.WriteLine("\""+SelectedProgramDetails.ProgramDetails.UninstallString+"\"");
                else
                    process.StandardInput.WriteLine(SelectedProgramDetails.ProgramDetails.UninstallString);
                process.StandardInput.WriteLine("exit");

                process.WaitForExit();


            }
        }


        #endregion

        #region command methods

        ProgramUninstallerWindow PUW = null;
        public void Command_ShowUninstaller_Click()
        {
            BtnUninstall = false;
            this.ShowWindow = true;
            PUW= new ProgramUninstallerWindow();
            PUW.Show();
            GetInstalledPrograms();
        }

         const string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

         public void GetInstalledPrograms()
         {
             ProgramCollection.Clear();
             GetInstalledProgramsFromRegistry(RegistryView.Registry32);
             GetInstalledProgramsFromRegistry(RegistryView.Registry64);
         }

    private void GetInstalledProgramsFromRegistry(RegistryView registryView)
    {

        using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView).OpenSubKey(registry_key))
        {
            foreach (string subkey_name in key.GetSubKeyNames())
            {
                using (RegistryKey sk = key.OpenSubKey(subkey_name))
                {
                    if (IsProgramVisible(sk))
                    {
                        var displayName = sk.GetValue("DisplayName");
                        var size = sk.GetValue("EstimatedSize",string.Empty);
                        var Publisher = sk.GetValue("Publisher", string.Empty);
                        var strUninstallString = sk.GetValue("UninstallString", string.Empty);
                        var strVersion = sk.GetValue("DisplayVersion", string.Empty);
                        if (!string.IsNullOrEmpty(Convert.ToString(displayName)) && !string.IsNullOrEmpty(Convert.ToString(strUninstallString)))
                        {
                            Model_WindowsUninstaller e = new Model_WindowsUninstaller();
                            e.ProgramDetails = new Model_Uninstaller_ProgramDetails()
                            {
                                ProgramName = displayName.ToString(),
                                EstimatedSize = size.ToString(),
                                PublisherName = Publisher.ToString(),
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
            if (PUW!=null)
                    PUW.Close();
        }

        public void Command_Refresh_Click()
        {
            GetInstalledPrograms();
            BtnUninstall = false;
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
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
