using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using CodeBureau;
using GlobDir;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Delete : CommandLogic_Base, iActions
    {
        bool _apply = false;

        private cleaner _CleanerML = new cleaner();
        public cleaner CleanerML
        {
            get { return _CleanerML; }
            set
            {
                if (_CleanerML != value)
                {
                    _CleanerML = value;
                }
            }
        }

        public CommandLogic_Delete()
        {

        }
        private static CommandLogic_Delete _i = new CommandLogic_Delete();
        public static CommandLogic_Delete I { get { return _i; } }

        public void Enqueue(bool apply = false)
        {
            this._apply = apply;
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.deep:
                    break;
                case SEARCH.file:
                    Search_File();
                    break;
                case SEARCH.glob:
                    Search_Glob(Action.regex);
                    break;
                case SEARCH.walk_all:
                    Search_Walk(true, Action.regex);
                    break;
                case SEARCH.walk_files:
                    Search_Walk(regex: Action.regex);
                    break;
            }
        }

        public void ExecuteDeleteCommand(Model_ThingsToDelete ttd, BackgroundWorker bgWorker, Queue<Model_ThingsToDelete> TTD, bool preview = false)
        {
            if (preview)
            {
                FileInfo fi = new FileInfo(ttd.FullPathName);
                if (fi.Exists)
                {
                    string text = string.Format("{0} {1} {2}", "Delete", Win32API.FormatByteSize(fi.Length), ttd.FullPathName);

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

                        //text = string.Format(" - DELETED");

                        //// then report to the gui
                        //bgWorker.ReportProgress(-1, text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR while deleting a file: " + ex.Message);
                    }
                }

                // delete directories as well if search parameter is walk.all
                if (ttd.search == SEARCH.walk_all)
                {
                    if (TTD.Count == 0)
                    {
                        DirectoryInfo di = new DirectoryInfo(ttd.path);
                        if (di.Exists)
                        {
                            FileOperations.I.DeleteEmptyDirectories(ttd.path, (a) =>
                            {
                                //string text = string.Format("Delete 0 {0} - DELETED", a);
                                string text = string.Format("Delete 0 {0}", a);
                                // then report to the gui
                                bgWorker.ReportProgress(-1, text);
                            });
                        }
                    }
                }
            }
        }

        public void Search_File()
        {
            string path = Action.path;

            if (Glob.HasGlobPattern(path))
            {
                IEnumerable<string> files = Glob.GetMatches(path, Glob.Constants.PathName);
                string[] _files = new List<string>(files).ToArray();
                path = _files.Length > 0 ? _files[0] : null;
            }

            if (path != null)
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    if (!IsWhitelisted(fi.FullName))
                    {
                        ProgressWorker.I.EnQ("Queueing file: " + fi.FullName);

                        // enqueue file for deletion
                        Worker.I.EnqueTTD(new Model_ThingsToDelete()
                        {
                            FullPathName = Action.path,
                            IsWhitelisted = false,
                            OverWrite = false,
                            WhatKind = THINGS_TO_DELETE.file,
                            command = COMMANDS.delete,
                            search = SEARCH.file,
                            path = string.Empty,
                            level = Action.parent_option.level,
                            cleaner_name = Action.parent_option.label
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="include_dir"></param>
        /// <param name="regex"></param>
        public void Search_Walk(bool include_dir = false, string regex = null)
        {
            string path = Action.path;
            List<string> list_paths = new List<string>();

            // check glob path
            {
                string rev_slash = path.Replace('\\', '/');
                if (Glob.HasGlobPattern(rev_slash))
                {
                    IEnumerable<string> paths = Glob.GetMatches(rev_slash, Glob.Constants.IgnoreCase);

                    // since we have glob in our path
                    // let's take that off
                    list_paths.Clear();

                    // add insert a new ones
                    list_paths.AddRange(paths);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    if (di.Exists) { list_paths.Add(path); }
                }
            }

            foreach (string currPath in list_paths)
            {
                // confirm if such directory exists
                DirectoryInfo di = new DirectoryInfo(currPath.Replace('/', '\\'));
                if (di.Exists)
                {
                    // get the following files.

                    List<string> files = FileOperations.I.GetFilesRecursive(currPath, regex, (s) =>
                    {
                        ProgressWorker.I.EnQ("Scanning directory " + s);
                        
                    });

               
                    files.Reverse();

                    foreach (string file in files)
                    {
                        if (!IsWhitelisted(file))
                        {
                            // enqueue file for deletion
                            Worker.I.EnqueTTD(new Model_ThingsToDelete()
                            {
                                FullPathName = file,
                                IsWhitelisted = false,
                                OverWrite = false,
                                WhatKind = THINGS_TO_DELETE.file,
                                command = COMMANDS.delete,
                                search = include_dir ? SEARCH.walk_all : SEARCH.walk_files,
                                path = di.FullName,
                                level = Action.parent_option.level,
                                cleaner_name = Action.parent_option.label
                            });
                        }
                    }
                }
            }
        }

        public void Search_Glob(string regex = null)
        {
            List<string> list_paths = new List<string>();

            IEnumerable<string> path_and_file = Glob.GetMatches(Action.path.Replace('\\', '/'), Glob.Constants.IgnoreCase);

            // since we have glob in our path
            // let's take that off
            list_paths.Clear();

            // add insert a new ones
            list_paths.AddRange(path_and_file);

            foreach (string currPath in list_paths)
            {
                // confirm if such directory exists
                DirectoryInfo di = new DirectoryInfo(currPath.Replace('/', '\\'));
                if (di.Exists)
                {
                    // get the following files.
                    List<string> files = FileOperations.I.GetFilesRecursive(currPath, regex, (s) =>
                    {
                            ProgressWorker.I.EnQ("Scanning directory " + s);
                    });
                    files.Reverse();

                    foreach (string file in files)
                    {
                        if (!IsWhitelisted(file))
                        {
                            // enqueue file for deletion
                            Worker.I.EnqueTTD(new Model_ThingsToDelete()
                            {
                                FullPathName = file,
                                IsWhitelisted = false,
                                OverWrite = false,
                                WhatKind = THINGS_TO_DELETE.file,
                                command = COMMANDS.delete,
                                search = SEARCH.glob,
                                path = di.FullName,
                                level = Action.parent_option.level,
                                cleaner_name = Action.parent_option.label
                            });
                        }
                    }
                }
                else
                {
                    // then it must be a file
                    FileInfo fi = new FileInfo(currPath);
                    if (fi.Exists)
                    {
                        if (!IsWhitelisted(fi.FullName))
                        {
                            ProgressWorker.I.EnQ("Queueing file: " + fi.FullName);

                            // enqueue file for deletion
                            Worker.I.EnqueTTD(new Model_ThingsToDelete()
                            {
                                FullPathName = fi.FullName,
                                IsWhitelisted = false,
                                OverWrite = false,
                                WhatKind = THINGS_TO_DELETE.file,
                                command = COMMANDS.delete,
                                search = SEARCH.glob,
                                path = fi.Directory.FullName,
                                level = Action.parent_option.level,
                                cleaner_name = Action.parent_option.label
                            });
                        }
                    }
                }
            }
        }

        public bool IsWhitelisted(string path)
        {
            bool ret = false;
            StringCollection lists = Settings.Default.WhitelistCollection;

            if (lists == null) return false;

            foreach (string f in lists)
            {
                ret = false;
                if (File.Exists(f))
                {
                    if (path.ToLower() == f.ToLower()) { ret = true; break; }
                }
                else if(Directory.Exists(f))
                {
                    // eg
                    // whitelisted directory: C:\Users\Jayson\AppData\Local\Temp\ALM
                    // path to be deleted   : C:\Users\Jayson\AppData\Local\Temp\ALM\ShadowCopies\d926192406574f99b57077a37efb88b7
                    string p = path.Substring(0, f.Length);
                    // p will be            : C:\Users\Jayson\AppData\Local\Temp\ALM
                    if (p == f) { ret = true; break; }
                }
            }

            return ret;
        }
    }
}
