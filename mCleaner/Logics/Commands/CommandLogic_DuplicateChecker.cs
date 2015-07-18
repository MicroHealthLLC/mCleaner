using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
        public CommandLogic_DuplicateChecker()
        {

        }
        private static CommandLogic_DuplicateChecker _i = new CommandLogic_DuplicateChecker();
        public static CommandLogic_DuplicateChecker I { get { return _i; } }
        #region ctor

        #endregion

        #region command methods

        #endregion

        #region methods
        public void Execute(bool apply = false)
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
                level = Action.level,
                cleaner_name = Action.parent_option.label
            });
        }

        public void ScanPath(string path)
        {
            List<string> files = FileOperations.I.GetFilesRecursive(path, null, new Action<string>((s) =>
            {
                ProgressWorker.I.EnQ("Retreiving files in: " + s);
            }));

            //Dictionary<string, List<string>> files_with_same_size = new Dictionary<string,List<string>>();
            Dictionary<long, List<string>> files_with_same_size = new Dictionary<long,List<string>>();
            ProgressWorker.I.EnQ("Checking for same file size. This may take a while"); // to minimize work load looking for duplicate files
            base.VMCleanerML.MaxProgress = files.Count;
            base.VMCleanerML.ProgressIndex = 0;
            foreach (string file in files)
            {
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

                base.VMCleanerML.ProgressIndex++;
            }

            base.VMCleanerML.ProgressIndex = 0;

            ProgressWorker.I.EnQ("Please wait while hashing files. This may take a while");
            // get all the files we need to hash
            List<string> files_to_hash = new List<string>();
            foreach (long filesize in files_with_same_size.Keys)
            {
                if (files_with_same_size[filesize].Count > 1)
                {
                    files_to_hash.AddRange(files_with_same_size[filesize].ToArray());
                }

                base.VMCleanerML.ProgressIndex++;
            }

            List<string> hashed_files = new List<string>();
            base.VMCleanerML.MaxProgress = files_to_hash.Count;
            base.VMCleanerML.ProgressIndex = 0;

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
            //                    ProgressWorker.I.EnQ("Hashing: " + files_to_hash[j] + " > " + hash);
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
                        ProgressWorker.I.EnQ("Hashing: " + filename + " > " + hash);
                        hashed_files.Add(filename + "|" + hash);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    base.VMCleanerML.ProgressIndex++;
                }
            }
            

            ProgressWorker.I.EnQ("Finalizing ...");
            Dictionary<string, List<string>> files_with_same_hash = new Dictionary<string, List<string>>();
            base.VMCleanerML.MaxProgress = hashed_files.Count;
            base.VMCleanerML.ProgressIndex = 0;

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
                base.VMCleanerML.ProgressIndex++;
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

            ProgressWorker.I.EnQ("Adding to collection for previewing");
            
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                DupChecker.DupplicateCollection.Clear();
                base.VMCleanerML.MaxProgress = files_with_same_hash.Count;
                base.VMCleanerML.ProgressIndex = 0;
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

            //foreach (Model_DuplicateChecker e in DupChecker.DupplicationCollection)
            //{
            //    foreach (string file in e.DuplicateFiles)
            //    {
            //        FileInfo fi = new FileInfo(file);
            //        UpdateProgressLog(string.Format("Duplicate {2} \"{0}\" in \"{1}\" - {3}", fi.Name, fi.Directory.FullName, Win32API.FormatByteSize(fi.Length), e.Hash), "Retreiving duplicate files from the collection");
            //    }
            //}
        }

        public void Start(ObservableCollection<Model_DuplicateChecker> files, int operation = 0) // 0 = delete, 1 = move
        {
            List<Model_DuplicateChecker> errors = new List<Model_DuplicateChecker>();
            List<Model_DuplicateChecker> done = new List<Model_DuplicateChecker>();

            base.VMCleanerML.MaxProgress = (from a in files where a.Selected select a).ToList().Count;
            base.VMCleanerML.ProgressIndex = 0;
            foreach (Model_DuplicateChecker dc in files)
            {
                if (dc.Selected)
                {
                    FileInfo fi = new FileInfo(dc.FileDetails.Fullfilepath);
                    if (fi.Exists)
                    {
                        try
                        {
                            #region process
                            if (operation == 0)
                            {
                                if (Properties.Settings.Default.ShredFiles)
                                {
                                    ProgressWorker.I.EnQ("Shredding file: " + fi.FullName);
                                    FileOperations.WipeFile(fi.FullName, 5);
                                }
                                else
                                {
                                    ProgressWorker.I.EnQ("Deleting file: " + fi.FullName);
                                    fi.Delete();
                                }
                            }
                            else if (operation == 1)
                            {
                                ProgressWorker.I.EnQ("Moving file: " + fi.FullName);
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

                    base.VMCleanerML.ProgressIndex++;
                }
            }
        }
        #endregion
    }
}
