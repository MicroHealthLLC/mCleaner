using GalaSoft.MvvmLight;

namespace mCleaner.Model
{
    public class Model_RestoreRegistry : ViewModelBase
    {
        private string _RegistryOption = string.Empty;
        public string RegistryOption
        {
            get { return _RegistryOption; }
            set
            {
                if (_RegistryOption != value)
                {
                    _RegistryOption = value;
                }
            }
        }

        private string _FullFilePath = string.Empty;
        public string FullFilePath
        {
            get { return _FullFilePath; }
            set
            {
                if (_FullFilePath != value)
                {
                    _FullFilePath = value;
                }
            }
        }


    }

}
