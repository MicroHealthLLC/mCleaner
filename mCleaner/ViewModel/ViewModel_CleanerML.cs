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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
                    _TextLog = value;
                    base.RaisePropertyChanged("TextLog");
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
                ProgressText = "lasdh aksjdhaks dkhasjdhkajsdh kh kadsh kajdhk ashdk asjdhk asdh";
            }
            else
            {
                Command_Preview = new RelayCommand(Command_Preview_Click);
                Command_Clean = new RelayCommand(Command_Clean_Click);
                Command_Quit = new RelayCommand(Command_Quit_Click);
            }
        }
        #endregion

        #region commands
        public ICommand Command_Quit { get; set; }
        public ICommand Command_Preview { get; internal set; }
        public ICommand Command_Clean { get; internal set; }
        
        #endregion

        #region command methods
        public void Command_Preview_Click()
        {
            Worker.I.Preview = true;
            this.Run = true;

            Start();
            Worker.I.PreviewWork();
        }

        public void Command_Clean_Click()
        {
            Worker.I.Preview = false;
            this.Run = true;
            Start();
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
        }

        void TeeNode_TreeNodeChecked(object sender, EventArgs e)
        {
            //TreeNode root = sender as TreeNode;
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
                    Children =
                    {
                        new TreeNode("Cache") {},
                        new TreeNode("Cookies") {}
                    }
                };
                root.Initialize();
                this.CleanersCollection.Add(root);

                TreeNode root2 = new TreeNode("Windows Explorer")
                {
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
                foreach (string filename in Directory.GetFiles(cleaners_folder, "*.xml"))
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
                                isSupported = CheckIfSupported(clnr);
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
                            this.CleanersCollection.Add(root);
                        }
                    }
                }
            }

            return this.CleanersCollection;
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

        public void Start()
        {
            Worker.I.ClearTTD(); // reset TTD content

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

                            Help.RunInBackground(() =>
                            {
                                this.ProgressText = "Please wait. " + (Worker.I.Preview ? "Previewing" : "Working on") + " " + o.parent_cleaner.label;
                                this.ProgressIndex = 0;
                                this.MaxProgress = 0;
                            });

                            ExecuteOption(o);
                        }
                    }
                }
            }
        }

        public void ExecuteOption(option o)
        {
            foreach (action _a in o.action)
            {
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
                }

                if (axn != null)
                {
                    axn.Action = _a;
                    axn.Execute(); // execute for queueing 
                }
            }
        }
        #endregion
    }
}
