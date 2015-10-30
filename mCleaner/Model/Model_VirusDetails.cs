namespace mCleaner.Model
{
    public class Model_VirusDetails
    {
        private string _File = string.Empty;
        public string File
        {
            get { return _File; }
            set
            {
                if (_File != value)
                {
                    _File = value;
                }
            }
        }

        private string _VirusName = string.Empty;
        public string VirusName
        {
            get { return _VirusName; }
            set
            {
                if (_VirusName != value)
                {
                    _VirusName = value;
                }
            }
        }

        private int _ColWidth = -1;
        public int ColWidth
        {
            get { return _ColWidth; }
            set
            {
                if (_ColWidth != value)
                {
                    _ColWidth = value;
                }
            }
        }

        private string _Status = string.Empty;
        public string Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                }
            }
        }
    }
}
