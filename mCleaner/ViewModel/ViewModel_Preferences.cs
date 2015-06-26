using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_Preferences : ViewModelBase
    {
        #region vars

        #endregion

        #region preferences
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

        }

        void WriteSettings()
        {

        }
        #endregion
    }
}
