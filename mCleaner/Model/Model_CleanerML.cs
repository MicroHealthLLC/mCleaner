using System.Collections.Generic;
using System.Xml.Serialization;

namespace mCleaner.Model
{
    public class Model_CleanerML
    {
        #region Properties
        private cleaner _CleanerML = new cleaner();
        public cleaner CleanerML
        {
            get { return _CleanerML; }
            set
            {
                if (_CleanerML != value)
                {
                    _CleanerML = CleanUp(value);

                    isSupported = CheckIfSupported(_CleanerML);
                }
            }
        }

        private bool _isSupported = false;
        public bool isSupported
        {
            get { return _isSupported; }
            set
            {
                if (_isSupported != value)
                {
                    _isSupported = value;
                }
            }
        }
        #endregion

        #region Methods
        // since we convert linuxy paths to empty strings, we need to remove them in action lists.
        cleaner CleanUp(cleaner c)
        {
            cleaner ret = c;
            List<action> actions_to_remove = new List<action>();

            foreach (option o in c.option)
            {
                foreach (action a in o.action)
                {
                    if (a.path == string.Empty) actions_to_remove.Add(a);
                }

                foreach (action a in actions_to_remove)
                {
                    o.action.Remove(a);
                }
            }

            return ret;
        }

        bool CheckIfSupported(cleaner c)
        {
            bool ret;

            // quickly check if this cleaner is for windows
            ret = (c.os == "windows" || c.os == null);

            // if os is null, then check for possible windowy actions
            if (c.os == null)
            {
                foreach (option o in c.option)
                {
                    ret = o.action.Count == 0 ? false : true;
                }
            }

            return ret;
        }
        #endregion
    }

    [XmlRoot("cleaner")]
    public class cleaner
    {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("os")]
        public string os { get; set; }

        [XmlElement("label")]
        public string label { get; set; }

        [XmlElement("running")]
        public List<running> running { get; set; }

        [XmlElement("description")]
        public string description { get; set; }

        [XmlElement("option")]
        public List<option> option { get; set; }
    }

    [XmlRoot("running")]
    public class running
    {

        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlText]
        public string text { get; set; }
    }

    [XmlRoot("option")]
    public class option
    {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlElement("label")]
        public string label { get; set; }

        [XmlElement("description")]
        public string description { get; set; }

        [XmlElement("warning")]
        public string warning { get; set; }

        [XmlElement("action")]
        public List<action> action { get; set; }

        [XmlAttribute("level")]
        // level of cleaning
        // 0 - safe
        // 1 - moderate
        // 2 - aggressive
        // when no level attribute, the default value is aggressive. Default cleaning option should be safe
        /// <summary>
        /// 
        /// </summary>
        public int level { get; set; }

        [XmlIgnore]
        public cleaner parent_cleaner { get; set; }
    }

    [XmlRoot("action")]
    public class action
    {
        [XmlAttribute("command")]
        public string command { get; set; }

        [XmlAttribute("search")]
        public string search { get; set; }

        [XmlAttribute("regex")]
        public string regex { get; set; }

        string _path = null;
        [XmlAttribute("path")]
        public string path
        {
            get { return _path; }
            set
            {
                if (value[0] == '$')
                {
                    _path = mCleaner.Helpers.FileOperations.I.GetSpecialFolderPath(value);
                }
                else if (value.Substring(0, 1) == "~" || value.Substring(0, 1) == "/") { 
                    if ( // we do not need linux paths
                        value.Substring(0, 2) == "~/" ||
                        value.Substring(0, 4) == "/var" ||
                        value.Substring(0, 4) == "/tmp" ||
                        value.Substring(0, 4) == "/dev"
                    )
                    {
                        _path = string.Empty;
                    }
                }
                else
                {
                    _path = value;
                }
            }
        }

        [XmlAttribute("cache")]
        public string cache { get; set; }

        /// <summary>
        /// used for registry only
        /// </summary>
        [XmlAttribute("name")]
        public string name { get; set; }

        /// <summary>
        /// used for json only
        /// </summary>
        [XmlAttribute("address")]
        public string address { get; set; }

        /// <summary>
        /// used for ini only
        /// </summary>
        [XmlAttribute("section")]
        public string section { get; set; }

        /// <summary>
        /// used for ini only
        /// </summary>
        [XmlAttribute("parameter")]
        public string parameter { get; set; }

        // level of cleaning
        // 0 - safe
        // 1 - moderate
        // 2 - aggressive
        // when no level attribute, the default value is aggressive. Default cleaning option should be safe
        /// <summary>
        /// 
        /// </summary>
        //[XmlAttribute("level")]
        //public int level { get; set; }

        [XmlIgnore]
        public option parent_option { get; set; }
    }
}