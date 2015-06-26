using mCleaner.Helpers;
using mCleaner.Helpers.Data;
using mCleaner.Logics.Clam;
using mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace mCleaner.Logics
{
    public class Worker
    {
        BackgroundWorker bgWorker;

        int TotalFileDelete = 0;
        long TotalFileSize = 0;
        int TotalWork = 0;
        int TotalSpecialOperations = 0;

        string _preview_log = string.Empty;
        string _execute_log = string.Empty;

        ViewModel_CleanerML CleanerML
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

        private Queue<Model_ThingsToDelete> _TTD = new Queue<Model_ThingsToDelete>();
        public Queue<Model_ThingsToDelete> TTD
        {
            get { return _TTD; }
            set
            {
                if (_TTD != value)
                {
                    _TTD = value;

                    if (CleanerML.MaxProgress == 0)
                    {
                        CleanerML.MaxProgress = value.Count();
                    }
                }
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

                CleanerML.TextLog = _execute_log;
                CleanerML.ProgressText = e.UserState.ToString();
                CleanerML.MaxProgress = this.TTD.Count;
                CleanerML.ProgressIndex++;
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
                            ExecuteDeleteCommand(ttd);
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
                    }
                }
            }
        }

        public void PreviewWork()
        {
            //this.TTD = new Queue<Model_ThingsToDelete>(this.TTD.Reverse());

            _preview_log = string.Empty;

            foreach (Model_ThingsToDelete ttd in this.TTD)
            {
                switch (ttd.command)
                {
                    case COMMANDS.delete:
                        ExecuteDeleteCommand(ttd, true);
                        break;

                    #region // special commands
                    case COMMANDS.winreg:
                        ExecuteWinRegCommand(ttd, true);
                        break;
                    case COMMANDS.sqlite_vacuum:
                        ExecuteSQLiteVacuumCommand(ttd, true);
                        break;
                    #endregion

                    #region // other special commands
                    case COMMANDS.ini:
                        ExecuteIniCommand(ttd, true);
                        break;
                    case COMMANDS.json:
                        ExecuteJSONCommand(ttd, true);
                        break;
                    #endregion

                    #region // Google Chrome commands
                    case COMMANDS.chrome_autofill:
                        ExecuteChromeCommand(ttd, true);
                        break;
                    case COMMANDS.chrome_database_db:
                        ExecuteChromeCommand(ttd, true);
                        break;
                    case COMMANDS.chrome_favicons:
                        ExecuteChromeCommand(ttd, true);
                        break;
                    case COMMANDS.chrome_history:
                        ExecuteChromeCommand(ttd, true);
                        break;
                    case COMMANDS.chrome_keywords:
                        ExecuteChromeCommand(ttd, true);
                        break;
                    #endregion

                    #region // ClamWin
                    case COMMANDS.clamscan:
                        ExecuteClamWinCommand(ttd, true);
                        break;
                    #endregion

                    #region little registry cleaner
                    case COMMANDS.littleregistry:
                        ExecuteLittleRegistryCleanerCommand(ttd, true);
                        break;
                    #endregion
                }
            }

            ShowTotalOperations();
        }

        public void DoWork()
        {
            this.TTD.Reverse(); // reverse our Stack

            CleanerML.MaxProgress = 0;
            CleanerML.ProgressIndex = 0;
            _execute_log = string.Empty;
            CleanerML.TextLog = _execute_log;

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
                        fi.Delete();

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

        void ExecuteLittleRegistryCleanerCommand(Model_ThingsToDelete ttd, bool preview = false)
        {
            string text = "{0} {1}";

            if (preview)
            {
                List<ScannerBase.InvalidKeys> BadKeys = new List<ScannerBase.InvalidKeys>();
                switch (ttd.search)
                {
                    case SEARCH.lrc_activex_com:
                        break;
                    case SEARCH.lrc_app_info:
                        ApplicationInfo.I.Preview();
                        BadKeys.AddRange(ApplicationInfo.I.BadKeys);
                        
                        break;
                    case SEARCH.lrc_progam_location:
                        ApplicationPaths.I.Preview();
                        BadKeys.AddRange(ApplicationPaths.I.BadKeys);

                        break;
                    case SEARCH.lrc_software_settings:
                        break;
                    case SEARCH.lrc_startup:
                        break;
                    case SEARCH.lrc_system_drivers:
                        break;
                    case SEARCH.lrc_shared_dll:
                        break;
                    case SEARCH.lrc_help_file:
                        break;
                    case SEARCH.lrc_sound_event:
                        break;
                    case SEARCH.lrc_history_list:
                        break;
                    case SEARCH.lrc_win_fonts:
                        break;
                }

                foreach (ScannerBase.InvalidKeys badkey in BadKeys)
                {
                    string log = string.Format(text, "Clean", badkey.Key + ", " + badkey.Name);
                    _preview_log += log + "\r\n";

                    UpdateProgressLog(log);

                    this.TotalSpecialOperations++;
                }
            }
            else
            {
                switch (ttd.search)
                {
                    case SEARCH.lrc_activex_com:
                        break;
                    case SEARCH.lrc_app_info:
                        //ApplicationInfo.I
                        break;
                    case SEARCH.lrc_progam_location:
                        break;
                    case SEARCH.lrc_software_settings:
                        break;
                    case SEARCH.lrc_startup:
                        break;
                    case SEARCH.lrc_system_drivers:
                        break;
                    case SEARCH.lrc_shared_dll:
                        break;
                    case SEARCH.lrc_help_file:
                        break;
                    case SEARCH.lrc_sound_event:
                        break;
                    case SEARCH.lrc_history_list:
                        break;
                    case SEARCH.lrc_win_fonts:
                        break;
                }
            }
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

                CleanerML.MaxProgress = 0;
                CleanerML.ProgressIndex = 0;
                _execute_log = string.Empty;
                CleanerML.TextLog = _execute_log;
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

            Help.RunInBackground(() =>
            {
                CleanerML.ProgressText = "Done";
                CleanerML.TextLog = text;

                CleanerML.ProgressIndex = 0;
                CleanerML.MaxProgress = 0;
            });
        }

        void UpdateProgressLog(string text)
        {
            Help.RunInBackground(() =>
            {
                CleanerML.TextLog = _preview_log;
                CleanerML.ProgressText = text;
                CleanerML.MaxProgress = this.TTD.Count;
                CleanerML.ProgressIndex++;
            }, false);
        }
    }
}
