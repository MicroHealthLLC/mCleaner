using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using mCleaner.Helpers;
using mCleaner.Helpers.Data;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics
{
    public class Worker
    {
        #region vars
        BackgroundWorker bgWorker;

        public int  TotalFileDelete = 0;
        public long TotalFileSize = 0;
        public int  TotalWork = 0;
        public int  TotalSpecialOperations = 0;
        #endregion

        #region properties
        ViewModel_CleanerML VMCleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        ViewModel_Clam Clam
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Clam>();
            }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }

        //private string _PreviewLog = string.Empty;
        //public string PreviewLog
        //{
        //    get { return _PreviewLog; }
        //    set
        //    {
        //        if (_PreviewLog != value)
        //        {
        //            _PreviewLog = value;
        //        }
        //    }
        //}

        /// <summary>
        /// set to true by default
        /// </summary>
        private bool _Preview = true;
        public bool Preview
        {
            get { return _Preview; }
            set
            {
                if (_Preview != value)
                {
                    _Preview = value;
                }
            }
        }

        private Queue<Model_ThingsToDelete> _TTD = new Queue<Model_ThingsToDelete>();
        public Queue<Model_ThingsToDelete> TTD
        {
            get { return _TTD; }
            set
            {
                if (_TTD != value)
                {
                    _TTD = value;

                    if (VMCleanerML.MaxProgress == 0)
                    {
                        VMCleanerML.MaxProgress = value.Count();
                    }
                }
            }
        }
        #endregion

        #region ctors
        public Worker()
        {
            bgWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.ProgressChanged += bgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
        }
        private static Worker _i = new Worker();
        public static Worker I { get { return _i; } }
        #endregion

        #region events
        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowTotalOperations(false);

            this.TotalWork = 0;
            this.TotalFileDelete = 0;
            this.TotalFileSize = 0;
            this.TotalSpecialOperations = 0;
            VMCleanerML.btnCloseEnable = true;
            VMCleanerML.IsCancelProcessEnabled = false;
        }

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                VMCleanerML.TextLog += e.UserState.ToString();
                VMCleanerML.ProgressText = e.UserState.ToString();
                VMCleanerML.MaxProgress = this.TTD.Count;
                VMCleanerML.ProgressIndex++;
            }
        }

        async void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string last_Log = string.Empty;
            string strBackUpFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string strRegistryDatSaveFolderPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "mCleaner\\RegistryBackups\\" + strBackUpFolderName);
            if (!Directory.Exists(strRegistryDatSaveFolderPath))
                Directory.CreateDirectory(strRegistryDatSaveFolderPath);

            while (this.TTD.Count != 0)
            {
                if (bgWorker.CancellationPending || this.VMCleanerML.Cancel) break;

                if (!this.Preview)
                {
                    Model_ThingsToDelete ttd = this.TTD.Dequeue();

                    #region // execute cleaning commands
                    switch (ttd.command)
                    {
                        case COMMANDS.delete:
                            //ExecuteDeleteCommand(ttd);
                            CommandLogic_Delete.I.ExecuteDeleteCommand(ttd, this.bgWorker, this.TTD);
                            break;

                        #region // special commands
                        case COMMANDS.winreg:
                            ExecuteWinRegCommand(ttd);
                            break;
                        case COMMANDS.sqlite_vacuum:
                            ExecuteSQLiteVacuumCommand(ttd);
                            break;
                        #endregion

                        #region // other special commands
                        case COMMANDS.ini:
                            ExecuteIniCommand(ttd);
                            break;
                        case COMMANDS.json:
                            ExecuteJSONCommand(ttd);
                            break;
                        #endregion

                        #region // Google Chrome commands
                        case COMMANDS.chrome_autofill:
                            ExecuteChromeCommand(ttd);
                            break;
                        case COMMANDS.chrome_database_db:
                            ExecuteChromeCommand(ttd);
                            break;
                        case COMMANDS.chrome_favicons:
                            ExecuteChromeCommand(ttd);
                            break;
                        case COMMANDS.chrome_history:
                            ExecuteChromeCommand(ttd);
                            break;
                        case COMMANDS.chrome_keywords:
                            ExecuteChromeCommand(ttd);
                            break;
                        #endregion

                        #region // clamwin commands
                        case COMMANDS.clamscan:
                            ExecuteClamWinCommand(ttd, false);
                            break;
                        #endregion

                        #region // little registry cleaner
                        case COMMANDS.littleregistry:
                          await ExecuteLittleRegistryCleanerCommand(ttd,strRegistryDatSaveFolderPath, false);
                            break;
                        #endregion

                        case COMMANDS.clipboard:
                            CommandLogic_Clipboard.I.ExecuteCommand();
                            break;

                        case COMMANDS.dupchecker:
                            CommandLogic_DuplicateChecker.I.ScanPath(ttd.FullPathName);
                            CommandLogic_DuplicateChecker.I.Start(this.DupChecker.DupplicateCollection, this.DupChecker.IsMove ? 1 : 0, this.bgWorker);
                            CommandLogic_DuplicateChecker.I.ScanPath(ttd.FullPathName);
                            this.DupChecker.FileOperationPanelShow = true;
                            
                            break;
                    }
                    #endregion
                }
            }
            if (!Preview && dtThingsDeleted != null && dtThingsDeleted.Rows.Count > 0)
            {
                string strRegistryFolderPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "mCleaner\\RegistryBackups\\XMLInfo");
                if (!Directory.Exists(strRegistryFolderPath))
                    Directory.CreateDirectory(strRegistryFolderPath);
                
                DirectoryInfo di = new DirectoryInfo(strRegistryFolderPath);
                FileSystemInfo[] files = di.GetFileSystemInfos();
                var orderedFiles = files.OrderBy(f => f.Name);
                if (orderedFiles.Count() > 4)
                {
                    orderedFiles.First().Delete();
                   var strDirectory= Path.Combine(Environment.GetEnvironmentVariable("APPDATA"),"mCleaner\\RegistryBackups\\" + Path.GetFileNameWithoutExtension(orderedFiles.First().Name));
                    if(Directory.Exists(strDirectory))
                        Directory.Delete(strDirectory);
                }
                    
               
                dtThingsDeleted.WriteXml(Path.Combine(strRegistryFolderPath, strBackUpFolderName + ".mCleanerBak"),XmlWriteMode.WriteSchema);
                dtThingsDeleted = null;
            }
        }


        #endregion

        #region methods
        public async Task<bool> PreviewWork()
        {
            //this.TTD = new Queue<Model_ThingsToDelete>(this.TTD.Reverse());

            //PreviewLog = string.Empty;
            //VMCleanerML.TextLog = string.Empty;
            string last_Log = string.Empty;

            foreach (Model_ThingsToDelete ttd in this.TTD)
            {
                if (this.VMCleanerML.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Canceled");
                    break;
                }

                #region execute preview commands
                switch (ttd.command)
                {
                    case COMMANDS.delete:
                        await Task.Run(() => ExecuteDeleteCommand(ttd, true));
                        //await Task.Run(() => CommandLogic_Delete.I.ExecuteDeleteCommand(ttd, this.bgWorker, this.TTD, true));
                        break;

                    #region // special commands
                    case COMMANDS.winreg:
                        await Task.Run(() => ExecuteWinRegCommand(ttd, true));
                        break;
                    case COMMANDS.sqlite_vacuum:
                        await Task.Run(() => ExecuteSQLiteVacuumCommand(ttd, true));
                        break;
                    #endregion

                    #region // other special commands
                    case COMMANDS.ini:
                        await Task.Run(() => ExecuteIniCommand(ttd, true));
                        break;
                    case COMMANDS.json:
                        await Task.Run(() => ExecuteJSONCommand(ttd, true));
                        break;
                    #endregion

                    #region // Google Chrome commands
                    case COMMANDS.chrome_autofill:
                        await Task.Run(() => ExecuteChromeCommand(ttd, true));
                        break;
                    case COMMANDS.chrome_database_db:
                        await Task.Run(() => ExecuteChromeCommand(ttd, true));
                        break;
                    case COMMANDS.chrome_favicons:
                        await Task.Run(() => ExecuteChromeCommand(ttd, true));
                        break;
                    case COMMANDS.chrome_history:
                        await Task.Run(() => ExecuteChromeCommand(ttd, true));
                        break;
                    case COMMANDS.chrome_keywords:
                        await Task.Run(() => ExecuteChromeCommand(ttd, true));
                        break;
                    #endregion

                    #region // ClamWin
                    case COMMANDS.clamscan:
                        await Task.Run(() => ExecuteClamWinCommand(ttd, true));
                        break;
                    #endregion

                    #region // little registry cleaner
                    case COMMANDS.littleregistry:
                        await Task.Run(() => ExecuteLittleRegistryCleanerCommand(ttd,String.Empty, true));
                        break;
                    #endregion

                    case COMMANDS.clipboard:
                        await Task.Run(() => CommandLogic_Clipboard.I.ExecuteCommand(true));
                        break;

                    case COMMANDS.dupchecker:
                        await Task.Run(() => CommandLogic_DuplicateChecker.I.ScanPath(ttd.FullPathName));
                        DupChecker.FileOperationPanelShow = true;
                        break;
                }
                #endregion
            }

            ShowTotalOperations();

            return true;
        }

        public void DoWork()
        {
            this.TTD.Reverse(); // reverse our Stack

            VMCleanerML.MaxProgress = 0;
            VMCleanerML.ProgressIndex = 0;

            bgWorker.RunWorkerAsync();
        }

        #region Execute
        void ExecuteDeleteCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            if (preview)
            {
                FileInfo fi = new FileInfo(ttd.FullPathName);
                if (fi.Exists)
                {
                    string text = string.Format("{0} {1} {2}", "Delete", Win32API.FormatByteSize(fi.Length), ttd.FullPathName);
                    //PreviewLog += text + "\r\n";

                    UpdateProgressLog(text, text);
                }
            }
            else
            {
                // next we need to know if the file exists so we can delete it.
                FileInfo fi = new FileInfo(ttd.FullPathName);
                if (fi.Exists)
                {
                    try
                    {
                        string text = string.Format("{0} {1} {2}", "Delete", Win32API.FormatByteSize(fi.Length), ttd.FullPathName);
                        // then report to the gui
                        bgWorker.ReportProgress(-1, text);

                        // then we delete it.
                        FileOperations.Delete(fi.FullName);

                        text = string.Format(" - DELETED");
                        // then report to the gui
                        bgWorker.ReportProgress(-1, text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR while deleting a file: " + ex.Message);
                    }
                }

                // delete directories as well if search parameter is walk.all
                if (ttd.search == SEARCH.walk_all)
                {
                    if (this.TTD.Count == 0)
                    {
                        DirectoryInfo di = new DirectoryInfo(ttd.path);
                        if (di.Exists)
                        {
                            FileOperations.I.DeleteEmptyDirectories(ttd.path, (a) =>
                            {
                                string text = string.Format("Delete 0 {0} - DELETED", a);
                                // then report to the gui
                                bgWorker.ReportProgress(-1, text);
                            });
                        }
                    }
                }
            }
        }

        void ExecuteWinRegCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = string.Format("{0} {1} {2}",
                                        "Clean",
                                        ttd.reg_name == null ?
                                            "registry key" :
                                            "registry name",
                                        ttd.reg_name == null ?
                                            ttd.reg_root + "\\" + ttd.reg_subkey :
                                            ttd.reg_root + "\\" + ttd.reg_subkey + "\\" + ttd.reg_name
                                        );

            if (preview)
            {
                //string text = string.Format("{0} {1} {2}",
                //        "Delete",
                //        ttd.reg_name == null ?
                //            "registry key" :
                //            "registry name",
                //        ttd.reg_name == null ?
                //            ttd.reg_root + "\\" + ttd.reg_subkey :
                //            ttd.reg_root + "\\" + ttd.reg_subkey + "\\" + ttd.reg_name
                //        );

                //PreviewLog += text + "\r\n";

                UpdateProgressLog(text, text);
            }
            else
            {
                //string text = string.Format("{0} {1} {2}",
                //        "Delete",
                //        ttd.reg_name == null ?
                //            "registry key" :
                //            "registry name",
                //        ttd.reg_name == null ?
                //            ttd.reg_root + "\\" + ttd.reg_subkey :
                //            ttd.reg_root + "\\" + ttd.reg_subkey + "\\" + ttd.reg_name
                //        );

                bgWorker.ReportProgress(-1, text);

                if (ttd.reg_name == null) // name is empty so 
                {
                    // we are deleting a key
                    RegistryHelper.I.DeleteEntries(ttd.reg_root, ttd.reg_subkey);
                }
                else
                {
                    // we are now deleting a name value
                    RegistryHelper.I.DeleteEntries(ttd.reg_root, ttd.reg_subkey, ttd.reg_name);
                }

                text = string.Format(" - DELETED");
                bgWorker.ReportProgress(-1, text);
            }
        }

        void ExecuteSQLiteVacuumCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "Vacuum {0} {1}";

            FileInfo fi = new FileInfo(ttd.FullPathName);
            if (fi.Exists)
            {
                text = string.Format(text, Win32API.FormatByteSize(fi.Length), ttd.FullPathName);
            }
            else
            {
                return;
            }

            if (preview)
            {
                //PreviewLog += text + "\r\n";

                UpdateProgressLog(text, text);
            }
            else
            {
                bgWorker.ReportProgress(-1, text);

                bool res = SQLite.I.Vacuum(ttd.FullPathName);
                string status = string.Empty;

                if (res)
                {
                    long old_size = fi.Length;
                    fi = new FileInfo(ttd.FullPathName);
                    long new_size = fi.Length;

                    this.TotalFileSize += old_size - new_size;

                    if (this.TotalFileSize > 0)
                    {
                        status = "CLEANED! new size " + Win32API.FormatByteSize(fi.Length);
                    }
                    else
                    {
                        status = "NO CHANGE";
                    }
                }

                text = string.Format(" - " + (res ? status : "NOT CLEANED"));
                bgWorker.ReportProgress(-1, text);
            }
        }

        void ExecuteIniCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "Delete \"{0}\" from \"{1}\" secion in \"{2}\"";

            string val = string.Empty;

            if (ttd.key != null)
            {
                string log = string.Empty;
                val = Win32API.IniHelper.GetValue(ttd.path, ttd.section, ttd.key);
                log = string.Format(text, ttd.key, ttd.section, ttd.path);

                if (!preview)
                {
                    Win32API.IniHelper.SetValue(ttd.path, ttd.section, ttd.key, string.Empty);
                    bgWorker.ReportProgress(-1, log);
                }
                else
                {
                    //PreviewLog += log + "\r\n";
                    UpdateProgressLog(log, log);
                }
            }
            else
            {
                // key is empty, so we need to enumerate keys from the secion
                List<string> keys = new List<string>(Win32API.IniHelper.GetKeyNames(ttd.section, ttd.path));
                //string[] keys = ini.GetKeyNames(ttd.section);
                foreach (string key in keys)
                {
                    string log = string.Empty;
                    val = Win32API.IniHelper.GetValue(ttd.path, ttd.section, ttd.key);
                    log = string.Format(text, key, ttd.section, ttd.path);

                    if (!preview)
                    {
                        Win32API.IniHelper.SetValue(ttd.path, ttd.section, key, string.Empty);
                        bgWorker.ReportProgress(-1, log);
                    }
                    else
                    {
                        //PreviewLog += log + "\r\n";
                        UpdateProgressLog(log, log);
                    }
                }
            }
        }

        void ExecuteJSONCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "Remove JSON entries in \"{0}\" from \"{1}\"";
            string[] add = ttd.address.Split('/');

            if (preview)
            {   
                string log = string.Format(text, add[add.Length - 1], ttd.FullPathName);
                //PreviewLog += log + "\r\n";

                UpdateProgressLog(log, text);
            }
            else
            {
                string log = string.Empty;
                string json = JSON.OpenJSONFiel(ttd.FullPathName);
                bool isfound = JSON.isAddressFound(json, ttd.address);
                if (isfound)
                {
                    log = string.Format(text, add[add.Length - 1], ttd.FullPathName);
                    bgWorker.ReportProgress(-1, log);

                    json = JSON.RemoveElementFromAddress(json, ttd.address);
                    using (StreamWriter writer = new StreamWriter(ttd.FullPathName))
                    {
                        writer.Write(json);
                    }

                    log = " - CLEANED";
                    bgWorker.ReportProgress(-1, log);
                }
            }
        }

        void ExecuteChromeCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string ret = string.Empty;

            string text = "Clean file {0} {1}";

            if (preview)
            {
                FileInfo fi = new FileInfo(ttd.FullPathName);
                if (fi.Exists)
                {
                    string log = string.Format(text, Win32API.FormatByteSize(fi.Length), fi.FullName);
                    //PreviewLog += log + "\r\n";
                    UpdateProgressLog(log, text);
                }
            }
            else
            {
                string log = string.Empty;
                log = string.Format(text, string.Empty, ttd.FullPathName);
                bgWorker.ReportProgress(-1, log);

                switch (ttd.command)
                {
                    case COMMANDS.chrome_autofill:
                        ProgressWorker.I.EnQ("Cleaning Chrome Autofill");
                        ret = CommandLogic_Chrome.CleanAutofill(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_database_db:
                        ProgressWorker.I.EnQ("Cleaning Chrome Database.db");
                        ret = CommandLogic_Chrome.CleanDatabases(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_favicons:
                        ProgressWorker.I.EnQ("Cleaning Chrome Favicons");
                        ret = CommandLogic_Chrome.CleanFavIcons(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_history:
                        ProgressWorker.I.EnQ("Cleaning Chrome History");
                        ret = CommandLogic_Chrome.CleanHistory(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_keywords:
                        ProgressWorker.I.EnQ("Cleaning Chrome Keywords");
                        ret = CommandLogic_Chrome.CleanKeywords(ttd.FullPathName);
                        break;
                }

                if (ret != string.Empty)
                {
                    log = string.Format(" - Not cleaned due to error\r\n\t{0}", ret);
                    bgWorker.ReportProgress(-1, log);
                }
                else
                {
                    bgWorker.ReportProgress(-1, " - Cleaned");
                }
            }
        }

        void ExecuteClamWinCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "Clean {0} {1}";

            if (preview)
            {
                if (ttd.search == SEARCH.clamscan_file)
                {
                    FileInfo fi = new FileInfo(ttd.FullPathName);
                    if (fi.Exists)
                    {
                        string log = string.Format(text, Win32API.FormatByteSize(fi.Length), fi.FullName);
                        //PreviewLog += log + "\r\n";
                        UpdateProgressLog(log, log);

                    }
                }
                else if(ttd.search == SEARCH.clamscan_folder_recurse || ttd.search == SEARCH.clamscan_folder) 
                {
                    if (Directory.Exists(ttd.FullPathName))
                    {
                        string log = string.Format(text, "", ttd.FullPathName);
                        //PreviewLog += log + "\r\n";
                        UpdateProgressLog(log, log);
                    }
                }
            }
            else
            {
                if (ttd.search == SEARCH.clamscan_file)
                {
                    FileInfo fi = new FileInfo(ttd.FullPathName);
                    string log = string.Format(text, Win32API.FormatByteSize(fi.Length), fi.FullName);
                    bgWorker.ReportProgress(-1, log);

                    CommandLogic_Clam.I.LaunchScanner(ttd.search, ttd.FullPathName);

                    log = " - CLEANED";
                    bgWorker.ReportProgress(-1, log);
                }
                else if (ttd.search == SEARCH.clamscan_folder_recurse || ttd.search == SEARCH.clamscan_folder)
                {
                    string log = string.Format(text, "", ttd.FullPathName);
                    bgWorker.ReportProgress(-1, log);

                    CommandLogic_Clam.I.LaunchScanner(ttd.search, ttd.FullPathName, regex: ttd.regex);

                    log = " - CLEANED";
                    bgWorker.ReportProgress(-1, log);
                }
            }
        }


        public DataTable CreateTableSchemaForRegistryStore()
        {
            DataTable dtThingsDeleted = new DataTable("RegistryPaths");
            dtThingsDeleted.Columns.Add("RegistryKeyFullPath");
            dtThingsDeleted.Columns.Add("Location");
            return dtThingsDeleted;

        }
        DataTable dtThingsDeleted = null;
        async Task<bool> ExecuteLittleRegistryCleanerCommand(Model_ThingsToDelete ttd, string RegistryDatSaveFolderPath, bool preview = false)
        {
            try
            {
            string text = "{0} {1}";

            List<ScannerBase.InvalidKeys> BadKeys = new List<ScannerBase.InvalidKeys>();
            string strRegistryDatSaveFolderPath = RegistryDatSaveFolderPath;
            if (!preview && dtThingsDeleted== null)
            {
                dtThingsDeleted = CreateTableSchemaForRegistryStore();
            }

            switch (ttd.search)
            {
                case SEARCH.lrc_activex_com:
                    ActiveXComObjects.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    ActiveXComObjects.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await ActiveXComObjects.I.Clean(preview);
                    BadKeys.AddRange(ActiveXComObjects.I.BadKeys);
                    dtThingsDeleted = ActiveXComObjects.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_app_info:
                    ApplicationInfo.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    ApplicationInfo.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await ApplicationInfo.I.Clean(preview);
                    BadKeys.AddRange(ApplicationInfo.I.BadKeys);
                    dtThingsDeleted = ApplicationInfo.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_progam_location:
                    ApplicationPaths.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    ApplicationPaths.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await ApplicationPaths.I.Clean(preview);
                    BadKeys.AddRange(ApplicationPaths.I.BadKeys);
                    dtThingsDeleted = ApplicationPaths.I.dtRegistryKeyDeleted;

                    break;
                case SEARCH.lrc_software_settings:
                    ApplicationSettings.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    ApplicationSettings.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await ApplicationSettings.I.Clean(preview);
                    BadKeys.AddRange(ApplicationSettings.I.BadKeys);
                    dtThingsDeleted = ApplicationSettings.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_startup:
                    StartupFiles.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    StartupFiles.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await StartupFiles.I.Clean(preview);
                    BadKeys.AddRange(StartupFiles.I.BadKeys);
                    dtThingsDeleted = StartupFiles.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_system_drivers:
                    SystemDrivers.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    SystemDrivers.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await SystemDrivers.I.Clean(preview);
                    BadKeys.AddRange(SystemDrivers.I.BadKeys);
                    dtThingsDeleted = SystemDrivers.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_shared_dll:
                    SharedDLLs.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    SharedDLLs.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await SharedDLLs.I.Clean(preview);
                    BadKeys.AddRange(SharedDLLs.I.BadKeys);
                    dtThingsDeleted = SharedDLLs.I.dtRegistryKeyDeleted;

                    break;
                case SEARCH.lrc_help_file:
                    WindowsHelpFiles.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    WindowsHelpFiles.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await WindowsHelpFiles.I.Clean(preview);
                    BadKeys.AddRange(WindowsHelpFiles.I.BadKeys);
                    dtThingsDeleted = WindowsHelpFiles.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_sound_event:
                    WindowsSounds.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    WindowsSounds.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await WindowsSounds.I.Clean(preview);
                    BadKeys.AddRange(WindowsSounds.I.BadKeys);
                    dtThingsDeleted = WindowsSounds.I.dtRegistryKeyDeleted;

                    break;
                case SEARCH.lrc_history_list:
                    RecentDocs.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    RecentDocs.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await RecentDocs.I.Clean(preview);
                    BadKeys.AddRange(RecentDocs.I.BadKeys);
                    dtThingsDeleted = RecentDocs.I.dtRegistryKeyDeleted;
                    break;
                case SEARCH.lrc_win_fonts:
                    WindowsFonts.I.dtRegistryKeyDeleted = dtThingsDeleted;
                    WindowsFonts.I.strRegistryBackupFolderPath = strRegistryDatSaveFolderPath;
                    await WindowsFonts.I.Clean(preview);
                    BadKeys.AddRange(WindowsFonts.I.BadKeys);
                    dtThingsDeleted = WindowsFonts.I.dtRegistryKeyDeleted;
                    break;
            }

            if(!Preview)

            foreach (ScannerBase.InvalidKeys badkey in BadKeys)
            {
                string log = string.Empty;

                string root = badkey.Root.OpenSubKey(badkey.Subkey).ToString();

                log = string.Format(text, "Clean", root + (badkey.Key != string.Empty ? "\\" + badkey.Key : string.Empty) + ", " + badkey.Name);
                //PreviewLog += log + "\r\n";

                if (preview)
                {
                    UpdateProgressLog(log, log, false);
                }
                else
                {
                    bgWorker.ReportProgress(-1, log);
                }

                this.TotalSpecialOperations++;
            }
            }
            catch (Exception)
            {

                throw;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Enqueque THINGS_TO_DELETE model
        /// </summary>
        /// <param name="ttd"></param>
        public void EnqueTTD(Model_ThingsToDelete ttd)
        {
            var a = from b in this.TTD
                    where b.FullPathName == ttd.FullPathName
                    select b;

            if (a.Count() == 0)
            {
                this.TTD.Enqueue(ttd);

                this.TotalWork = this.TTD.Count;

                if (ttd.WhatKind == THINGS_TO_DELETE.file)
                {
                    if (
                        ttd.command == COMMANDS.sqlite_vacuum || 
                        ttd.command == COMMANDS.ini ||
                        ttd.command == COMMANDS.json ||
                        ttd.command == COMMANDS.chrome_autofill ||
                        ttd.command == COMMANDS.chrome_database_db ||
                        ttd.command == COMMANDS.chrome_favicons ||
                        ttd.command == COMMANDS.chrome_history ||
                        ttd.command == COMMANDS.chrome_keywords
                        )
                    {
                        this.TotalSpecialOperations++;
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(ttd.FullPathName);
                        if (fi.Exists)
                        {
                            this.TotalFileSize += fi.Length;
                            this.TotalFileDelete++;
                        }
                    }
                }
                else if (
                            ttd.WhatKind == THINGS_TO_DELETE.registry_key ||
                            ttd.WhatKind == THINGS_TO_DELETE.registry_name ||
                            ttd.WhatKind == THINGS_TO_DELETE.clamwin ||
                            ttd.WhatKind == THINGS_TO_DELETE.system
                        )
                {
                    this.TotalSpecialOperations++;
                }
            }
        }

        /// <summary>
        /// Clear THINGS_TO_DELETE collection
        /// resets other variables too
        /// </summary>
        public void ClearTTD(bool reset_vars = true)
        {
            if (reset_vars)
            {
                TotalFileDelete = 0;
                TotalFileSize = 0;
                TotalWork = 0;
                TotalSpecialOperations = 0;

                VMCleanerML.MaxProgress = 0;
                VMCleanerML.ProgressIndex = 0;

            }

            this.TTD.Clear();
        }

        void ShowTotalOperations(bool preview = true)
        {
            string final_note = "\r\nDisk space {3}recovered: {0}\r\nFiles {3}deleted: {1}\r\nSpecial operations: {2}";
            string text = string.Empty;

            if (preview)
            {
                text = string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations, "to be ");
            }
            else
            {
                text = string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations, string.Empty);
            }

            //ExecuteLog += string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations);

            ProgressWorker.I.EnQ("Done");
            VMCleanerML.TextLog += text;
            VMCleanerML.ProgressIndex = 0;
            VMCleanerML.MaxProgress = 0;
        }

        void UpdateProgressLog(string WindowLogText, string ProgressText, bool update_progress_text = true)
        {
            if (update_progress_text) ProgressWorker.I.EnQ(ProgressText);

            VMCleanerML.TextLog += WindowLogText;
            VMCleanerML.MaxProgress = this.TTD.Count;
            VMCleanerML.ProgressIndex++;
        }
        #endregion
    }
}
