using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using CodeBureau;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Cleaners;
using mCleaner.Helpers;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using Microsoft.Practices.ServiceLocation;
using Octokit;
using MessageBox = System.Windows.MessageBox;
using TreeNode = mCleaner.Helpers.Controls.TreeNode;

namespace mCleaner.ViewModel
{
    public delegate void OnTreeNodeSelected(object sender, EventArgs e);
    public class ViewModel_CleanerML : ViewModelBase
    {
        public event OnTreeNodeSelected TreeNodeSelected;
        string _execPath = string.Empty;
        public List<TreeNode> _nodes = new List<TreeNode>();

        #region properties
        private ObservableCollection<TreeNode> _cleanersCollection = new ObservableCollection<TreeNode>();
        public ObservableCollection<TreeNode> CleanersCollection
        {
            get { return _cleanersCollection; }
            set
            {
                if (_cleanersCollection != value)
                {
                    _cleanersCollection = value;
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

        public ViewModel_RestoreRegistryOptions RestoreRegistry
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_RestoreRegistryOptions>();
            }
        }

        private bool _run = false;
        public bool Run
        {
            get { return _run; }
            set
            {
                if (_run != value)
                {
                    _run = value;
                    base.RaisePropertyChanged("Run");
                }
            }
        }

        private bool _isCancelProcessEnabled = false;
        public bool IsCancelProcessEnabled
        {
            get { return _isCancelProcessEnabled; }
            set
            {
                if (_isCancelProcessEnabled != value)
                {
                    _isCancelProcessEnabled = value;
                    base.RaisePropertyChanged("IsCancelProcessEnabled");
                }
            }
        }


        private int _gridWidth =150;
        public int GridWidth
        {
            get { return _gridWidth; }
            set
            {
                if (_gridWidth != value)
                {
                    _gridWidth = value;
                    base.RaisePropertyChanged("GridWidth");
                }
            }
        }

        private int _gridHeight = 120;
        public int GridHeight
        {
            get { return _gridHeight; }
            set
            {
                if (_gridHeight != value)
                {
                    _gridHeight = value;
                    base.RaisePropertyChanged("GridHeight");
                }
            }
        }

        

        private bool _btnPreviewCleanEnable = false;
        public bool btnPreviewCleanEnable
        {
            get { return _btnPreviewCleanEnable; }
            set
            {
                if (_btnPreviewCleanEnable != value)
                {
                    _btnPreviewCleanEnable = value;
                    base.RaisePropertyChanged("btnPreviewCleanEnable");
                }
            }
        }

        private bool _blnCollapseAll = true;
        public bool blnCollapseALL
        {
            get { return _blnCollapseAll; }
            set
            {
                if (_blnCollapseAll != value)
                {
                    _blnCollapseAll = value;
                    base.RaisePropertyChanged("blnCollapseALL");
                }
            }
        }


        private string _txtCollapse = "Collapse All";
        public string txtCollapse
        {
            get { return _txtCollapse; }
            set
            {
                if (_txtCollapse != value)
                {
                    _txtCollapse = value;
                    base.RaisePropertyChanged("txtCollapse");
                }
            }
        }

        private string _updateAvailableText =string.Empty;
        public string UpdateAvailableText
        {
            get { return _updateAvailableText; }
            set
            {
                if (_updateAvailableText != value)
                {
                    _updateAvailableText = value;
                    base.RaisePropertyChanged("UpdateAvailableText");
                }
            }
        }

        private bool _blnUpdateAvaibleVisible = false;
        public bool blnUpdateAvaibleVisible
        {
            get { return _blnUpdateAvaibleVisible; }
            set
            {
                if (_blnUpdateAvaibleVisible != value)
                {
                    _blnUpdateAvaibleVisible = value;
                    base.RaisePropertyChanged("blnUpdateAvaibleVisible");
                }
            }
        }

        private string _txtLoForgoundColor = "Black";
        public string txtLoForgoundColor
        {
            get { return _txtLoForgoundColor; }
            set
            {
                if (_txtLoForgoundColor != value)
                {
                    _txtLoForgoundColor = value;
                    base.RaisePropertyChanged("txtLoForgoundColor");
                }
            }
        }

        private bool _btnCleanNowPreviousState = false;
        public bool btnCleanNowPreviousState
        {
            get { return _btnCleanNowPreviousState; }
            set
            {
                if (_btnCleanNowPreviousState != value)
                {
                    _btnCleanNowPreviousState = value;
                    base.RaisePropertyChanged("btnCleanNowPreviousState");
                }
            }
        }

        private bool _btnCleaningOptionsEnable = true;
        public bool btnCleaningOptionsEnable
        {
            get { return _btnCleaningOptionsEnable; }
            set
            {
                if (_btnCleaningOptionsEnable != value)
                {
                    _btnCleaningOptionsEnable = value;
                    base.RaisePropertyChanged("btnCleaningOptionsEnable");
                }
            }
        }

        private bool _showFrontPage = false;
        public bool ShowFrontPage
        {
            get { return _showFrontPage; }
            set
            {
                if (_showFrontPage != value)
                {
                    _showFrontPage = value;
                    base.RaisePropertyChanged("ShowFrontPage");
                }
            }
        }

        private bool _showCleanLogBox = false;
        public bool ShowCleanLogBox
        {
            get { return _showCleanLogBox; }
            set
            {
                if (_showCleanLogBox != value)
                {
                    _showCleanLogBox = value;
                    base.RaisePropertyChanged("ShowCleanLogBox");
                }
            }
        }

        private bool _showCleanerDescription = false;
        public bool ShowCleanerDescription
        {
            get { return _showCleanerDescription; }
            set
            {
                if (_showCleanerDescription != value)
                {
                    _showCleanerDescription = value;
                    base.RaisePropertyChanged("ShowCleanerDescription");
                }
            }
        }

        private string _textLog = string.Empty;
        public string TextLog
        {
            get { return _textLog; }
            set
            {
                if (_textLog != value)
                {
                    _textLog = value + "\r\n";
                    base.RaisePropertyChanged("TextLog");
                }
            }
        }

        private bool _progressIsIndeterminate = false;
        public bool ProgressIsIndeterminate
        {
            get { return _progressIsIndeterminate; }
            set
            {
                if (_progressIsIndeterminate != value)
                {
                    _progressIsIndeterminate = value;
                    base.RaisePropertyChanged("ProgressIsIndeterminate");
                }
            }
        }

        private int _maxProgress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                if (_maxProgress != value)
                {
                    _maxProgress = value;
                    base.RaisePropertyChanged("MaxProgress");
                }
            }
        }

