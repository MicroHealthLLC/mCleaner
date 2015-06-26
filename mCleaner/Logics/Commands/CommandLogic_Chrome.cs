using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Chrome : iActions
    {
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

        public void Execute(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.file:
                    EnqueueFiles();
                    break;
                case SEARCH.glob:
                    break;
            }
        }


        void EnqueueFiles()
        {
            if (File.Exists(Action.path))
            {
                COMMANDS command = (COMMANDS)StringEnum.Parse(typeof(COMMANDS), Action.command);

                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = Action.path,
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.clamwin,
                    command = command,
                    search = SEARCH.file
                });
            }
        }
    }
}
