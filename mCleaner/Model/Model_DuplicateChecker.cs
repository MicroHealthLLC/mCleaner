using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace mCleaner.Model
{
    public class Model_DuplicateChecker : ViewModelBase
    {
        private Model_DuplicateChecker_FileDetails _FileDetails = new Model_DuplicateChecker_FileDetails();
        public Model_DuplicateChecker_FileDetails FileDetails
        {
            get { return _FileDetails; }
            set
            {
                if (_FileDetails != value)
                {
                    _FileDetails = value;
                    base.RaisePropertyChanged("FileDetails");
                }
            }
        }

        private bool _Selected = false;
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    base.RaisePropertyChanged("Selected");
                }
            }
        }

        private string _Hash = string.Empty;
        public string Hash
        {
            get { return _Hash; }
            set
            {
                if (_Hash != value)
                {
                    _Hash = value;
                    base.RaisePropertyChanged("Hash");
                }
            }
        }
    }

    public class Model_DuplicateChecker_FileDetails
    {
        private string _Filename = string.Empty;
        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (_Filename != value)
                {
                    _Filename = value;
                }
            }
        }

        private string _Fullfilepath = string.Empty;
        public string Fullfilepath
        {
            get { return _Fullfilepath; }
            set
            {
                if (_Fullfilepath != value)
                {
                    _Fullfilepath = value;
                }
            }
        }

        private string _ParentDirectory = string.Empty;
        public string ParentDirectory
        {
            get { return _ParentDirectory; }
            set
            {
                if (_ParentDirectory != value)
                {
                    _ParentDirectory = value;
                }
            }
        }
    }
}
