using GalaSoft.MvvmLight;

namespace mCleaner.Model
{
    public class Model_DuplicateChecker : ViewModelBase
    {
        private Model_DuplicateChecker_FileDetails _fileDetails = new Model_DuplicateChecker_FileDetails();
        public Model_DuplicateChecker_FileDetails FileDetails
        {
            get { return _fileDetails; }
            set
            {
                if (_fileDetails != value)
                {
                    _fileDetails = value;
                    base.RaisePropertyChanged("FileDetails");
                }
            }
        }

        private bool _selected = false;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    base.RaisePropertyChanged("Selected");
                }
            }
        }

        private string _hash = string.Empty;
        public string Hash
        {
            get { return _hash; }
            set
            {
                if (_hash != value)
                {
                    _hash = value;
                    base.RaisePropertyChanged("Hash");
                }
            }
        }
    }

    public class Model_DuplicateChecker_FileDetails
    {
        private string _filename = string.Empty;
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private string _fullfilepath = string.Empty;
        public string Fullfilepath
        {
            get { return _fullfilepath; }
            set { _fullfilepath = value; }
        }

        private string _parentDirectory = string.Empty;
        public string ParentDirectory
        {
            get { return _parentDirectory; }
            set { _parentDirectory = value; }
        }
    }
}
