using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Delete : iActions
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

        public CommandLogic_Delete()
        {

        }
        private static CommandLogic_Delete _i = new CommandLogic_Delete();
        public static CommandLogic_Delete I { get { return _i; } }

        public void Execute(bool apply = false)
        {
            this._apply = apply;
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.deep:
                    break;
                case SEARCH.file:
                    File();
                    break;
                case SEARCH.glob:
                    Glob(Action.regex);
                    break;
                case SEARCH.walk_all:
                    Walk(true, Action.regex);
                    break;
                case SEARCH.walk_files:
                    Walk(regex: Action.regex);
                    break;
            }
        }

        public void File()
        {
            string path = Action.path;

            if (GlobDir.Glob.HasGlobPattern(path))
            {
                IEnumerable<string> files = GlobDir.Glob.GetMatches(path, GlobDir.Glob.Constants.PathName);
                string[] _files = new List<string>(files).ToArray();
                path = _files.Length > 0 ? _files[0] : null;
            }

            if (path != null)
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    // enqueue file for deletion
                    Worker.I.EnqueTTD(new Model_ThingsToDelete()
                    {
                        FullPathName = Action.path,
                        IsWhitelisted = false,
                        OverWrite = false,
                        WhatKind = THINGS_TO_DELETE.file,
                        command = COMMANDS.delete,
                        search = SEARCH.file,
                        path = string.Empty
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="include_dir"></param>
        /// <param name="regex"></param>
        public void Walk(bool include_dir = false, string regex = null)
        {
            string path = Action.path;
            List<string> list_paths = new List<string>();

            // check glob path
            {
                string rev_slash = path.Replace('\\', '/');
                if (GlobDir.Glob.HasGlobPattern(rev_slash))
                {
                    IEnumerable<string> paths = GlobDir.Glob.GetMatches(rev_slash, GlobDir.Glob.Constants.IgnoreCase);

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
                    List<string> files = FileOperations.I.GetFilesRecursive(currPath, regex);
                    files.Reverse();

                    foreach (string file in files)
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
                            path = di.FullName
                        });
                    }
                }
            }
        }

        public void Glob(string regex = null)
        {
            List<string> list_paths = new List<string>();

            IEnumerable<string> path_and_file = GlobDir.Glob.GetMatches(Action.path.Replace('\\', '/'), GlobDir.Glob.Constants.IgnoreCase);

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
                    List<string> files = FileOperations.I.GetFilesRecursive(currPath, regex);
                    files.Reverse();

                    foreach (string file in files)
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
                            path = di.FullName
                        });
                    }
                }
                else
                {
                    // then it must be a file
                    FileInfo fi = new FileInfo(currPath);
                    if (fi.Exists)
                    {
                        // enqueue file for deletion
                        Worker.I.EnqueTTD(new Model_ThingsToDelete()
                        {
                            FullPathName = fi.FullName,
                            IsWhitelisted = false,
                            OverWrite = false,
                            WhatKind = THINGS_TO_DELETE.file,
                            command = COMMANDS.delete,
                            search = SEARCH.glob,
                            path = fi.Directory.FullName
                        });
                    }
                }
            }
        }
    }
}
