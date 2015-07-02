using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace mCleaner.Logics.Clam
{
    public class CommandLogic_Clam : iActions
    {
        BackgroundWorker bgWorker = new BackgroundWorker();
        Process update_process = new Process();

        string _preview_log = string.Empty;
        string _exec_path = string.Empty;
        string _exec_clam = string.Empty;
        string _exec_clam_db_path= string.Empty;
        List<string> log = new List<string>();

        public ViewModel_Clam Clam
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Clam>();
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

        public void Execute(bool apply = false)
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

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
                                // oh that's just lame
            string exepath      = e.Argument.ToString().Split('|')[0];
            string args         = e.Argument.ToString().Split('|')[1];

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
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateProgressLog("************************************");
            UpdateProgressLog("DONE                                ");
            UpdateProgressLog("************************************");
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!this.update_process.HasExited)
            {
                UpdateProgressLog(e.Data);
            }
        }

        public void LaunchUpdater()
        {
            this.WriteConfig();

            this.log.Clear();
            this.Clam.ShowClamWinVirusUpdateWindow = true;
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

        public void LaunchScanner(SEARCH search, string path, bool launchinbackground = false, string regex = null)
        {
            List<string> param = new List<string>() {
                "--tempdir \"{0}\"",
                "--keep-mbox",
                "--stdout",
                "--database=\"{1}\"",
                "--log=\"{2}\"",
                "--infected",
                "--show-progress",
                "--kill"
            };

            // if max files are declared
            StringCollection max = Settings.Default.ClamWin_Max;
            string[] limits = {
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
                    param.Add("--memory");
                    break;
            }

            string full_param = string.Join(" ", param.ToArray());
            full_param = string.Format(full_param, 
                Environment.GetEnvironmentVariable("TEMP"), 
                this._exec_clam_db_path,
                Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "clamscan_temp_" + Guid.NewGuid().ToString())
            );

            this.log.Clear();
            this.Clam.WindowTitle = "Scan for virus";
            this.Clam.ShowClamWinVirusUpdateWindow = true;
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
            }
            else
            {
                bgWorker.RunWorkerAsync(Path.Combine(this._exec_clam, "clamscan.exe") + "|" + full_param);
            }

            this.Clam.ShowClamWinVirusUpdateWindow = false;

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
                Settings.Default.ClamWin_DB = Path.Combine(this._exec_clam, ".clamwin\\db");

                string default_db_path = ".clamwin\\db";
                default_db_path = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), default_db_path);
                if (Directory.Exists(default_db_path))
                {
                    MessageBoxResult mbr = MessageBox.Show("mCleaner detected you currently have ClamWin installed in your system. Do you want to use its current database?\r\n\r\nmCleaner strongly suggests that you use mClearner's database for ClamAV to have more control over over the virus definition database.", "mCleaner", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Settings.Default.ClamWin_DB = default_db_path;
                        this._exec_clam_db_path = Settings.Default.ClamWin_DB;
                    }
                }

                Settings.Default.ClamWin_SupressMessageAtStartup = true;
                Settings.Default.Save();
                this._exec_clam_db_path = Settings.Default.ClamWin_DB;
            }
        }

        void UpdateProgressLog(string text)
        {
            bool has_logged = false;

            // check if the first 5 character are the same
            // so we do not add that to our log collection.
            if ((this.log.Count > 0 && text != null && text.Length > 5) && this.log[this.log.Count - 1].Substring(0, 5) == text.Substring(0, 5))
            {
                // replace the last entry to the newest one
                this.log[this.log.Count - 1] = text;
                has_logged = true;
            }
            else
            {
                if (text.Length > 5 && text != null)
                {
                    has_logged = true;
                    this.log.Add(text);
                }
            }

            if (has_logged)
            {
                _preview_log = string.Join("\r\n", this.log.ToArray());

                Help.RunInBackground(() =>
                {
                    Clam.VirusDefUpdateLog = _preview_log;
                }, false);
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
    }
}
