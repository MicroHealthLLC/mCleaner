using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace mCleaner.Model
{
    public class Model_WindowsUninstaller : ViewModelBase
    {
        private Model_Uninstaller_ProgramDetails _ProgramDetails = new Model_Uninstaller_ProgramDetails();
        public Model_Uninstaller_ProgramDetails ProgramDetails
        {
            get { return _ProgramDetails; }
            set
            {
                if (_ProgramDetails != value)
                {
                    _ProgramDetails = value;
                    base.RaisePropertyChanged("ProgramDetails");
                }
            }
        }
    }
    
    public class Model_Uninstaller_ProgramDetails
    {
        private string _ProgramName = string.Empty;
        public string ProgramName
        {
            get { return _ProgramName; }
            set
            {
                if (_ProgramName != value)
                {
                    _ProgramName = value;
                }
            }
        }

        private BitmapImage _IconImage = null;
        public BitmapImage IconImage
        {
            get { return _IconImage; }
            set
            {
                if (_IconImage != value)
                {
                    _IconImage = value;
                }
            }
        }
        

        private string _PublisherName = string.Empty;
        public string PublisherName
        {
            get { return _PublisherName; }
            set
            {
                if (_PublisherName != value)
                {
                    _PublisherName = value;
                }
            }
        }

        private string _EstimatedSize = string.Empty;
        public string EstimatedSize
        {
            get { return _EstimatedSize; }
            set
            {
                if (_EstimatedSize != value)
                {
                    _EstimatedSize = value;
                }
            }
        }

        private string _UninstallString = string.Empty;
        public string UninstallString
        {
            get { return _UninstallString; }
            set
            {
                if (_UninstallString != value)
                {
                    _UninstallString = value;
                }
            }
        }

        private string _Version = string.Empty;
        public string Version
        {
            get { return _Version; }
            set
            {
                if (_Version != value)
                {
                    _Version = value;
                }
            }
        }
    }
}
