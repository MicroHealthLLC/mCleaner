using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mCleaner.Helpers.Data;
using System.Diagnostics;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_JSON : iActions
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

        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.file:
                    ProcessJSONFile(Action.regex);
                    break;
            }
        }

        void ProcessJSONFile(string regex = null)
        {
            FileInfo fi = new FileInfo(Action.path);
            if (fi.Exists)
            {
                string json = JSON.OpenJSONFiel(Action.path);

                if (json != string.Empty)
                {
                    bool isaddressfound = JSON.isAddressFound(json, Action.address);

                    if (isaddressfound)
                    {
                        Worker.I.EnqueTTD(new Model_ThingsToDelete()
                        {
                            FullPathName = Action.path,

                            path = fi.Directory.FullName,

                            IsWhitelisted = false,
                            OverWrite = false,
                            WhatKind = THINGS_TO_DELETE.file,
                            command = COMMANDS.json,
                            search = SEARCH.file,

                            address = Action.address,
                            level = Action.parent_option.level,
                            cleaner_name = Action.parent_option.label
                        });
                    }
                }
            }
        }
    }
}
