using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_DuplicateChecker : CommandLogic_Base, iActions
    {
        #region vars

        #endregion

        #region properties
        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }
        #endregion

        #region commands

        #endregion
        
        #region ctor
        public CommandLogic_DuplicateChecker()
        {

        }
        private static CommandLogic_DuplicateChecker _i = new CommandLogic_DuplicateChecker();
        public static CommandLogic_DuplicateChecker I { get { return _i; } }
        #endregion

        #region command methods

        #endregion

        #region methods
        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.dupchecker_all:
                    EnqueueCustomPath(Action.path);
                    break;
            }
        }

        public void EnqueueCustomPath(string path)
        {
            // enqueue file for deletion
            Worker.I.EnqueTTD(new Model_ThingsToDelete()
            {
                FullPathName = path,
                IsWhitelisted = false,
                OverWrite = false,
                WhatKind = THINGS_TO_DELETE.system,
                command = COMMANDS.dupchecker,
                search = SEARCH.dupchecker_all,
                path = string.Empty,
                level = Action.parent_option.level,
                cleaner_name = Action.parent_option.label
            });
        }

        /// <summary>
        ///  Check duplicates from custom paths. This will called from clicking the toolbar button "Duplicate Checker"
        /// </summary>
        public async Task CheckDuplicates()
        {
            this.DupChecker.DupplicateCollection.Clear();
            int i = 0;
            foreach (string path in Settings.Default.DupChecker_CustomPath)
            {
                //option o = new option()
                //{
                //    id = "duplicate_checker_" + (i++),
                //    label = path.Substring(path.LastIndexOf("\\") + 1),
                //    description = "Check for duplicate entries in " + path,
                //    warning = "This option is slow!",
                //    action = new List<action>()
                //};

                //this.Action = new action()
                //{
                //    command = "dupchecker",
                //    search = "dupchecker.all",
                //    path = path,
                //    level = 2,
                //    parent_option = o,
                //};

                //EnqueueCustomPath(path);
                await Task.Run(() => ScanPath(path));
            }

            this.DupChecker.EnableRemoveDuplicates = true;
            this.DupChecker.EnableScanFolder = true;
            this.DupChecker.EnableSelectFolder = true;
        }

        public void ScanPath(string path)
        {
            List<string> files = FileOperations.I.GetFilesRecursive(path, null, new Action<string>((s) =>
            {
                if (this.DupChecker.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Cancelled");
                }
                else
                    ProgressWorker.I.EnQ("Retreiving files in: " + s + "|1");
            }));

            //Dictionary<string, List<string>> files_with_same_size = new Dictionary<string,List<string>>();
            Dictionary<long, List<string>> files_with_same_size = new Dictionary<long,List<string>>();
            ProgressWorker.I.EnQ("Checking for same file size. This may take a while" + "|1"); // to minimize work load looking for duplicate files
            //base.VMCleanerML.MaxProgress = files.Count;
            //base.VMCleanerML.ProgressIndex = 0;
            this.DupChecker.ProgressMax = files.Count;
            this.DupChecker.ProgressIndex = 0;
            foreach (string file in files)
            {
                if (this.DupChecker.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Cancelled");
                    break;
                }
                bool add = true;

                FileInfo fi = new FileInfo(file);
                if (Settings.Default.DupChecker_MinSize != 0)
                {
                    if (fi.Length < Settings.Default.DupChecker_MinSize * 1000) add = false;
                }
                if (Settings.Default.DupChecker_MaxSize != 0)
                {
                    if (fi.Length > Settings.Default.DupChecker_MaxSize * 1000) add = false;
                }

                if (Settings.Default.DupChecker_FileExtensions != "*.*")
                {
                    string[] exts = Settings.Default.DupChecker_FileExtensions.Split(';');
                    if (!exts.Contains("*" + fi.Extension.ToLower())) add = false;
                }

                if (fi.Length > 0) // do not include 0 length files.
                {
                    if (add)
                    {
                        if (files_with_same_size.ContainsKey(fi.Length))
                        {
                            files_with_same_size[fi.Length].Add(fi.FullName);
                        }
                        else
                        {
                            files_with_same_size.Add(fi.Length, new List<string>() { fi.FullName });
                        }
                    }
                }

                this.DupChecker.ProgressIndex++;
            }

            this.DupChecker.ProgressIndex = 0;

            ProgressWorker.I.EnQ("Please wait while hashing files. This may take a while" + "|1");
            // get all the files we need to hash
            List<string> files_to_hash = new List<string>();
            foreach (long filesize in files_with_same_size.Keys)
            {
                if (this.DupChecker.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Cancelled");
                    break;
                }
                if (files_with_same_size[filesize].Count > 1)
                {
                    files_to_hash.AddRange(files_with_same_size[filesize].ToArray());
                }

                this.DupChecker.ProgressIndex++;
            }

            List<string> hashed_files = new List<string>();
            this.DupChecker.ProgressMax = files_to_hash.Count;
            this.DupChecker.ProgressIndex = 0;

            //if (files_to_hash.Count / 5 > 50)
            //{
            //    List<Task> task_list = new List<Task>();
            //    int index = 0;
            //    int max = files_to_hash.Count / 5;
            //    for (i = 0; i < 5; i++)
            //    {
            //        var task = new Task(new Action(() =>
            //        {
            //            for (int j = index; j < max; j++)
            //            {
            //                if (j < files_to_hash.Count)
            //                {
            //                    string hash = FileOperations.I.HashFile(files_to_hash[j]);
            //                    ProgressWorker.I.EnQ("Hashing: " + files_to_hash[j] + " > " + hash + "|1");
            //                    hashed_files.Add(files_to_hash[j] + "|" + hash);
            //                }
            //            }
            //        }));
            //        task.Start();
            //        task_list.Add(task);
            //        index += max;
            //    }

            //    await Task.WhenAll(task_list.ToArray());
            //}
            //else
            {
                foreach (string filename in files_to_hash)
                {
                    try
                    {
                        string hash = FileOperations.I.HashFile(filename);
                        ProgressWorker.I.EnQ("Hashing: " + filename + " > " + hash + "|1");
                        hashed_files.Add(filename + "|" + hash);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    this.DupChecker.ProgressIndex++;
                }
            }


            ProgressWorker.I.EnQ("Finalizing ..." + "|1");
            Dictionary<string, List<string>> files_with_same_hash = new Dictionary<string, List<string>>();
            this.DupChecker.ProgressMax = hashed_files.Count;
            this.DupChecker.ProgressIndex = 0;

            foreach (string hashedfile in hashed_files)
            {
                string[] tmp = hashedfile.Split('|');
                string file = tmp[0];
                string hash = tmp[1];

                if (files_with_same_hash.ContainsKey(hash))
                {
                    if (!files_with_same_hash[hash].Contains(file))
                    {
                        files_with_same_hash[hash].Add(file);
                    }
                }
                else
                {
                    files_with_same_hash.Add(hash, new List<string>());
                    files_with_same_hash[hash].Add(file);
                }
                this.DupChecker.ProgressIndex++;
            }

            List<string> teremove = new List<string>();
            foreach (string key in files_with_same_hash.Keys)
            {
                if (files_with_same_hash[key].Count == 1) teremove.Add(key);
            }
            foreach(string key in teremove)
            {
                files_with_same_hash.Remove(key);
            }

            ProgressWorker.I.EnQ("Adding to collection for previewing" + "|1");
            
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                this.DupChecker.ProgressMax = files_with_same_hash.Count;
                this.DupChecker.ProgressIndex = 0;
                foreach (string entry in files_with_same_hash.Keys)
                {
                    for (int i = 0; i < files_with_same_hash[entry].ToArray().Length; i++)
                    {
                        string file_entries = files_with_same_hash[entry][i];
                        FileInfo fi = new FileInfo(file_entries);
                        Model_DuplicateChecker e = new Model_DuplicateChecker();
                        e.Hash = entry;
                        e.Selected = i != 0;
                        e.FileDetails = new Model_DuplicateChecker_FileDetails()
                        {
                            Filename = fi.Name,
                            Fullfilepath = fi.FullName,
                            ParentDirectory = fi.Directory.FullName
                        };
                        DupChecker.DupplicateCollection.Add(e);
                    }
                }
            });
            

            // clear some memory
            files.Clear();
            files_with_same_size.Clear();
            files_to_hash.Clear();
            hashed_files.Clear();
            files_with_same_hash.Clear();
            ProgressWorker.I.EnQ("Done click on remove duplicates to remove files.");
        }

        public void Start(ObservableCollection<Model_DuplicateChecker> files, int operation = 0, BackgroundWorker bgWorker = null) // 0 = delete, 1 = move
        {
            List<Model_DuplicateChecker> errors = new List<Model_DuplicateChecker>();
            List<Model_DuplicateChecker> done = new List<Model_DuplicateChecker>();
            this.DupChecker.ProgressMax = (from a in files where a.Selected select a).ToList().Count;
            this.DupChecker.ProgressIndex = 0;
            
            foreach (Model_DuplicateChecker dc in files)
            {
                if (this.DupChecker.Cancel)
                {
                    ProgressWorker.I.EnQ("Operation Cancelled");
                    break;
                }
                if (dc.Selected)
                {
                    FileInfo fi = new FileInfo(dc.FileDetails.Fullfilepath);

                    if (bgWorker != null)
                    {
                        string text = string.Format("{0} {1} {2}", operation == 0 ? "Delete" : "Move", Win32API.FormatByteSize(fi.Length), dc.FileDetails.Fullfilepath);
                        // then report to the gui
                        bgWorker.ReportProgress(-1, text);
                    }

                    Worker.I.TotalSpecialOperations++;
                    
                    if (fi.Exists)
                    {
                        try
                        {
                            #region process
                            if (operation == 0)
                            {
                                if (Properties.Settings.Default.ShredFiles)
                                {
                                    ProgressWorker.I.EnQ("Shredding file: " + fi.FullName + "|1");
                                    FileOperations.WipeFile(fi.FullName, 5);
                                }
                                else
                                {
                                    ProgressWorker.I.EnQ("Deleting file: " + fi.FullName + "|1");
                                    fi.Delete();
                                }

                                Worker.I.TotalFileDelete++;
                                Worker.I.TotalFileSize += fi.Length;
                            }
                            else if (operation == 1)
                            {
                                ProgressWorker.I.EnQ("Moving file: " + fi.FullName + "|1");
                                string moveDir = Properties.Settings.Default.DupChecker_DuplicateFolderPath;
                                moveDir = Path.Combine(moveDir, fi.Name);
                                FileInfo fi_moveDir = new FileInfo(moveDir);

                                if (fi_moveDir.Exists)
                                {
                                    fi.Delete();
                                }

                                fi.MoveTo(moveDir);
                            }

                            done.Add(dc);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            //this.UpdateProgressLog("Erorr while {0} the file \"{0}\"", "Error", false);
                            Debug.WriteLine(ex.Message);
                            errors.Add(dc);

                        }
                    }

                    this.DupChecker.ProgressIndex++;
                }
            }
        }
        #endregion
    }
}
