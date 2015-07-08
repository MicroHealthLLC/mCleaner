using mCleaner.Helpers;
using mCleaner.Helpers.Data;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mCleaner.Logics
{
    public class Worker
    {
        #region vars
        BackgroundWorker bgWorker;

        int TotalFileDelete = 0;
        long TotalFileSize = 0;
        int TotalWork = 0;
        int TotalSpecialOperations = 0;

        string _preview_log = string.Empty;
        string _execute_log = string.Empty;
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
        }

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                _execute_log += e.UserState.ToString();

                VMCleanerML.TextLog = _execute_log;
                VMCleanerML.ProgressText = e.UserState.ToString();
                VMCleanerML.MaxProgress = this.TTD.Count;
                VMCleanerML.ProgressIndex++;
            }
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (this.TTD.Count != 0)
            {
                if (bgWorker.CancellationPending) break;

                if (!this.Preview)
                {
                    Model_ThingsToDelete ttd = this.TTD.Dequeue();

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
                            ExecuteLittleRegistryCleanerCommand(ttd, false);
                            break;
                        #endregion
                    }
                }
            }
        }
        #endregion

        #region methods
        public async Task<bool> PreviewWork()
        {
            //this.TTD = new Queue<Model_ThingsToDelete>(this.TTD.Reverse());

            _preview_log = string.Empty;

            foreach (Model_ThingsToDelete ttd in this.TTD)
            {
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

                    #region little registry cleaner
                    case COMMANDS.littleregistry:
                        await Task.Run(() => ExecuteLittleRegistryCleanerCommand(ttd, true));
                        break;
                    #endregion
                }
            }

            ShowTotalOperations();

            return true;
        }

        public void DoWork()
        {
            this.TTD.Reverse(); // reverse our Stack

            VMCleanerML.MaxProgress = 0;
            VMCleanerML.ProgressIndex = 0;
            _execute_log = string.Empty;
            VMCleanerML.TextLog = _execute_log;

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
                    _preview_log += text + "\r\n";

                    UpdateProgressLog(text);
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

                        text = string.Format(" - DELETED\r\n");
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
                                string text = string.Format("Delete 0 {0} - DELETED\r\n", a);
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

                _preview_log += text + "\r\n";

                UpdateProgressLog(text);
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

                text = string.Format(" - DELETED\r\n");
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

            if (preview)
            {
                _preview_log += text + "\r\n";

                UpdateProgressLog(text);
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

                text = string.Format(" - " + (res ? status : "NOT CLEANED") + "\r\n");
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
                    bgWorker.ReportProgress(-1, log + "\r\n");
                }
                else
                {
                    _preview_log += log + "\r\n";
                    UpdateProgressLog(log);
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
                        bgWorker.ReportProgress(-1, log + "\r\n");
                    }
                    else
                    {
                        _preview_log += log + "\r\n";
                        UpdateProgressLog(log);
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
                _preview_log += log + "\r\n";

                UpdateProgressLog(text);
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

                    log = " - CLEANED\r\n";
                    bgWorker.ReportProgress(-1, log);
                }
            }
        }

        void ExecuteChromeCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "Clean file {0} {1}";

            if (preview)
            {
                FileInfo fi = new FileInfo(ttd.FullPathName);
                if (fi.Exists)
                {
                    string log = string.Format(text, Win32API.FormatByteSize(fi.Length), fi.FullName);
                    _preview_log += log + "\r\n";
                    UpdateProgressLog(text);
                }
            }
            else
            {
                switch (ttd.command)
                {
                    case COMMANDS.chrome_autofill:
                        CommandLogic_Chrome.CleanAutofill(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_database_db:
                        CommandLogic_Chrome.CleanDatabases(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_favicons:
                        CommandLogic_Chrome.CleanFavIcons(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_history:
                        CommandLogic_Chrome.CleanHistory(ttd.FullPathName);
                        break;
                    case COMMANDS.chrome_keywords:
                        CommandLogic_Chrome.CleanKeywords(ttd.FullPathName);
                        break;
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
                        _preview_log += log + "\r\n";
                        UpdateProgressLog(text);

                    }
                }
                else if(ttd.search == SEARCH.clamscan_folder_recurse || ttd.search == SEARCH.clamscan_folder) 
                {
                    if (Directory.Exists(ttd.FullPathName))
                    {
                        string log = string.Format(text, "", ttd.FullPathName);
                        _preview_log += log + "\r\n";
                        UpdateProgressLog(text);
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

                    log = " - CLEANED\r\n";
                    bgWorker.ReportProgress(-1, log);
                }
                else if (ttd.search == SEARCH.clamscan_folder_recurse || ttd.search == SEARCH.clamscan_folder)
                {
                    string log = string.Format(text, "", ttd.FullPathName);
                    bgWorker.ReportProgress(-1, log);

                    CommandLogic_Clam.I.LaunchScanner(ttd.search, ttd.FullPathName, regex: ttd.regex);

                    log = " - CLEANED\r\n";
                    bgWorker.ReportProgress(-1, log);
                }
            }
        }

        async Task<bool> ExecuteLittleRegistryCleanerCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "{0} {1}";

            List<ScannerBase.InvalidKeys> BadKeys = new List<ScannerBase.InvalidKeys>();
            switch (ttd.search)
            {
                case SEARCH.lrc_activex_com:
                    await ActiveXComObjects.I.Clean(preview);
                    BadKeys.AddRange(ActiveXComObjects.I.BadKeys);

                    break;
                case SEARCH.lrc_app_info:
                    await ApplicationInfo.I.Clean(preview);
                    BadKeys.AddRange(ApplicationInfo.I.BadKeys);

                    break;
                case SEARCH.lrc_progam_location:
                    await ApplicationPaths.I.Clean(preview);
                    BadKeys.AddRange(ApplicationPaths.I.BadKeys);

                    break;
                case SEARCH.lrc_software_settings:
                    await ApplicationSettings.I.Clean(preview);
                    BadKeys.AddRange(ApplicationSettings.I.BadKeys);

                    break;
                case SEARCH.lrc_startup:
                    await StartupFiles.I.Clean(preview);
                    BadKeys.AddRange(StartupFiles.I.BadKeys);

                    break;
                case SEARCH.lrc_system_drivers:
                    await SystemDrivers.I.Clean(preview);
                    BadKeys.AddRange(SystemDrivers.I.BadKeys);

                    break;
                case SEARCH.lrc_shared_dll:
                    await SharedDLLs.I.Clean(preview);
                    BadKeys.AddRange(SharedDLLs.I.BadKeys);

                    break;
                case SEARCH.lrc_help_file:
                    await WindowsHelpFiles.I.Clean(preview);
                    BadKeys.AddRange(WindowsHelpFiles.I.BadKeys);

                    break;
                case SEARCH.lrc_sound_event:
                    await WindowsSounds.I.Clean(preview);
                    BadKeys.AddRange(WindowsSounds.I.BadKeys);

                    break;
                case SEARCH.lrc_history_list:
                    await RecentDocs.I.Clean(preview);
                    BadKeys.AddRange(RecentDocs.I.BadKeys);

                    break;
                case SEARCH.lrc_win_fonts:
                    await WindowsFonts.I.Clean(preview);
                    BadKeys.AddRange(WindowsFonts.I.BadKeys);

                    break;
            }

            foreach (ScannerBase.InvalidKeys badkey in BadKeys)
            {
                string log = string.Empty;

                string root = badkey.Root.OpenSubKey(badkey.Subkey).ToString();

                log = string.Format(text, "Clean", root + (badkey.Key != string.Empty ? "\\" + badkey.Key : string.Empty) + ", " + badkey.Name);
                _preview_log += log + "\r\n";

                if (preview)
                {
                    UpdateProgressLog(log, false);
                }
                else
                {
                    bgWorker.ReportProgress(-1, log + "\r\n");
                }

                this.TotalSpecialOperations++;
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
                            ttd.WhatKind == THINGS_TO_DELETE.clamwin
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
                _execute_log = string.Empty;
                VMCleanerML.TextLog = _execute_log;
            }

            this.TTD.Clear();
        }

        void ShowTotalOperations(bool preview = true)
        {
            string final_note = "\r\nDisk space recovered: {0}\r\nFiles deleted: {1}\r\nSpecial operations: {2}";
            string text = string.Empty;

            if (preview)
            {
                _preview_log += string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations);
                text = _preview_log;
            }
            else
            {
                _execute_log += string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations);
                text = _execute_log;
            }

            _execute_log += string.Format(final_note, Win32API.FormatByteSize(this.TotalFileSize), this.TotalFileDelete, this.TotalSpecialOperations);

            ProgressWorker.I.EnQ("Done");
            VMCleanerML.TextLog = text;
            VMCleanerML.ProgressIndex = 0;
            VMCleanerML.MaxProgress = 0;
        }

        void UpdateProgressLog(string text, bool update_progress_text = true)
        {
            if(update_progress_text) ProgressWorker.I.EnQ(text);

            VMCleanerML.TextLog = _preview_log;
            VMCleanerML.MaxProgress = this.TTD.Count;
            VMCleanerML.ProgressIndex++;
        }
        #endregion
    }
}