        private int _progressIndex = 0;
        public int ProgressIndex
        {
            get { return _progressIndex; }
            set
            {
                if (_progressIndex != value)
                {
                    _progressIndex = value;
                    base.RaisePropertyChanged("ProgressIndex");
                }
            }
        }

        private string _progressText = string.Empty;
        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    base.RaisePropertyChanged("ProgressText");
                }
            }
        }

        private bool _cleanOptionSafe = false;
        public bool CleanOption_Safe
        {
            get { return _cleanOptionSafe; }
            set
            {
                if (_cleanOptionSafe != value)
                {
                    _cleanOptionSafe = value;
                    base.RaisePropertyChanged("CleanOption_Safe");
                }
            }
        }

        private bool _cleanOptionModerate = false;
        public bool CleanOption_Moderate
        {
            get { return _cleanOptionModerate; }
            set
            {
                if (_cleanOptionModerate != value)
                {
                    _cleanOptionModerate = value;
                    base.RaisePropertyChanged("CleanOption_Moderate");
                }
            }
        }

        private bool _cleanOptionAggressive = false;
        public bool CleanOption_Aggressive
        {
            get { return _cleanOptionAggressive; }
            set
            {
                if (_cleanOptionAggressive != value)
                {
                    _cleanOptionAggressive = value;
                    base.RaisePropertyChanged("CleanOption_Aggressive");
                }
            }
        }

        private bool _cleanOptionCustom = false;
        public bool CleanOption_Custom
        {
            get { return _cleanOptionCustom; }
            set
            {
                if (_cleanOptionCustom != value)
                {
                    _cleanOptionCustom = value;
                    base.RaisePropertyChanged("CleanOption_Custom");
                }
            }
        }

        private bool _saveCustomOptions = false;
        public bool SaveCustom_Options
        {
            get { return _saveCustomOptions; }
            set
            {
                if (_saveCustomOptions != value)
                {
                    _saveCustomOptions = value;
                    base.RaisePropertyChanged("SaveCustom_Options");
                }
            }
        }

        private bool _btnCloseEnable = true;
        public bool btnCloseEnable
        {
            get { return _btnCloseEnable; }
            set
            {
                if (_btnCloseEnable != value)
                {
                    _btnCloseEnable = value;
                    base.RaisePropertyChanged("btnCloseEnable");
                }
            }
        }

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    base.RaisePropertyChanged("SelectedTabIndex");
                }
            }
        }

        private TreeNode _selectedNode = new TreeNode();
        public TreeNode SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    base.RaisePropertyChanged("SelectedNode");
                }
            }
        }


        private bool _btnCancel = false;
        public bool BtnCancel
        {
            get { return _btnCancel; }
            set
            {
                if (_btnCancel != value)
                {
                    _btnCancel = value;
                    this.RaisePropertyChanged("BtnCancel");
                }
            }
        }
        

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (_cancel != value)
                {
                    _cancel = value;
                    this.RaisePropertyChanged("Cancel");
                }
            }
        }
        #endregion

        #region ctor
        public ViewModel_CleanerML()
        {
            _execPath = AppDomain.CurrentDomain.BaseDirectory;
            GetCleaners();

            Run = false;
            ShowFrontPage = true;
            ShowCleanerDescription = false;

            if (base.IsInDesignMode)
            {
                Run = true;
                ShowFrontPage = false;
                ProgressText = "Status goes here";
                this.ProgressIsIndeterminate = true;
            }
            else
            {
                CleanOption_Safe = Settings.Default.CleanOption == 1 ? true : false;
                CleanOption_Moderate = Settings.Default.CleanOption == 2 ? true : false;
                CleanOption_Aggressive = Settings.Default.CleanOption == 3 ? true : false;

                Command_Preview = new RelayCommand(Command_Preview_Click);
                Command_Clean = new RelayCommand<string>(Command_Clean_Click);
                Command_CustomCleaningSelection = new RelayCommand(Command_CustomCleaningSelection_Click);
                Command_RegistrySelection= new RelayCommand(Command_RegitryClearner_Click);
                Command_CustomSaveAsSelection = new RelayCommand(Command_CustomSaveAsSelection_Click);
                //Command_RestoreRegistrySelection = new RelayCommand(CommandRe);
                CommandClearSelection = new RelayCommand(Command_ClearSelection_Click);
                Command_CleanNow = new RelayCommand(Command_CleanNow_Click);
                Command_Quit = new RelayCommand(Command_Quit_Click);
                Command_CloseWindow = new RelayCommand(Command_CloseWindow_Click);
                Command_SaveLog = new RelayCommand(Command_SaveLog_Click);
                Command_CloseCleanerDescription = new RelayCommand(Command_CloseCleanerDescription_Click);
                CommandCollapseAll = new RelayCommand(CommandCollapseAllClick);
                Command_Cancel = new RelayCommand(Command_Cancel_Click);
            }
        }

        private void Command_CloseWindow_Click()
        {
            this.Run = false;
            btnPreviewCleanEnable = true;
            this.Cancel = true;
            this.ShowFrontPage = true;
        }
        private void Command_SaveLog_Click()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt";
            sfd.DefaultExt= "txt";
            sfd.FileName = "mCLeanerLog";
            sfd.Title = "Save a Log";
            sfd.ShowDialog();
            File.WriteAllText(sfd.FileName, TextLog);
        }


        
        #endregion

        #region commands
        public ICommand Command_Quit { get; set; }

        public ICommand Command_CloseWindow { get; set; }
        public ICommand Command_SaveLog { get; set; }
        public ICommand Command_Preview { get; internal set; }
        public ICommand Command_Clean { get; internal set; }
        public ICommand Command_CustomCleaningSelection { get; internal set; }
        public ICommand Command_CustomSaveAsSelection { get; internal set; }
        public ICommand Command_RegistrySelection { get; internal set; }
        public ICommand Command_RestoreRegistrySelection { get; internal set; }
        public ICommand CommandClearSelection { get; internal set; }
        public ICommand Command_CleanNow { get; internal set; }
        public ICommand Command_CleanOption { get; internal set; }
        public ICommand Command_CloseCleanerDescription { get; internal set; }
        public ICommand CommandCollapseAll { get; internal set; }
        public ICommand Command_Cancel { get; internal set; }
        #endregion

        #region command methods
        public async void Command_Preview_Click()
        {
            Worker.I.Preview = true;
            txtLoForgoundColor = "Black";
            btnCloseEnable = false;
            IsCancelProcessEnabled = true;
            this.Cancel = false;
            this.ShowCleanerDescription = false;
            this.ShowFrontPage = false;
            this.Run = true;
            btnPreviewCleanEnable = false;
            btnCleaningOptionsEnable = false;
            this.ProgressIsIndeterminate = true;

            await Start();

            if (this.Cancel)
            {
                ProgressWorker.I.EnQ("Operation Canceled");
            }

            await Worker.I.PreviewWork();

            // some time, user may have cancel the operation
            if (this.Cancel)
            {
                ProgressWorker.I.EnQ("Operation Canceled");
            }
            btnCloseEnable = true;
            IsCancelProcessEnabled = false;
            btnCleaningOptionsEnable = true;
            this.ProgressIsIndeterminate = false;
            if(Worker.I.TotalFileDelete>0 && MessageBox.Show("You want to clean this files?","mCleaner",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes)
            {
                Command_CleanNow_Click();
            }
        }


        public void Command_Clean_Click(string i)
        {
            this.Cancel = false;
            int level = int.Parse(i);
            this.CleanOption_Custom = false;
            this.SaveCustom_Options = false;

            if (level <= 3)
            {
                CleanOption_Safe = false;
                CleanOption_Moderate = false;
                CleanOption_Aggressive = false;

                CleanOption_Safe = (level == 1) ? true : false;
                CleanOption_Moderate = (level == 2) ? true : false;
                CleanOption_Aggressive = (level == 3) ? true : false;

                Settings.Default.CleanOption = level;
                Settings.Default.Save();
                btnPreviewCleanEnable = true;
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


            CommandExpandAllClick();

            if (level >= 4)
            {
                Command_CleanNow_Click();
            }
        }

        public void Command_CustomCleaningSelection_Click()
        {
            this.CleanOption_Custom = true;
            this.SaveCustom_Options = true;
            btnPreviewCleanEnable = true;

            // uncheck everything first
            foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        option o = (option)child.Tag;
                        child.IsChecked = false;
                        child.SupressWarningMessage = true;
                    }
                }
            }

            if (Settings.Default.CustomCleanerSelections == null) return;

            // do we have anything to restore?
            if (Settings.Default.CustomCleanerSelections.Count > 0)
            {
                List<string> ids = new List<string>();
                foreach (string id in Settings.Default.CustomCleanerSelections) ids.Add(id);

                // then restore selection
                #region check what needs to be checked
                foreach (TreeNode tn in this.CleanersCollection)
                {
                    foreach (TreeNode child in tn.Children)
                    {
                        if (child.Tag is option)
                        {
                            option o = (option)child.Tag;
                            child.IsChecked = ids.Contains(o.id + "|" + o.parent_cleaner.id);
                        }
                    }
                }
                #endregion
            }


            CommandExpandAllClick();
        }

        //little_reg;

        public void Command_RegitryClearner_Click()
        {
            btnPreviewCleanEnable = true;
            this.CleanOption_Custom = true;
            this.SaveCustom_Options = false;
            // uncheck everything first
            foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        option o = (option)child.Tag;
                        child.IsChecked = false;
                        child.SupressWarningMessage = true;
                    }
                }
            }

            #region check what needs to be checked
            foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        option o = (option)child.Tag;
                        if (o.parent_cleaner.id == "little_reg")
                            child.IsChecked = true;
                    }
                }
            }
            #endregion


            CommandExpandAllClick();
        }

        public void Command_RestoreRegistrySelection_Click(string FullFilePathtoRestore)
        {

            try
            {
                this.Cancel = false;
                Worker.I.Preview = false;
                btnCloseEnable = false;
                IsCancelProcessEnabled = false;
                this.Run = true;
                btnPreviewCleanEnable = false;
                btnCleaningOptionsEnable = false;
                this.ShowCleanerDescription = false;
                this.ShowFrontPage = false;
                txtLoForgoundColor = "Red";
                DataTable dt = new DataTable("RegistryPaths");
                dt.ReadXml(FullFilePathtoRestore);
                MaxProgress = dt.Rows.Count;
                foreach (DataRow dr in dt.Rows)
                {
                    if (!dr["RegistryKeyFullPath"].ToString().StartsWith("HKCR"))
                    {
                        ProgressWorker.I.EnQ("Restoring Key: " + dr["RegistryKeyFullPath"].ToString());
                        TextLog += "Restoring Key: " + dr["RegistryKeyFullPath"].ToString();
                        ProgressIndex++;
                        RestoreRegistrykey(dr["RegistryKeyFullPath"].ToString(), dr["Location"].ToString());
                        Thread.Sleep(25);
                        ProgressWorker.I.EnQ("Restoring Key Success: " + dr["RegistryKeyFullPath"].ToString());
                        TextLog += "Restoring Key Success: " + dr["RegistryKeyFullPath"].ToString();
                    }
                    else
                        ProgressIndex++;
                }
            }
            catch (Exception ex)
            {
                
            }
            btnCloseEnable = true;
            this.ProgressIsIndeterminate = false;
            IsCancelProcessEnabled = false;
            btnCleaningOptionsEnable = true;
            ProgressWorker.I.EnQ("Done");
            ProgressText = "Done";
        }


         public void Command_CustomSaveAsSelection_Click()
        {

           var ListofStrgingCollecion= new StringCollection();
           foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        option o = (option)child.Tag;
                        if (child.IsChecked.HasValue && child.IsChecked.Value == true)
                        {
                            if (Settings.Default.CustomCleanerSelections == null) 
                                Settings.Default.CustomCleanerSelections = new StringCollection();
                            ListofStrgingCollecion.Add(o.id + "|" + o.parent_cleaner.id);
                            // then save it
                            
                        }
                    }
                }
                
            }
             if (ListofStrgingCollecion.Count > 0)
             {
                 if (Settings.Default.CustomCleanerSelections != null)
                     Settings.Default.CustomCleanerSelections.Clear();

                 Settings.Default.CustomCleanerSelections = ListofStrgingCollecion;
                 Settings.Default.Save();
             }
             else
                 MessageBox.Show("There are no cleaning options selected.", "mCleaner", MessageBoxButton.OK,MessageBoxImage.Information);
        }

       


        public void RestoreRegistrykey(String strKey,String strLocationToSave)
        {

            var strCommnd = "RegSaveRestore /R " + strKey + " " + strLocationToSave + " /F"; 
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            var process = new Process {StartInfo = startInfo};

            process.Start();
            process.StandardInput.WriteLine("cd " + AppDomain.CurrentDomain.BaseDirectory);
            process.StandardInput.WriteLine(strCommnd);
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }

        public void Command_ClearSelection_Click()
        {
            btnPreviewCleanEnable = false;
            foreach (TreeNode tn in this.CleanersCollection)
            {
                foreach (TreeNode child in tn.Children)
                {
                    if (child.Tag is option)
                    {
                        option o = (option)child.Tag;
                        child.IsChecked = false;
                    }
                }
            }
        }

        public async void Command_CleanNow_Click()
        {
            this.Cancel = false;
            Worker.I.Preview = false;
            btnCloseEnable = false;
            IsCancelProcessEnabled = true;
            this.Run = true;
            btnPreviewCleanEnable = false;
            btnCleaningOptionsEnable = false;
            this.ShowCleanerDescription = false;
            this.ShowFrontPage = false;
            txtLoForgoundColor = "Red";
            await Start();

            // some time, user may have cancel the operation
            if (this.Cancel)
            {
                ProgressWorker.I.EnQ("Operation Canceled");
            }

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

        public void CommandCollapseAllClick()
        {
            blnCollapseALL = !blnCollapseALL;
              
            foreach (TreeNode tn in this.CleanersCollection)
            {
                tn.IsInitiallySelected = false;
                tn.IsExpanded = blnCollapseALL;
            }
            if(!blnCollapseALL)
            if(this.CleanersCollection.Where(dc=>dc.IsExpanded==true).Any())
            {
                this.CleanersCollection.Where(dc => dc.IsExpanded == true).First().IsInitiallySelected = blnCollapseALL;
                this.CleanersCollection.Where(dc => dc.IsExpanded == true).First().IsChecked = blnCollapseALL;
                this.CleanersCollection.Where(dc => dc.IsExpanded == true).First().IsExpanded = blnCollapseALL;
                
                Command_CloseCleanerDescription_Click();

            }
            txtCollapse = blnCollapseALL ? "Collapse All" : "Expand All";
        }

        public void CommandExpandAllClick()
        {
            foreach (TreeNode tn in this.CleanersCollection)
            {
                tn.IsExpanded = true;
            }
            blnCollapseALL = true;
            ShowCleanerDescription = false;
            txtCollapse = blnCollapseALL ? "Collapse All" : "Expand All";
        }

        

        public void Command_Cancel_Click()
        {
            this.Cancel = true;
            ProgressWorker.I.EnQ("Please wait while operation is being canceled.");
            MessageBox.Show("Please wait while operation is being canceled.", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
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
            this.btnPreviewCleanEnable = false;
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
                    this.btnPreviewCleanEnable = true;
                }

                if (!this.btnPreviewCleanEnable)
                    if (CleanersCollection.Where(dc => dc.IsChecked == true).Count() <= 0)
                    {
                        foreach (TreeNode tn in this.CleanersCollection)
                        {
                            foreach (TreeNode child in tn.Children)
                            {
                                if (child.IsChecked == true)
                                {
                                    this.btnPreviewCleanEnable = true;
                                    break;
                                }
                            }
                            if (btnPreviewCleanEnable)
                                break;
                        }
                    }
                    else
                        this.btnPreviewCleanEnable = true;
            }
            else if (root.Tag is cleaner)
            {

            }

            // get checked nodes and save it its custom selection


            //if (root.Key.Contains("duplicate_checker"))
            //{
            //    SelectedTabIndex = 1;
            //}
            //else
            //{
            //    SelectedTabIndex = 0;
            //}
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

                string cleaners_folder = Path.Combine(this._execPath, "cleaners");
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
                            root.Cleaner = clnr;
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

       public async Task CheckForUpdates()
        {
           try
           {
               
                   var client = new GitHubClient(new ProductHeaderValue("mCleaner"));
                   var releasess = await client.Release.GetAll("MicroHealthLLC", "mCleaner");
                   var latest = releasess[0];

                   if (new Version(latest.TagName.Replace("v", string.Empty)) >
                       Assembly.GetExecutingAssembly().GetName().Version)
                   {
                       UpdateAvailableText = "mCleaner update available version:" +
                                             latest.TagName.Replace("v", string.Empty);
                       blnUpdateAvaibleVisible = true;
                   }
           }
            catch (Exception ex)
            {
              //leaving cathc blank since we dont want user to display the error.
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

            if(!Worker.I.Preview)
            foreach (TreeNode node in this.CleanersCollection)
            {
                if (node.Name == "Google Chrome" || node.Name == "Mozilla Firefox")
                {
                    foreach (TreeNode child in node.Children)
                    {
                        if (this.Cancel)
                        {
                            ProgressWorker.I.EnQ("Operation Canceled");
                            break;
                        }

                        if (child.IsChecked != null)
                        {
                            if (child.IsChecked.Value)
                            {
                                if (node.Name == "Google Chrome")
                                {
                                    if (CommandLogic_Chrome.IsChromeRunning())
                                    {
                                        if (MessageBox.Show("It seems Google Chrome is running. To perform Google Chrome cleaning, you need to close Chrome. Are you sure you want to continue anyway?", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                            return false;
                                        else
                                            break;
                                    }
                                }
                                else if (node.Name == "Mozilla Firefox")
                                {
                                    if (CommandLogic_Chrome.IsFirefoxRunning())
                                    {
                                        if (MessageBox.Show("It seems Firefox is running. To perform Firefox cleaning, you need to close Firefox. Are you sure you want to continue anyway?", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                            return false;
                                        else
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }


            }
            foreach (TreeNode node in this.CleanersCollection)
            {
                if (this.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Canceled");
                    break;
                }

                foreach (TreeNode child in node.Children)
                {
                    if (this.Cancel)
                    {
                        ProgressWorker.I.EnQ("Operation Canceled");
                        break;
                    }

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

        }

        public void EnqueueOption(option o)
        {
            string last_Log = string.Empty;

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

            SortCleanerCollection();
        }

        void AddSystemCleaner(bool select = false)
        {
            cleaner c = new cleaner()
            {
                id = "system",
                type = "Windows Temp and Cache",
                label = "Microsoft Windows System",
                description = "General Windows system cleaners",
                option = new List<option>()
            };

            TreeNode root = new TreeNode();
            root = new TreeNode(c.label, c.id);
            root.IsAccordionHeader = true;
            root.IsExpanded = true;
            root.Tag = c;
            root.Cleaner = c;
            root.TreeNodeChecked += TeeNode_TreeNodeChecked;
            root.TreeNodeSelected += TreeNode_TreeNodeSelected;
            root.Children = new List<TreeNode>();

            c.option.Add(MicrosoftWindows.AddClipboardCleaner());

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

            if (Settings.Default.DupChecker_CustomPath != null)
            {
                int i = 0;
                foreach (string filepath in Settings.Default.DupChecker_CustomPath)
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
