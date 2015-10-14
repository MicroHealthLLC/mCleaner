using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace mCleaner.Model
{
    public class Model_Shred : ViewModelBase
    {
        private string _FilePath = string.Empty;
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                if (_FilePath != value)
                {
                    _FilePath = value;
                }
            }
        }
    }

}
