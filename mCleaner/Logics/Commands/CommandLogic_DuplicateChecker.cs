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
            this.DupChecker.Cancel = false;
            int i = 0;
            foreach (string path in Settings.Default.DupChecker_CustomPath)
            {
                Trace.WriteLine("Check Duplicates Started. for Path "+path);
                if (this.DupChecker.Cancel)
                {
                    DupChecker.ProgressText = "Operation Cancelled";
                    return;
                }
                await Task.Run(() => ScanPath(path));
            }
            if (DupChecker.DupplicateCollection.Count > 0)
            {
                this.DupChecker.EnableRemoveDuplicates = true;
                
            }

            this.DupChecker.EnableScanFolder = true;
            this.DupChecker.EnableSelectFolder = true;
        }

        public void ScanPath(string path)
        {
            List<string> files = FileOperations.I.GetFilesRecursive(path, null, new Action<string>((s) =>
            {
                       DupChecker.ProgressText="Retreiving files in: " + s;
            }));

            Dictionary<string, List<string>> files_with_same_size = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            DupChecker.ProgressText="Checking for same file size. This may take a while"; // to minimize work load looking for duplicate files
            this.DupChecker.ProgressMax = files.Count;
            this.DupChecker.ProgressIndex = 0;
            foreach (string file in files)
            {
                if (this.DupChecker.Cancel)
                {
                    DupChecker.ProgressText="Operation Cancelled.";
                    return;
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
                string strKeytoAdd=string.Empty;
                Trace.WriteLine("Inside Scan Path Method");
                if ((Settings.Default.DuplicateFilterFileSizeCriteara && fi.Length > 0) || !Settings.Default.DuplicateFilterFileSizeCriteara) // do not include 0 length files.
                {
                    if (add)
                    {
                        if (!Settings.Default.DuplicateFilterFileSizeCriteara)
                        {
                            strKeytoAdd = fi.Name;
                        }
                        else
                            strKeytoAdd = fi.Length.ToString();
                        Trace.WriteLine("key to Add for path :"+fi.FullName);

                        if (files_with_same_size.ContainsKey(strKeytoAdd))
                        {
                            files_with_same_size[strKeytoAdd].Add(fi.FullName);
                        }
                        else
                        {
                            files_with_same_size.Add(strKeytoAdd, new List<string>() { fi.FullName });
                        }
                    }
                }

                this.DupChecker.ProgressIndex++;
            }

            this.DupChecker.ProgressIndex = 0;

            DupChecker.ProgressText = "Please wait while hashing files. This may take a while";
            // get all the files we need to hash
            List<string> files_to_hash = new List<string>();
            Dictionary<string, List<string>> FileswithSameNameSize = new Dictionary<string, List<string>>();
            foreach (string filesize in files_with_same_size.Keys)
            {
                if (this.DupChecker.Cancel)
                {
                    DupChecker.ProgressText="Operation Cancelled";
                    return;
                }
                if (files_with_same_size[filesize].Count > 1)
                {
                    Trace.WriteLine("File With Same Size Count >2 :" + filesize);
                    files_to_hash.AddRange(files_with_same_size[filesize].ToArray());
                    FileswithSameNameSize.Add(filesize, files_with_same_size[filesize]);
                }

                this.DupChecker.ProgressIndex++;
            }



            List<string> hashed_files = new List<string>();
            this.DupChecker.ProgressMax = files_to_hash.Count;
            this.DupChecker.ProgressIndex = 0;
            Dictionary<string, List<string>> files_with_same_hash = new Dictionary<string, List<string>>();
            if (Settings.Default.DuplicateFilterFileSizeCriteara)
            {
                foreach (string filename in files_to_hash)
                {
                    try
                    {

                        if (this.DupChecker.Cancel)
                        {
                            DupChecker.ProgressText = "Operation Cancelled";
                            return;
                        }
                        string hash = FileOperations.I.HashFile(filename);
                        DupChecker.ProgressText = "Hashing: " + filename + " > " + hash;
                        Trace.WriteLine("Hashing: " + filename + " > " + hash);
                        hashed_files.Add(filename + "|" + hash);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    this.DupChecker.ProgressIndex++;
                }


                DupChecker.ProgressText = "Finalizing ...";
                this.DupChecker.ProgressMax = hashed_files.Count;
                this.DupChecker.ProgressIndex = 0;

                foreach (string hashedfile in hashed_files)
                {
                    if (this.DupChecker.Cancel)
                    {
                        DupChecker.ProgressText = "Operation Cancelled";
                        return;
                    }
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
                    if (this.DupChecker.Cancel)
                    {
                        DupChecker.ProgressText = "Operation Cancelled";
                        return;
                    }
                    if (files_with_same_hash[key].Count == 1) teremove.Add(key);
                }
                foreach (string key in teremove)
                {
                    files_with_same_hash.Remove(key);
                }
            }
            else
            {
                files_with_same_hash = FileswithSameNameSize;
            }

            DupChecker.ProgressText = "Adding to collection for previewing.";
            
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                this.DupChecker.ProgressMax = files_with_same_hash.Count;
                this.DupChecker.ProgressIndex = 0;
                foreach (string entry in files_with_same_hash.Keys)
                {
                    DupChecker.ProgressText = "Adding Files to collection " + entry;
                    if (this.DupChecker.Cancel)
                    {
                        DupChecker.ProgressText = "Operation Cancelled";
                        return;
                    }
                    for (int i = 0; i < files_with_same_hash[entry].ToArray().Length; i++)
                    {
                        if (this.DupChecker.Cancel)
                        {
                            DupChecker.ProgressText = "Operation Cancelled";
                            return;
                        }
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

            DupChecker.ProgressText ="Done.";
            

            // clear some memory
            files.Clear();
            files_with_same_size.Clear();
            files_to_hash.Clear();
            hashed_files.Clear();
            files_with_same_hash.Clear();
            if (DupChecker.DupplicateCollection.Count > 0)
                DupChecker.ProgressText = "Done. Select file(s) to be removed, then click on remove duplicates button.";
            else
                DupChecker.ProgressText = "Done. No duplicates found.";
        }

        public void Start(ObservableCollection<Model_DuplicateChecker> files, int operation = 0, BackgroundWorker bgWorker = null) // 0 = delete, 1 = move
        {
            List<Model_DuplicateChecker> errors = new List<Model_DuplicateChecker>();
            List<Model_DuplicateChecker> done = new List<Model_DuplicateChecker>();
            this.DupChecker.ProgressMax = (from a in files where a.Selected select a).ToList().Count;
            this.DupChecker.ProgressIndex = 0;
            this.DupChecker.Cancel = false;
            
            foreach (Model_DuplicateChecker dc in files)
            {
                if (this.DupChecker.Cancel)
                {
                    DupChecker.ProgressText="Operation Cancelled.";
                    break;
                }
                if (dc.Selected)
                {
                    FileInfo fi = new FileInfo(dc.FileDetails.Fullfilepath);

                    if (bgWorker != null)
                    {
                        string text = string.Format("{0} {1} {2}", operation == 0 ? "Delete" : "Move", Win32API.FormatByteSize(fi.Length), dc.FileDetails.Fullfilepath);
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
                                DupChecker.ProgressText = "Deleting file: " + fi.FullName;
                                FileOperations.Delete(fi.FullName);

                                Worker.I.TotalFileDelete++;
                                Worker.I.TotalFileSize += fi.Length;
                            }
                            else if (operation == 1)
                            {
                                DupChecker.ProgressText = "Moving file: " + fi.FullName + " to " + Settings.Default.DupChecker_DuplicateFolderPath;                                 string moveDir = Settings.Default.DupChecker_DuplicateFolderPath;
                                moveDir = Path.Combine(moveDir, fi.Name);
                                FileInfo fiMoveDir = new FileInfo(moveDir);

                                if (fiMoveDir.Exists)
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
                            Debug.WriteLine(ex.Message);
                            errors.Add(dc);

                        }
                    }

                    this.DupChecker.ProgressIndex++;
                }

                DupChecker.ProgressText = "Done.";
                
            }
        }
        #endregion
    }
}
