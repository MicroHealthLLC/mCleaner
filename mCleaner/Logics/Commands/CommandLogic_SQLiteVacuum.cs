using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_SQLiteVacuum : iActions
    {
        bool _apply = false;

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

        public CommandLogic_SQLiteVacuum()
        {

        }
        static CommandLogic_SQLiteVacuum _i = new CommandLogic_SQLiteVacuum();
        public static CommandLogic_SQLiteVacuum I { get { return _i; } }

        public void Execute(bool apply = false)
        {
            this._apply = apply;

            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.file:
                    EnqueueFile(Action.path);
                    break;
                case SEARCH.glob:
                    EnqueueFiles(Action.regex);
                    break;
            }
        }

        public void EnqueueFile(string file)
        {
            FileInfo fi = new FileInfo(file);
            if (fi.Exists)
            {
                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = file,
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.file,
                    command = COMMANDS.sqlite_vacuum,
                    search = SEARCH.file,
                });
            }
        }

        public void EnqueueFiles(string regex = "")
        {
            IEnumerable<string> path_and_file = GlobDir.Glob.GetMatches(Action.path.Replace('\\', '/'), GlobDir.Glob.Constants.IgnoreCase);
            foreach (string file in path_and_file)
            {
                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = file.Replace('/', '\\'),
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.file,
                    command = COMMANDS.sqlite_vacuum,
                    search = SEARCH.file,
                });
            }
        }
    }
}
