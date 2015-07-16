using System.Collections.Generic;

namespace mCleaner.Model
{
    public class Model_DuplicateChecker
    {
        private List<string> _DuplicateFiles = new List<string>();
        public List<string> DuplicateFiles
        {
            get { return _DuplicateFiles; }
            set
            {
                if (_DuplicateFiles != value)
                {
                    _DuplicateFiles = value;
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

        private string _Hash = string.Empty;
        public string Hash
        {
            get { return _Hash; }
            set
            {
                if (_Hash != value)
                {
                    _Hash = value;
                }
            }
        }
    }
}
