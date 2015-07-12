using mCleaner.Logics.Enumerations;

namespace mCleaner.Model
{
    public class Model_ThingsToDelete
    {
        private string _Name = string.Empty;
        public string FullPathName
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                }
            }
        }

        private THINGS_TO_DELETE _WhatKind = THINGS_TO_DELETE.file;
        public THINGS_TO_DELETE WhatKind
        {
            get { return _WhatKind; }
            set
            {
                if (_WhatKind != value)
                {
                    _WhatKind = value;
                }
            }
        }

        private bool _OverWrite = false;
        public bool OverWrite
        {
            get { return _OverWrite; }
            set
            {
                if (_OverWrite != value)
                {
                    _OverWrite = value;
                }
            }
        }

        private bool _IsWhitelisted = false;
        public bool IsWhitelisted
        {
            get { return _IsWhitelisted; }
            set
            {
                if (_IsWhitelisted != value)
                {
                    _IsWhitelisted = value;
                }
            }
        }

        private COMMANDS _command = COMMANDS.none;
        public COMMANDS command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    _command = value;
                }
            }
        }

        private SEARCH _search = SEARCH.none;
        public SEARCH search
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    _search = value;
                }
            }
        }

        /// <summary>
        /// Parent directory of the file. 
        /// </summary>
        public string path { get; set; }

        // level of cleaning
        // 0 - safe
        // 1 - moderate
        // 2 - aggressive
        // when no level attribute, the default value is aggressive. Default cleaning option should be safe
        public int level { get; set; }

        public string cleaner_name { get; set; }

        #region for registry
        public string reg_root { get; set; }
        public string reg_subkey { get; set; }
        public string reg_name { get; set; }
        public string reg_name_val { get; set; }
        #endregion

        #region for .INI files
        public string section { get; set; }
        public string key { get; set; }
        #endregion

        #region for JSON files
        public string address { get; set; }
        #endregion

        #region for clamwin
        public string regex { get; set; }
        #endregion
    }
}
