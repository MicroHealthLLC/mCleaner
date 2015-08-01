using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

using CodeBureau;

using mCleaner.Helpers;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;

using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics.Clam
{
    public class CommandLogic_Clam : iActions
    {
        #region local vars
        BackgroundWorker bgWorker = new BackgroundWorker();
        Process update_process = new Process();

        string _preview_log = string.Empty;
        string _exec_path = string.Empty;
        string _exec_clam = string.Empty;
        string _exec_clam_db_path= string.Empty;
        List<string> log = new List<string>();

        public bool isUpdate = false;
        bool IsRemove = false;
        #endregion

        #region properties
        public ViewModel_Clam Clam
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Clam>();
            }
        }
        public ViewModel_Preferences Prefs
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Preferences>();
            }
        }

        private action _Action = new action();
        public action Action
        {
            get { return _Action; }
            set
            {
                if (_Action != value)
                {
                    _Action = value;
                }
            }
        }
        #endregion

        #region ctor
        public CommandLogic_Clam()
        {
            this._exec_path = System.AppDomain.CurrentDomain.BaseDirectory;
            this._exec_clam = Path.Combine(this._exec_path, "Clam");

            this._exec_clam_db_path = Settings.Default.ClamWin_DB;  //Path.Combine(this._exec_clam, ".clamwin\\db");

            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
        }
        static CommandLogic_Clam _i = new CommandLogic_Clam();
        public static CommandLogic_Clam I { get { return _i; } }
        #endregion

        #region events
        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // oh that's just lame
            string exepath = e.Argument.ToString().Split('|')[0];
            string args = e.Argument.ToString().Split('|')[1];

            //ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(this._exec_clam, "freshclam.exe"), e.Argument.ToString())
            ProcessStartInfo startInfo = new ProcessStartInfo(exepath, args)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            update_process = new Process()
            {
                StartInfo = startInfo
            };
            update_process.OutputDataReceived += process_OutputDataReceived;
            update_process.Start();
            update_process.BeginOutputReadLine();
            update_process.WaitForExit();

            // check if we ran freshclam to update virus definition database
            if (exepath.Contains("freshclam.exe"))
            {
                // se we can save the date
                Settings.Default.ClamWin_Update = false;
                Settings.Default.ClamWin_LastDBUpdate = DateTime.Now;
                Settings.Default.Save();
            }
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateProgressLog("************************************");
            UpdateProgressLog("DONE                                ");
            UpdateProgressLog("************************************");

            this.Clam.ShowClamWinVirusUpdateWindow = false;

            if (this.Clam.InfectedFilesCollection.Count == 0)
            {
                UpdateProgressLog("No virus found");
                this.Clam.EnableCleanNowButton = false;
                this.Clam.EnableCancelButton = false;
                this.Clam.EnableCloseButton = true;
            }
            else
            {
                // if virus found
                this.Clam.EnableCleanNowButton = true;
                this.Clam.EnableCancelButton = false;
                this.Clam.EnableCloseButton = true;

                if (this.IsRemove)
                {
                    UpdateProgressLog(string.Format("{0} virus removed", this.Clam.InfectedFilesCollection.Count));
                }
                else
                {
                    UpdateProgressLog(string.Format("{0} virus found! Click 'Clean Now'!", this.Clam.InfectedFilesCollection.Count));
                }
            }
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            if (!this.update_process.HasExited)
            {
                UpdateProgressLog(e.Data);
            }
        }
        #endregion

        #region methods
        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.clamscan_file:
                    EnqueueFile();
                    break;
                case SEARCH.clamscan_folder:
                    EnqueueFilesInFolder(false);
                    break;
                case SEARCH.clamscan_folder_recurse:
                    EnqueueFilesInFolder(true);
                    break;
                case SEARCH.clamscan_memory:
                    break;
            }
        }

        public void EnqueueFile()
        {
            FileInfo fi = new FileInfo(Action.path);
            if (fi.Exists)
            {
                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = fi.FullName,
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.file,
                    command = COMMANDS.clamscan,
                    search = SEARCH.clamscan_file,
                });
            }
        }

        public void EnqueueFilesInFolder(bool isRecursion)
        {
            DirectoryInfo di = new DirectoryInfo(Action.path);
            if (di.Exists)
            {
                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = Action.path,
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.clamwin,
                    command = COMMANDS.clamscan,
                    search = isRecursion ? SEARCH.clamscan_folder_recurse : SEARCH.clamscan_folder,
                    regex = Action.regex
                });

                //if (!isRecursion)
                //{
                //    IEnumerable<string> files = Directory.EnumerateFileSystemEntries(di.FullName);
                //    List<string> files_to_scan = new List<string>();
                //    foreach (string file in files)
                //    {
                //        if (Action.regex != null)
                //        {
                //            string regex = Action.regex;
                //            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
                //            Regex reg = new Regex(regex, options);
                //            if (reg.IsMatch(file))
                //            {
                //                files_to_scan.Add(file);
                //            }
                //        }
                //        else
                //        {
                //            files_to_scan.Add(file);
                //        }
                //    }

                //    foreach (string file in files_to_scan)
                //    {
                //        FileInfo fi = new FileInfo(file);
                //        if (fi.Exists)
                //        {
                //            Worker.I.EnqueTTD(new Model_ThingsToDelete()
                //            {
                //                FullPathName = fi.FullName,
                //                IsWhitelisted = false,
                //                OverWrite = false,
                //                WhatKind = THINGS_TO_DELETE.clamwin,
                //                command = COMMANDS.clamscan,
                //                search = SEARCH.clamscan_folder,
                //                regex = Action.regex
                //            });
                //        }
                //    }
                //}
                //else
                //{
                //    Worker.I.EnqueTTD(new Model_ThingsToDelete()
                //    {
                //        FullPathName = Action.path,
                //        IsWhitelisted = false,
                //        OverWrite = false,
                //        WhatKind = THINGS_TO_DELETE.clamwin,
                //        command = COMMANDS.clamscan,
                //        search = SEARCH.clamscan_folder_recurse
                //    });
                //}
            }
        }

        /// <summary>
        /// Scan custom paths
        /// </summary>
        public void LaunchCleaner(bool remove = false)
        {
            if (Settings.Default.ClamWin_ScanLocations.Count != 0)
            {
                this.Clam.ShowClamWinVirusScanner = true;
                // get custom paths from settings
                List<string> paths = new List<string>();
                foreach (string path in Settings.Default.ClamWin_ScanLocations)
                {
                    paths.Add(path);
                }

                this.IsRemove = remove;
                LaunchScanner(SEARCH.clamscan_folderfile, string.Join("|", paths.ToArray()), true, remove: remove);
            }
            else
            {
                MessageBox.Show("You do not have folder selected where to scan for a virus.", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Prefs.ShowWindow = true;
                this.Prefs.SelectedTabIndex = 2;
            }
        }

        public void LaunchUpdater()
        {
            this.WriteConfig();

            this.log.Clear();
            this.Clam.ShowClamWinVirusUpdateWindow = true;
            this.Clam.EnableCancelButton = true;
            this.Clam.EnableCloseButton = true;
            this.Clam.WindowTitle = "Update Virus Definition Database";
            UpdateProgressLog("╔═══════════════════════════════════════════════╗");
            UpdateProgressLog("║ Starting to update virus definition database. ║");
            UpdateProgressLog("╚═══════════════════════════════════════════════╝");

            string[] param = {
                                 "--stdout",
                                 "--datadir=\"{0}\"",
                                 "--config-file=\"{1}\"",
                                 "--log=\"{2}\""
                             };
            string fullparam = string.Join(" ", param);
            fullparam = string.Format(fullparam,
                                        this._exec_clam_db_path,
                                        Path.Combine(this._exec_clam, "freshclam.conf"),
                                        Path.Combine(this._exec_clam, "freshclam.log")
                                     );

            bgWorker.RunWorkerAsync(Path.Combine(this._exec_clam, "freshclam.exe") + "|" + fullparam);
        }

        public void LaunchScanner(SEARCH search, string path, bool launchinbackground = false, string regex = null, bool remove = false)
        {
            this.Clam.InfectedFilesCollection.Clear();
            this.Clam.WindowTitle = "Scan custom locations for viruses";
            this.Clam.EnableCleanNowButton = false;
            this.Clam.EnableCancelButton = true;
            this.Clam.EnableCloseButton = true;

            this.WriteConfig();

            List<string> param = new List<string>() {
                //"--tempdir \"{0}\"",
                //"--keep-mbox",
                //"--stdout",
                "--database=\"{1}\"",
                "--log=\"{2}\"",
                "--infected",
                "--show-progress",
                "--kill",
                "--recursive=yes"
            };

            if (remove)
            {
                param.Add("--remove=yes");
            }

            // if max files are declared
            StringCollection max = Settings.Default.ClamWin_Max;
            string[] limits  = {
                                  "--max-files=" + max[0],
                                  "--max-scansize=" + max[1],
                                  "--max-recursion=" + max[2],
                                  "--max-filesize=" + max[3]
                              };

            // exclusions
            string exclude = string.Empty;
            List<string> list_exclude = new List<string>();
            foreach (string e in Settings.Default.ClamWin_Exclude)
            {
                list_exclude.Add(string.Format("--exclude=\"{0}\"", e));
            }
            exclude = string.Join(" ", list_exclude.ToArray());

            // add additional parameters
            if (regex != null)
            {
                param.Add("--include=\"" + regex + "\"");
            }
            param.Add(string.Join(" ", limits));

            switch (search)
            {
                case SEARCH.clamscan_file:
                    param.Add("\"" + path + "\"");
                    break;
                case SEARCH.clamscan_folder:
                    param.Add(exclude);
                    param.Add("\"" + path + "\"");
                    break;
                case SEARCH.clamscan_folder_recurse:
                    param.Add("--recursive");
                    param.Add(exclude);
                    param.Add("\"" + path + "\"");
                    break;
                case SEARCH.clamscan_memory:
                    this.Clam.WindowTitle = "Scan memory for viruses";
                    param.Add("--memory");
                    break;
                case SEARCH.clamscan_folderfile:
                    string[] locs = path.Split('|');
                    foreach (string loc in locs)
                    {
                        param.Add("\"" + loc + "\"");
                    }
                    break;
            }

            string full_param = string.Join(" ", param.ToArray());
            full_param = string.Format(full_param, 
                Environment.GetEnvironmentVariable("TEMP"), 
                this._exec_clam_db_path,
                Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "clamscan_temp_" + Guid.NewGuid().ToString())
            );

            this.log.Clear();
            UpdateProgressLog("╔═══════════════════════════════════════════════╗");
            UpdateProgressLog("║ Starting to scan files and folders for virus. ║");
            UpdateProgressLog("╚═══════════════════════════════════════════════╝");

            if (!launchinbackground)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(this._exec_clam, "clamscan.exe"), full_param)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };

                update_process = new Process()
                {
                    StartInfo = startInfo
                };
                update_process.OutputDataReceived += process_OutputDataReceived;
                update_process.Start();
                update_process.BeginOutputReadLine();
                update_process.WaitForExit();

                this.Clam.ShowClamWinVirusUpdateWindow = false;
            }
            else
            {
                if (!bgWorker.IsBusy)
                {
                    bgWorker.RunWorkerAsync(Path.Combine(this._exec_clam, "clamscan.exe") + "|" + full_param);
                }
                else
                {
                    MessageBox.Show("ClamAV is currently busy", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            //bgWorker.RunWorkerAsync(full_param);
        }

        public void CancelUpdate()
        {
            if (!this.update_process.HasExited)
            {
                this.update_process.Kill();
                UpdateProgressLog("Process terminated.");
            }
        }

        public void CheckClamWinInstallation()
        {
            if (string.IsNullOrEmpty(Settings.Default.ClamWin_DB))
            {
                // override setting first
                Settings.Default.ClamWin_DB = Path.Combine(this._exec_clam, "db");

                //string default_db_path = "db";
                //default_db_path = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), default_db_path);
                //if (Directory.Exists(default_db_path))
                //{
                //    MessageBoxResult mbr = MessageBox.Show("mCleaner detected you currently have ClamWin installed in your system. Do you want to use its current database?\r\n\r\nmCleaner strongly suggests that you use mClearner's database for ClamAV to have more control over over the virus definition database.", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Information);
                //    if (mbr == MessageBoxResult.Yes)
                //    {
                //        Settings.Default.ClamWin_DB = default_db_path;
                //        this._exec_clam_db_path = Settings.Default.ClamWin_DB;
                //    }
                //}

                Settings.Default.ClamWin_SupressMessageAtStartup = true;
                Settings.Default.Save();
                this._exec_clam_db_path = Settings.Default.ClamWin_DB;
            }

            // check for .cvd files
            string maincvd = Path.Combine(this._exec_clam_db_path, "main.cvd");
            string dailycvd = Path.Combine(this._exec_clam_db_path, "daily.cvd");

                                       // not sure if daily.cvd is necessary
            if (!File.Exists(maincvd)) // && !File.Exists(dailycvd))
            {
                MessageBox.Show("mCleaner has to update the virus definitions.", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.ClamWin_Update = true;
            }
            else
            {
                // if they exists then let's check how many days since the last update

                if (Settings.Default.ClamWin_LastDBUpdate.Ticks > 0)
                {
                    TimeSpan timespan = DateTime.Now - Settings.Default.ClamWin_LastDBUpdate;
                    if (timespan.TotalDays >= Settings.Default.ClamWin_DaysBeforeNotifyToUpdate)
                    {
                        MessageBox.Show(string.Format("It's been {0} days since the last time you updated the virus definitions.", (int)timespan.TotalDays), "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                        Settings.Default.ClamWin_Update = true;
                    }
                }
                else
                {
                    //MessageBox.Show(string.Format("mCleaner was not able to get the date ", ""), "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        void UpdateProgressLog(string data)
        {
            if (this.isUpdate == false)
            {
                #region virus scanner
                // check for infected files
                if (!string.IsNullOrEmpty(data))
                {
                    if (data.Length > 10) // make sure we have enough string to look for "FOUND" string
                    {
                        if (data.Substring(data.Length - 5, 5) == "FOUND")
                        {
                            string file = data.Substring(0, data.IndexOf(": "));
                            string virus = data.Substring(data.IndexOf(": ") + 2).Replace(" FOUND", null);

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.Clam.InfectedFilesCollection.Add(new Model_VirusDetails()
                                {
                                    File = file,
                                    VirusName = virus,
                                    ColWidth = -1,
                                    Status = this.IsRemove ? "Removed" : "Infected!"
                                });
                            }));
                        }
                    }
                }

                this.Clam.ProgressText = data;
                Debug.WriteLine(data);
                #endregion
            }
            else
            {
                #region update
                bool has_logged = false;

                if (data == null) return;

                if (log.Count > 0)
                {
                    string last_text = log[log.Count - 1];
                    string new_text = data;

                    if (last_text.Length != new_text.Length) // if they are not in the same lenght, obviously the new text is different
                    {
                        log.Add(new_text);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(new_text))
                        {
                            if (last_text[0] != new_text[0]) // if both 1st character were not the same, obiosuly the new text is different
                            {
                                log.Add(new_text);
                            }
                            else
                            {
                                string temp = string.Empty;
                                string diff = string.Empty;

                                for (int i = 0; i < last_text.Length; i++)
                                {
                                    if (last_text[i] != new_text[i])
                                    {
                                        temp = new_text.Substring(0, i);
                                        diff = new_text.Substring(i);
                                        break;
                                    }
                                }

                                if (temp == last_text.Substring(0, temp.Length))
                                {
                                    log[log.Count - 1] = new_text;
                                }
                            }
                        }
                        else
                        {
                            log.Add(new_text);
                        }
                    }
                }
                else
                {
                    log.Add(data);
                }

                //if (has_logged)
                {
                    _preview_log = string.Join("\r\n", this.log.ToArray());

                    Help.RunInBackground(() =>
                    {
                        Clam.VirusDefUpdateLog = _preview_log;
                    }, false);
                }
                #endregion
            }
        }

        public void WriteConfig()
        {
            var utf8wobom = new System.Text.UTF8Encoding(false);
            using (var a = new StreamWriter(Path.Combine(this._exec_clam, "freshclam.conf"), false, utf8wobom))
            {
                List<string> config = new List<string>();

                config.Add("DNSDatabaseInfo current.cvd.clamav.net");

                if (Settings.Default.ClamWin_DatabaseMirror != string.Empty)
                {
                    config.Add("DatabaseMirror " + Settings.Default.ClamWin_DatabaseMirror);
                }

                if (Settings.Default.ClamWin_Proxy_Address != string.Empty)
                {
                    string[] proxy = Settings.Default.ClamWin_Proxy_Address.Split(':');

                    config.Add("HTTPProxyServer " + proxy[0]);
                    config.Add("HTTPProxyPort " + proxy[1]);
                }

                if (Settings.Default.ClamWin_Proxy_UserPass != string.Empty)
                {
                    string[] userpass = Settings.Default.ClamWin_Proxy_UserPass.Split(':');

                    config.Add("HTTPProxyUsername " + userpass[0]);
                    config.Add("HTTPProxyPassword " + userpass[1]);
                }

                a.Write(string.Join("\r\n", config.ToArray()));
            }
        }
        #endregion
    }
}
