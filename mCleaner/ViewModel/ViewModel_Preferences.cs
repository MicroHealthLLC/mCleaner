using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Properties;
using System.Windows;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_Preferences : ViewModelBase
    {
        #region vars

        #endregion

        #region properties
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

        private string _ProxyAddress = string.Empty;
        public string ProxyAddress
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
        #endregion

        #region commands
        public ICommand Command_OK { get; set; }
        public ICommand Command_CloseWindow { get; set; }
        public ICommand Command_Menu_Preferences { get; set; }
        #endregion

        #region ctor
        public ViewModel_Preferences()
        {
            if (base.IsInDesignMode)
            {
                this.ShowWindow = false;
            }
            else
            {
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                Command_Menu_Preferences = new RelayCommand(Command_Menu_Preferences_Click);

                ReadSettings();
            }
        }
        #endregion

        #region command methods
        void Command_OK_Click()
        {
            WriteSettings();
            this.ShowWindow = false;
        }
        void Command_CloseWindow_Click()
        {
            this.ShowWindow = false;
        }
        void Command_Menu_Preferences_Click()
        {
            this.ShowWindow = true;
        }
        #endregion

        #region methods
        void ReadSettings()
        {
            if (Settings.Default.ClamWin_Proxy_Address != string.Empty)
            {
                string[] proxy = Settings.Default.ClamWin_Proxy_Address.Split(':');
                this.ProxyAddress = proxy[0];
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
            this.AutoUpdateDBAtStartup = Settings.Default.ClamWin_UpdateDBAtStartup;
            this.HideIrrelevantCleaners = Settings.Default.HideIrrelevantCleaners;
        }

        void WriteSettings()
        {
            if (this.ProxyAddress != string.Empty && this.ProxyPort != string.Empty)
            {
                Settings.Default.ClamWin_Proxy_Address = this.ProxyAddress + ":" + this.ProxyPort;
            }
            else if (this.ProxyAddress != string.Empty && this.ProxyPort != string.Empty)
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

            Settings.Default.ClamWin_UpdateDBAtStartup = this.AutoUpdateDBAtStartup;
            Settings.Default.HideIrrelevantCleaners = this.HideIrrelevantCleaners;

            Settings.Default.Save();
        }
        #endregion
    }
}
