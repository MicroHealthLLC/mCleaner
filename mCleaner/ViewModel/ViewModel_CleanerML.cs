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
using mCleaner.Cleaners;

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

        private bool _ShowFrontPage = false;
        public bool ShowFrontPage
        {
            get { return _ShowFrontPage; }
            set
            {
                if (_ShowFrontPage != value)
                {
                    _ShowFrontPage = value;
                    base.RaisePropertyChanged("ShowFrontPage");
                }
            }
        }

        private bool _ShowCleanLogBox = false;
        public bool ShowCleanLogBox
        {
            get { return _ShowCleanLogBox; }
            set
            {
                if (_ShowCleanLogBox != value)
                {
                    _ShowCleanLogBox = value;
                    base.RaisePropertyChanged("ShowCleanLogBox");
                }
            }
        }

        private bool _ShowCleanerDescription = false;
        public bool ShowCleanerDescription
        {
            get { return _ShowCleanerDescription; }
            set
            {
                if (_ShowCleanerDescription != value)
                {
                    _ShowCleanerDescription = value;
                    base.RaisePropertyChanged("ShowCleanerDescription");
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

        private TreeNode _SelectedNode = new TreeNode();
        public TreeNode SelectedNode
        {
            get { return _SelectedNode; }
            set
            {
                if (_SelectedNode != value)
                {
                    _SelectedNode = value;
                    base.RaisePropertyChanged("SelectedNode");
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
            ShowFrontPage = true;
            ShowCleanerDescription = false;

            if (base.IsInDesignMode)
            {
                Run = false;
                ShowFrontPage = true;
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
                Command_CleanNow = new RelayCommand(Command_CleanNow_Click);
                Command_Quit = new RelayCommand(Command_Quit_Click);
                Command_CloseCleanerDescription = new RelayCommand(Command_CloseCleanerDescription_Click);
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
        public ICommand Command_CleanNow { get; internal set; }
        public ICommand Command_CleanOption { get; internal set; }
        public ICommand Command_CloseCleanerDescription { get; internal set; }
        #endregion

        #region command methods
        public async void Command_Preview_Click()
        {
            Worker.I.Preview = true;

            this.ShowCleanerDescription = false;
            this.ShowFrontPage = false;
            this.Run = true;
            this.ProgressIsIndeterminate = true;

            await Start();

            await Worker.I.PreviewWork();
        }

        public void Command_Clean_Click(string i)
        {
            int level = int.Parse(i);

            if (level <= 3)
            {
                CleanOption_Safe = false;
                CleanOption_Moderate = false;
                CleanOption_Aggressive = false;

                CleanOption_Safe = (level == 1) ? true : false;
                CleanOption_Moderate = (level == 2) ? true : false;
                CleanOption_Aggressive = (level == 3) ? true : false;

                Properties.Settings.Default.CleanOption = level;
                Properties.Settings.Default.Save();
            }

            #region check what needs to be checked
            foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        // reset check status
                        foreach (action a in ((option)child.Tag).action)
                        {
                            child.IsChecked = false;
                            child.SupressWarningMessage = true;
                        }

                        foreach (action a in ((option)child.Tag).action)
                        {
                            if (a.parent_option.level == 0) a.parent_option.level = 3;

                            if (level <= 3)
                            {
                                child.IsChecked = a.parent_option.level <= level ? true : false;
                            }
                            else
                            {
                                child.IsChecked = a.parent_option.level <= level - 3 ? true : false;
                            }
#if DEBUG
                            Console.WriteLine(tn.Name + ", " + ((option)child.Tag).label + ", " + a.parent_option.level);
#endif
                            break;
                        }
                    }
                }
            }
            #endregion

            if (level >= 4)
            {
                Command_CleanNow_Click();
            }
        }

        public async void Command_CleanNow_Click()
        {
            Worker.I.Preview = false;
            this.Run = true;
            this.ShowCleanerDescription = false;
            this.ShowFrontPage = false;

            await Start();
            Worker.I.DoWork();
        }

        public void Command_Quit_Click()
        {
            // Disable needed privileges
            Permissions.SetPrivileges(false);
            Process.GetCurrentProcess().Kill();
        }

        public void Command_CloseCleanerDescription_Click()
        {
            this.ShowCleanerDescription = false;

            if (Worker.I.TTD.Count == 0)
            {
                this.ShowFrontPage = true;
            }
            else
            {
                this.Run = true;
            }
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
                        if (!root.SupressWarningMessage)
                        {
                            MessageBox.Show("Warning!\r\n" + o.warning, "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
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
                TreeNode root = new TreeNode("Chromium", "chrome")
                {
                    IsInitiallySelected = true,
                    IsAccordionHeader = true,
                    IsExpanded = true,
                    Tag = 1,
                    Children =
                    {
                        new TreeNode("Cache") {},
                        new TreeNode("Cookies") {}
                    }
                };
                root.Initialize();
                this._nodes.Add(root);

                TreeNode root2 = new TreeNode("Windows Explorer", "win_exp")
                {
                    IsAccordionHeader = true,
                    IsExpanded = true,
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
                this._nodes.Add(root2);

                SortCleanerCollection();
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

#if !DEBUG
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
#endif

                        if (isSupported)
                        {
                            root = new TreeNode(clnr.label, clnr.id);
                            //root.IsInitiallySelected = true;
                            //root.IsChecked = true;
                            root.Tag = clnr;
                            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
                            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
                            root.IsAccordionHeader = true;
                            root.IsExpanded = true;
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
                //AddDuplicateCleaner();
            }

            SortCleanerCollection();

            return this.CleanersCollection;
        }

        void SortCleanerCollection()
        {
            this._nodes = this._nodes.OrderBy(p => p.Name).ToList();
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

                EnqueueOption(o);

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
                // insert actions?
                p.Inlines.Add(string.Format("There {0} {1} actions in this option.", o.action.Count == 1 ? "is" : "are", o.action.Count));
                //foreach (action a in o.action)
                //{

                //}
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

                            await Task.Run(() => EnqueueOption(o));
                        }
                    }
                }
            }

            return true;

            //AddCustomLocationsToTTD();
        }

        public void EnqueueOption(option o)
        {
            string last_Log = string.Empty;

            foreach (action _a in o.action)
            {
                if (!Worker.I.Preview) // check only when not in preview mode
                {
                    #region // check cleaning level
                    int level = 3; // let it be the default
                    int curlevel = Properties.Settings.Default.CleanOption;

                    if (_a.parent_option.level == 0)
                    {
                        level = 3;
                    }
                    else
                    {
                        level = _a.parent_option.level;
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
                    axn.Enqueue(); // execute for queueing 
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
            root.IsExpanded = true;
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
                label = "Microsoft Windows System",
                description = "General Windows system cleaners",
                option = new List<option>()
            };

            TreeNode root = new TreeNode();
            root = new TreeNode(c.label, c.id);
            root.IsAccordionHeader = true;
            root.IsExpanded = true;
            root.Tag = c;
            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
            root.Children = new List<TreeNode>();

            c.option.Add(MicrosoftWindows.AddClipboardCleaner());
            //c.option.Add(AddClamAVCustomLocationsToTTD());
            c.option.Add(MicrosoftWindows.AddCustomLocationsToTTD());
            //c.option.Add(AddDuplicateCheckerCleaner());

            // deep scan options
            c.option.Add(MicrosoftWindows.AddDeepScan_Backup_Cleaner());
            c.option.Add(MicrosoftWindows.AddDeepScan_OfficeTemp_Cleaner());
            c.option.Add(MicrosoftWindows.AddDeepScan_ThumbsDB_Cleaner());

            c.option.Add(MicrosoftWindows.AddWindowsLogsCleaner());
            c.option.Add(MicrosoftWindows.AddMemoryDumpCleaner());
            c.option.Add(MicrosoftWindows.AddMUICacheCleaner());
            c.option.Add(MicrosoftWindows.AddPrefetchCleaner());
            c.option.Add(MicrosoftWindows.AddRecycleBinCleaner());
            c.option.Add(MicrosoftWindows.AddTemporaryFilesCleaner());
            c.option.Add(MicrosoftWindows.AddUpdateUninstallersCleaner());

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

        public static option[] AddDuplicateCheckerCleaner()
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
                            level = 2,
                            action = new List<action>()
                        };

                        o.action.Add(new action()
                        {
                            command = "dupchecker",
                            search = "dupchecker.all",
                            path = filepath,
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
