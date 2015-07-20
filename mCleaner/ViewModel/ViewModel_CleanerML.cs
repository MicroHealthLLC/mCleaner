using CodeBureau;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using mCleaner.Helpers.Controls;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;

namespace mCleaner.ViewModel
{
    public delegate void OnTreeNodeSelected(object sender, EventArgs e);
    public class ViewModel_CleanerML : ViewModelBase
    {
        public event OnTreeNodeSelected TreeNodeSelected;

        string _exec_path = string.Empty;

        List<TreeNode> _nodes = new List<TreeNode>();

        #region properties
        private ObservableCollection<TreeNode> _CleanersCollection = new ObservableCollection<TreeNode>();
        public ObservableCollection<TreeNode> CleanersCollection
        {
            get { return _CleanersCollection; }
            set
            {
                if (_CleanersCollection != value)
                {
                    _CleanersCollection = value;
                    base.RaisePropertyChanged("CleanersCollection");
                }
            }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }

        private bool _Run = false;
        public bool Run
        {
            get { return _Run; }
            set
            {
                if (_Run != value)
                {
                    _Run = value;
                    base.RaisePropertyChanged("Run");
                }
            }
        }

        private string _TextLog = string.Empty;
        public string TextLog
        {
            get { return _TextLog; }
            set
            {
                if (_TextLog != value)
                {
                    _TextLog = value + "\r\n";
                    base.RaisePropertyChanged("TextLog");
                }
            }
        }

        private bool _ProgressIsIndeterminate = false;
        public bool ProgressIsIndeterminate
        {
            get { return _ProgressIsIndeterminate; }
            set
            {
                if (_ProgressIsIndeterminate != value)
                {
                    _ProgressIsIndeterminate = value;
                    base.RaisePropertyChanged("ProgressIsIndefinite");
                }
            }
        }

        private int _MaxProgress = 0;
        public int MaxProgress
        {
            get { return _MaxProgress; }
            set
            {
                if (_MaxProgress != value)
                {
                    _MaxProgress = value;
                    base.RaisePropertyChanged("MaxProgress");
                }
            }
        }

        private int _ProgressIndex = 0;
        public int ProgressIndex
        {
            get { return _ProgressIndex; }
            set
            {
                if (_ProgressIndex != value)
                {
                    _ProgressIndex = value;
                    base.RaisePropertyChanged("ProgressIndex");
                }
            }
        }

        private string _ProgressText = string.Empty;
        public string ProgressText
        {
            get { return _ProgressText; }
            set
            {
                if (_ProgressText != value)
                {
                    _ProgressText = value;
                    base.RaisePropertyChanged("ProgressText");
                }
            }
        }

        private bool _CleanOption_Safe = false;
        public bool CleanOption_Safe
        {
            get { return _CleanOption_Safe; }
            set
            {
                if (_CleanOption_Safe != value)
                {
                    _CleanOption_Safe = value;
                    base.RaisePropertyChanged("CleanOption_Safe");
                }
            }
        }

        private bool _CleanOption_Moderate = false;
        public bool CleanOption_Moderate
        {
            get { return _CleanOption_Moderate; }
            set
            {
                if (_CleanOption_Moderate != value)
                {
                    _CleanOption_Moderate = value;
                    base.RaisePropertyChanged("CleanOption_Moderate");
                }
            }
        }

        private bool _CleanOption_Aggressive = false;
        public bool CleanOption_Aggressive
        {
            get { return _CleanOption_Aggressive; }
            set
            {
                if (_CleanOption_Aggressive != value)
                {
                    _CleanOption_Aggressive = value;
                    base.RaisePropertyChanged("CleanOption_Aggressive");
                }
            }
        }

