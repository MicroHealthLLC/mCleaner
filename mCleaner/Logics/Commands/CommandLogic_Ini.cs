using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mCleaner.Logics.Commands
{
    // ref: https://jachman.wordpress.com/2006/09/11/how-to-access-ini-files-in-c-net/
    // ref: http://www.codeproject.com/Articles/1966/An-INI-file-handling-class-using-C
    public class CommandLogic_Ini : iActions
    {
        public CommandLogic_Ini() { }
        static CommandLogic_Ini _i = new CommandLogic_Ini();
        public static CommandLogic_Ini I { get { return _i; } }

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

        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.file:
                    ReadIni();
                    break;
            }
        }

        public bool trueorfalse(Func<bool> callback)
        {
            return callback();
        }

        public void ReadIni()
        {
            string section = Action.section;
            string key = Action.parameter;
            string path = Action.path;

            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                bool add = false;

                if (Win32API.IniHelper.IsSectionExists(path, section))
                {
                    add = key == null ? true : trueorfalse(() =>
                    {
                        bool ret = false;

                        foreach (string k in Win32API.IniHelper.GetKeyNames(section, path))
                        {
                            if (k == key)
                            {
                                ret = true;
                                break;
                            }
                        }

                        return ret;
                    });
                }

                if (add)
                {
                    Worker.I.EnqueTTD(new Model_ThingsToDelete()
                    {
                        FullPathName = path + section + key,

                        path = path, // this should be the parent directory of the file name
                        // but for this case, we will use it to set the filename

                        IsWhitelisted = false,
                        OverWrite = false,
                        WhatKind = THINGS_TO_DELETE.file,
                        command = COMMANDS.ini,
                        search = SEARCH.file,

                        section = section,
                        key = key,
                        level = Action.level,
                        cleaner_name = Action.parent_option.label
                    });
                }
            }
        }
    }
}