        private int _SelectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (_SelectedTabIndex != value)
                {
                    _SelectedTabIndex = value;
                    base.RaisePropertyChanged("SelectedTabIndex");
                }
            }
        }
        #endregion

        #region ctor
        public ViewModel_CleanerML()
        {
            _exec_path = System.AppDomain.CurrentDomain.BaseDirectory;
            GetCleaners();

            Run = false;

            if (base.IsInDesignMode)
            {
                Run = true;
                ProgressText = "Status goes here";
                this.ProgressIsIndeterminate = true;
            }
            else
            {
                CleanOption_Safe = Properties.Settings.Default.CleanOption == 1 ? true : false;
                CleanOption_Moderate = Properties.Settings.Default.CleanOption == 2 ? true : false;
                CleanOption_Aggressive = Properties.Settings.Default.CleanOption == 3 ? true : false;

                Command_Preview = new RelayCommand(Command_Preview_Click);
                Command_Clean = new RelayCommand<string>(Command_Clean_Click);
                Command_Quit = new RelayCommand(Command_Quit_Click);
                //Command_CleanOption = new RelayCommand<string>(new Action<string>((i) =>
                //{
                //    /*
                //     * Safe
                //       Safe removes temp files, keeps settings, preferences and passwords, cleans for malware.  
                //     * Doesn't clean registry

                //       Moderate
                //       Recommended does all of the above plus cleans registry.  
                //     * Instead of calling it recommended call it Moderate

                //       Aggressive
                //       Aggressive cleans every option including preferences, forms and passwords.
                //     */

                //    CleanOption_Safe = false;
                //    CleanOption_Moderate = false;
                //    CleanOption_Aggressive = false;

                //    CleanOption_Safe = i == "1" ? true : false;
                //    CleanOption_Moderate = i == "2" ? true : false;
                //    CleanOption_Aggressive = i == "3" ? true : false;

                //    Properties.Settings.Default.CleanOption = int.Parse(i);
                //    Properties.Settings.Default.Save();
                //}));
            }
        }
        #endregion

        #region commands
        public ICommand Command_Quit { get; set; }
        public ICommand Command_Preview { get; internal set; }
        public ICommand Command_Clean { get; internal set; }
        public ICommand Command_CleanOption { get; internal set; }
        #endregion

        #region command methods
        public async void Command_Preview_Click()
        {
            Worker.I.Preview = true;
            this.Run = true;
            this.ProgressIsIndeterminate = true;

            await Start();

            await Worker.I.PreviewWork();
        }

        public async void Command_Clean_Click(string i)
        {
            CleanOption_Safe = false;
            CleanOption_Moderate = false;
            CleanOption_Aggressive = false;

            CleanOption_Safe = i == "1" ? true : false;
            CleanOption_Moderate = i == "2" ? true : false;
            CleanOption_Aggressive = i == "3" ? true : false;

            Properties.Settings.Default.CleanOption = int.Parse(i);
            Properties.Settings.Default.Save();

            Worker.I.Preview = false;
            this.Run = true;

            await Start();
            Worker.I.DoWork();
        }

        public void Command_Quit_Click()
        {
            // Disable needed privileges
            Permissions.SetPrivileges(false);
            Process.GetCurrentProcess().Kill();
        }
        #endregion

        #region Events
        void TreeNode_TreeNodeSelected(object sender, EventArgs e)
        {
            TreeNode root = sender as TreeNode;
            
            //if (root.Parent != null)
            {
                if (TreeNodeSelected != null)
                {
                    TreeNodeSelected(root, new EventArgs());
                }
            }

            if (root.Key.Contains("duplicate_checker"))
            {
                if (DupChecker.DupplicateCollection.Count > 0)
                {
                    SelectedTabIndex = 1;
                }
                else
                {
                    SelectedTabIndex = 0;
                }
            }
            else
            {
                SelectedTabIndex = 0;
            }
        }

        void TeeNode_TreeNodeChecked(object sender, EventArgs e)
        {
            TreeNode root = sender as TreeNode;

            if (root.Tag is option)
            {
                option o = (option)root.Tag;

                if (root.IsChecked.Value)
                {
                    if (o.warning != string.Empty && o.warning != null)
                    {
                        MessageBox.Show("Warning!\r\n" + o.warning, "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else if (root.Tag is cleaner)
            {

            }

            if (root.Key.Contains("duplicate_checker"))
            {
                SelectedTabIndex = 1;
            }
            else
            {
                SelectedTabIndex = 0;
            }
        }
        #endregion

        #region Methods
        public ObservableCollection<TreeNode> GetCleaners()
        {
            if (base.IsInDesignMode)
            {
                #region designer data
                TreeNode root = new TreeNode("Chromium")
                {
                    IsInitiallySelected = true,
                    IsAccordionHeader = true,
                    Tag = 1,
                    //Children =
                    //{
                    //    new TreeNode("Cache") {},
                    //    new TreeNode("Cookies") {}
                    //}
                };
                root.Initialize();
                this.CleanersCollection.Add(root);

                TreeNode root2 = new TreeNode("Windows Explorer")
                {
                    IsAccordionHeader = true,
                    Tag = 1,
                    Children =
                    {
                        new TreeNode("Most recently used") {},
                        new TreeNode("Recent documents list") {},
                        new TreeNode("Run") {},
                        new TreeNode("Search history") {},
                        new TreeNode("Thumbnails") {},
                    }
                };
                root2.Initialize();
                this.CleanersCollection.Add(root2);
                #endregion
            }
            else
            {
                XmlSerializer srlzr = new XmlSerializer(typeof(cleaner));
                Model_CleanerML CleanerML = new Model_CleanerML();

                string cleaners_folder = Path.Combine(this._exec_path, "cleaners");
                string[] files = Directory.GetFiles(cleaners_folder, "*.xml");
                Array.Sort(files);
                this._nodes.Clear();
                
                foreach (string filename in files)
                {
                    TreeNode root = new TreeNode();

                    FileInfo fi = new FileInfo(filename);
                    using (StreamReader reader = fi.OpenText())
                    {
                        cleaner clnr = (cleaner)srlzr.Deserialize(reader);
                        CleanerML.CleanerML = clnr;

                        // cleaner files are initially verified after deserialization
                        bool isSupported = CleanerML.isSupported;
                        Debug.WriteLine(clnr.label + "> " + isSupported.ToString());

                        if (isSupported)
                        {
                            if (Settings.Default.HideIrrelevantCleaners)
                            {
                                // further check if the current cleaner is executable.

                                // though check if it has a slow option so skip the precheck
                                // and just include it automatically
                                bool skipprecheck = false;
                                foreach (option o in clnr.option)
                                {
                                    if (o.warning != null && o.warning != string.Empty)
                                    {
                                        skipprecheck = true;
                                        break;
                                    }
                                }

                                if (skipprecheck)
                                {
                                    isSupported = true;
                                }
                                else
                                {
                                    isSupported = CheckIfSupported(clnr);
                                }
                            }
                        }

                        if (isSupported)
                        {
                            root = new TreeNode(clnr.label, clnr.id);
                            //root.IsInitiallySelected = true;
                            //root.IsChecked = true;
                            root.Tag = clnr;
                            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
                            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
                            root.IsAccordionHeader = true;
                            //root.IsExpanded = false;
                            root.Children = new List<TreeNode>();

                            foreach (option o in clnr.option)
                            {
                                o.parent_cleaner = clnr;

                                TreeNode child = new TreeNode(o.label, o.id);
                                child.Tag = o;
                                child.TreeNodeChecked += TeeNode_TreeNodeChecked;
                                child.TreeNodeSelected += TreeNode_TreeNodeSelected;
                                child.Initialize();

                                root.Children.Add(child);

                                foreach (action a in o.action)
                                {
                                    a.parent_option = o;
                                }
                            }

                            root.Initialize();
                            this._nodes.Add(root);
                        }
                    }
                }

                AddSystemCleaner();
                AddDuplicateCleaner();
            }

            SortCleanerCollection();

            return this.CleanersCollection;
        }

        void SortCleanerCollection()
        {
            this._nodes = this._nodes.OrderBy(p => p.Key).ToList();
            this.CleanersCollection.Clear();
            foreach (TreeNode t in this._nodes)
            {
                this.CleanersCollection.Add(t);
            }
        }

        bool CheckIfSupported(cleaner c)
        {
            bool res = false;

            // just clear TTD and do not reset vars as other controls
            // in window will trigger.
            Worker.I.ClearTTD(false);

            foreach (option o in c.option)
            {
                o.parent_cleaner = c;

                foreach (action a in o.action)
                {
                    a.parent_option = o;
                }

                ExecuteOption(o);

                // do we have a new entry in TTD?
                int total_queued_work = Worker.I.TTD.Count;

                if (total_queued_work > 0)
                {
                    res = true;
                }
            }

            return res;
        }

        public FlowDocument BuildCleanerDetails(cleaner c)
        {
            FlowDocument doc = new FlowDocument();

            Paragraph p = new Paragraph();

            // header
            Run label = new Run();
            label.Text = c.label;
            label.FontSize = 20;
            Bold label_bold = new Bold(label);
            p.Inlines.Add(label_bold);
            p.Inlines.Add(new LineBreak());

            // cleaner description
            p.Inlines.Add(new Run(c.description));
            p.Inlines.Add(new LineBreak());
            p.Inlines.Add(new LineBreak());
            p.Inlines.Add(new LineBreak());

            // cleaner options
            foreach (option o in c.option)
            {
                label = new Run();
                label.Text = o.label + ": ";
                //label.FontSize = 20;
                label_bold = new Bold(label);
                p.Inlines.Add(label_bold);
                p.Inlines.Add(new Run(o.description));
                p.Inlines.Add(new LineBreak());
                p.Inlines.Add(new LineBreak());
            }

            doc.Blocks.Add(p);

            return doc;
        }

        public async Task<bool> Start()
        {
            Worker.I.ClearTTD(); // reset TTD content
            this.TextLog = string.Empty;

            foreach (TreeNode node in this.CleanersCollection)
            {
                foreach (TreeNode child in node.Children)
                {
                    if (child.IsChecked != null)
                    {
                        if (child.IsChecked.Value)
                        {
                            // child's tag has option object
                            option o = (option)child.Tag;

                            this.ProgressIsIndeterminate = true;
                            ProgressWorker.I.EnQ("Please wait. " + (Worker.I.Preview ? "Previewing" : "Working on") + " " + o.parent_cleaner.label);

                            await Task.Run(() => ExecuteOption(o));
                        }
                    }
                }
            }

            return true;

            //AddCustomLocationsToTTD();
        }

        public void ExecuteOption(option o)
        {
            string last_Log = string.Empty;

            foreach (action _a in o.action)
            {
                if (!Worker.I.Preview) // check only when not in preview mode
                {
                    #region // check cleaning level
                    int level = 3; // let it be the default
                    int curlevel = Properties.Settings.Default.CleanOption;

                    if (_a.level == 0)
                    {
                        level = 3;
                    }
                    else
                    {
                        level = _a.level;
                    }

                    if (level > curlevel)
                    {
                        // do not execute the cleaner when the level set is greater than
                        // what is in current setting.

                        string level_name = "Aggressive";
                        if (level == 1) level_name = "Safe";
                        else if (level == 2) level_name = "Moderate";

                        string text = string.Format("\"{0}\" cleaner skipped because it is set for {1} cleaning level", _a.parent_option.label, level_name);

                        if (last_Log != text)
                        {
                            last_Log = text;

                            this.TextLog += text;
                        }

                        continue;
                    }
                    #endregion
                }

                Console.WriteLine("Executing '{0}' action from '{3}' option in '{4}' cleaner, command with '{1}' search parameter in '{2}' path", _a.command, _a.search, _a.path, _a.parent_option.label, _a.parent_option.parent_cleaner.label);

                COMMANDS cmd = (COMMANDS)StringEnum.Parse(typeof(COMMANDS), _a.command);

                iActions axn = null;

                switch (cmd)
                {
                    case COMMANDS.delete:
                        axn = new CommandLogic_Delete();
                        break;

                    #region // special commands
                    case COMMANDS.sqlite_vacuum:
                        axn = new CommandLogic_SQLiteVacuum();
                        break;
                    case COMMANDS.truncate:
                        break;
                    case COMMANDS.winreg:
                        axn = new CommandLogic_Winreg();
                        break;
                    #endregion

                    #region // other special commands
                    case COMMANDS.json:
                        axn = new CommandLogic_JSON();
                        break;
                    case COMMANDS.ini:
                        axn = new CommandLogic_Ini();
                        break;
                    #endregion

                    #region // special commands for Google Chrome
                    case COMMANDS.chrome_autofill:
                        axn = new CommandLogic_Chrome();
                        break;
                    case COMMANDS.chrome_database_db:
                        axn = new CommandLogic_Chrome();
                        break;
                    case COMMANDS.chrome_favicons:
                        axn = new CommandLogic_Chrome();
                        break;
                    case COMMANDS.chrome_history:
                        axn = new CommandLogic_Chrome();
                        break;
                    case COMMANDS.chrome_keywords:
                        axn = new CommandLogic_Chrome();
                        break;
                    #endregion

                    #region // ClamWin commands
                    case COMMANDS.clamscan:
                        axn = new CommandLogic_Clam();
                        break;
                    #endregion

                    #region // little registry commands
                    case COMMANDS.littleregistry:
                        axn = new CommandLogic_LittleRegistryCleaner();
                        break;
                    #endregion

                    case COMMANDS.clipboard:
                        axn = new CommandLogic_Clipboard();
                        break;

                    case COMMANDS.dupchecker:
                        axn = new CommandLogic_DuplicateChecker();
                        break;
                }

                if (axn != null)
                {
                    axn.Action = _a;
                    axn.Execute(); // execute for queueing 
                }
            }
        }

        #region System cleaner 
        public void RefreshSystemCleaners()
        {
            List<TreeNode> nodes_to_remove = new List<TreeNode>();

            foreach (TreeNode node in this._nodes)
            {
                if (node.Key == "system" || node.Key == "duplicate_checker")
                {
                    nodes_to_remove.Add(node);
                }
            }

            nodes_to_remove.ForEach((x) => this._nodes.Remove(x));

            nodes_to_remove.Clear();
            nodes_to_remove = null;

            AddSystemCleaner();
            AddDuplicateCleaner();

            SortCleanerCollection();
        }

        void AddDuplicateCleaner(bool select = false)
        {
            cleaner c = new cleaner()
            {
                id = "duplicate_checker",
                label = "Duplicate Checker",
                description = "Checks for duplicate in your file system",
                option = new List<option>()
            };

            TreeNode root = new TreeNode();
            root = new TreeNode(c.label, c.id);
            root.IsAccordionHeader = true;
            //root.IsExpanded = false;
            root.Tag = c;
            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
            root.Children = new List<TreeNode>();

            // add subtree
            c.option.AddRange(AddDuplicateCheckerCleaner());

            foreach (option o in c.option)
            {
                o.parent_cleaner = c;

                TreeNode child = new TreeNode(o.label, o.id);
                child.Tag = o;
                child.TreeNodeChecked += TeeNode_TreeNodeChecked;
                child.TreeNodeSelected += TreeNode_TreeNodeSelected;
                child.Initialize();

                root.Children.Add(child);

                foreach (action a in o.action)
                {
                    a.parent_option = o;
                }
            }

            root.Initialize();
            root.IsInitiallySelected = select;
            this._nodes.Add(root);
        }

        void AddSystemCleaner(bool select = false)
        {
            cleaner c = new cleaner()
            {
                id = "system",
                label = "System",
                description = "The system in general",
                option = new List<option>()
            };

            TreeNode root = new TreeNode();
            root = new TreeNode(c.label, c.id);
            root.IsAccordionHeader = true;
            //root.IsExpanded = false;
            root.Tag = c;
            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
            root.Children = new List<TreeNode>();

            c.option.Add(AddClipboardCleaner());
            c.option.Add(AddClamAVCustomLocationsToTTD());
            c.option.Add(AddCustomLocationsToTTD());
            //c.option.Add(AddDuplicateCheckerCleaner());
            c.option.Add(AddWindowsLogsCleaner());
            c.option.Add(AddMemoryDumpCleaner());
            c.option.Add(AddMUICacheCleaner());
            c.option.Add(AddPrefetchCleaner());
            c.option.Add(AddRecycleBinCleaner());
            c.option.Add(AddTemporaryFilesCleaner());
            c.option.Add(AddUpdateUninstallersCleaner());

            foreach (option o in c.option)
            {
                o.parent_cleaner = c;

                TreeNode child = new TreeNode(o.label, o.id);
                child.Tag = o;
                child.TreeNodeChecked += TeeNode_TreeNodeChecked;
                child.TreeNodeSelected += TreeNode_TreeNodeSelected;
                child.Initialize();

                root.Children.Add(child);

                foreach (action a in o.action)
                {
                    a.parent_option = o;
                }
            }

            root.Initialize();
            root.IsInitiallySelected = select;
            this._nodes.Add(root);
        }

        option AddCustomLocationsToTTD()
        {
            option o = new option()
            {
                id = "custom_locations",
                label = "Custom Location",
                description = "Delete user-specified files and folders",
                action = new List<action>()
            };

            if (mCleaner.Properties.Settings.Default.CustomLocationForDeletion != null)
            {
                foreach (string filepath in mCleaner.Properties.Settings.Default.CustomLocationForDeletion)
                {
                    if (File.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "delete",
                            search = "file",
                            path = filepath,
                            level = 1,
                            parent_option = o
                        });
                    }
                    else if (Directory.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "delete",
                            search = "walk.all",
                            path = filepath,
                            level = 1,
                            parent_option = o
                        });
                    }
                }
            }

            return o;
        }

        option AddClamAVCustomLocationsToTTD()
        {
            option o = new option()
            {
                id = "clamav_custom_locations",
                label = "ClamAV Custom Location",
                description = "Scan user-specified files and folders",
                action = new List<action>()
            };

            if (mCleaner.Properties.Settings.Default.ClamWin_ScanLocations != null)
            {
                foreach (string filepath in mCleaner.Properties.Settings.Default.ClamWin_ScanLocations)
                {
                    if (File.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "clamscan",
                            search = "clamscan.file",
                            path = filepath,
                            parent_option = o
                        });
                    }
                    else if (Directory.Exists(filepath))
                    {
                        o.action.Add(new action()
                        {
                            command = "clamscan",
                            search = "clamscan.folder",
                            path = filepath,
                            parent_option = o
                        });
                    }
                }
            }

            return o;
        }

        option AddClipboardCleaner()
        {
            option o = new option()
            {
                id = "clipboard",
                label = "Clipboard",
                description = "The desktop environment's clipboard used for copy and paste operations",
                action = new List<action>()
            };

            o.action.Add(new action()
            {
                command = "clipboard",
                search = "clipboard.clear",
                level = 1,
                parent_option = o
            });

            return o;
        }

        option AddWindowsLogsCleaner()
        {
            option o = new option()
            {
                id = "windows_logs",
                label = "Windows Logs",
                description = "Delete the logs",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$ALLUSERSPROFILE\\Application Data\\Microsoft\\Dr Watson\\*.log",
                "$ALLUSERSPROFILE\\Application Data\\Microsoft\\Dr Watson\\user.dmp",
                "$LocalAppData\\Microsoft\\Windows\\WER\\ReportArchive\\*\\*",
                "$LocalAppData\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*",
                "$programdata\\Microsoft\\Windows\\WER\\ReportArchive\\*\\*",
                "$programdata\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*",
                "$localappdata\\Microsoft\\Internet Explorer\\brndlog.bak",
                "$localappdata\\Microsoft\\Internet Explorer\\brndlog.txt",
                "$windir\\*.log",
                "$windir\\imsins.BAK",
                "$windir\\OEWABLog.txt",
                "$windir\\SchedLgU.txt",
                "$windir\\ntbtlog.txt",
                "$windir\\setuplog.txt",
                "$windir\\REGLOCS.OLD",
                "$windir\\Debug\\*.log",
                "$windir\\Debug\\Setup\\UpdSh.log",
                "$windir\\Debug\\UserMode\\*.log",
                "$windir\\Debug\\UserMode\\ChkAcc.bak",
                "$windir\\Debug\\UserMode\\userenv.bak",
                "$windir\\Microsoft.NET\\Framework\\*\\*.log",
                "$windir\\pchealth\\helpctr\\Logs\\hcupdate.log",
                "$windir\\security\\logs\\*.log",
                "$windir\\security\\logs\\*.old",
                "$windir\\system32\\TZLog.log",
                "$windir\\system32\\config\\systemprofile\\Application Data\\Microsoft\\Internet Explorer\\brndlog.bak",
                "$windir\\system32\\config\\systemprofile\\Application Data\\Microsoft\\Internet Explorer\\brndlog.txt",
                "$windir\\system32\\LogFiles\\AIT\\AitEventLog.etl.???",
                "$windir\\system32\\LogFiles\\Firewall\\pfirewall.log*",
                "$windir\\system32\\LogFiles\\Scm\\SCM.EVM*",
                "$windir\\system32\\LogFiles\\WMI\\Terminal*.etl",
                "$windir\\system32\\LogFiles\\WMI\\RTBackup\\EtwRT.*etl",
                "$windir\\system32\\wbem\\Logs\\*.lo_",
                "$windir\\system32\\wbem\\Logs\\*.log"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "glob",
                    path = path,
                    parent_option = o,
                    level = 1
                });
            }

            return o;
        }

        option AddTemporaryFilesCleaner()
        {
            option o = new option()
            {
                id = "windows_temp_files",
                label = "Temporary Files",
                description = "Delete the temporary files",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$USERPROFILE\\Local Settings\\Temp\\",
                "$windir\\temp\\"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.all",
                    path = path,
                    parent_option = o,
                    level = 1
                });
            }

            return o;
        }

        option AddMemoryDumpCleaner()
        {
            option o = new option()
            {
                id = "windows_memory_dump",
                label = "Memory Dump",
                description = "Delete the file memory.dmp",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\memory.dmp",
                "$windir\\Minidump\\*.dmp"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o,
                    level = 2
                });
            }

            return o;
        }

        option AddMUICacheCleaner()
        {
            option o = new option()
            {
                id = "windows_muicache",
                label = "MUICache",
                description = "Delete the cache",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "HKCU\\Software\\Microsoft\\Windows\\ShellNoRoam\\MUICache",
                "HKCU\\Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "winreg",
                    path = path,
                    parent_option = o,
                    level = 2
                });
            }

            return o;
        }

        option AddPrefetchCleaner()
        {
            option o = new option()
            {
                id = "windows_prefetch",
                label = "Prefetch",
                description = "Delete the cache",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\Prefetch\\*.pf"
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "glob",
                    path = path,
                    parent_option = o,
                    level = 2
                });
            }

            return o;
        }

        option AddUpdateUninstallersCleaner()
        {
            option o = new option()
            {
                id = "windows_update_uninstallers",
                label = "Update uninstallers",
                description = "Delete uninstallers for Microsoft updates including hotfixes, service packs, and Internet Explorer updates",
                action = new List<action>()
            };

            string[] paths = new string[] {
                "$windir\\SoftwareDistribution\\Download",
                "$windir\\ie7updates",
                "$windir\\ie8updates",
            };

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o,
                    level = 3
                });
            }

            return o;
        }

        option AddRecycleBinCleaner()
        {
            option o = new option()
            {
                id = "windows_recyclebin",
                label = "Recycle bin",
                description = "Empty the recycle bin",
                action = new List<action>()
            };

            var drvs = DriveInfo.GetDrives();
            List<string> drivenames = new List<string>();
            foreach (var drv in drvs)
            {
                if (drv.DriveType == System.IO.DriveType.Fixed)
                {
                    drivenames.Add(drv.Name);
                }
            }

            List<string> paths = new List<string>();

            foreach (string drive in drivenames)
            {
                paths.Add(Path.Combine(drive, "$RECYCLE.BIN"));
            }
            
            //string[] paths = new string[] {
            //    "$windir\\SoftwareDistribution\\Download",
            //    "$windir\\ie7updates",
            //    "$windir\\ie8updates",
            //};

            foreach (string path in paths)
            {
                o.action.Add(new action()
                {
                    command = "delete",
                    search = "walk.files",
                    path = path,
                    parent_option = o,
                    level = 1
                });
            }

            return o;
        }

        option[] AddDuplicateCheckerCleaner()
        {
            List<option> ret = new List<option>();

            if (mCleaner.Properties.Settings.Default.DupChecker_CustomPath != null)
            {
                int i = 0;
                foreach (string filepath in mCleaner.Properties.Settings.Default.DupChecker_CustomPath)
                {
                    if (Directory.Exists(filepath))
                    {
                        option o = new option()
                        {
                            id = "duplicate_checker_" + (i++),
                            label = filepath.Substring(filepath.LastIndexOf("\\") + 1),
                            description = "Check for duplicate entries in " + filepath,
                            warning = "This option is slow!",
                            action = new List<action>()
                        };

                        o.action.Add(new action()
                        {
                            command = "dupchecker",
                            search = "dupchecker.all",
                            path = filepath,
                            level = 2,
                            parent_option = o,
                        });

                        ret.Add(o);
                    }
                }
            }

            return ret.ToArray();
        }
        #endregion
        #endregion
    }
}
