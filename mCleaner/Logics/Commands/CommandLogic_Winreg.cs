using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Winreg : iActions
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

        public CommandLogic_Winreg()
        {

        }
        private static CommandLogic_Winreg _i = new CommandLogic_Winreg();
        public static CommandLogic_Winreg I { get { return _i; } }

        public void Execute(bool apply = false)
        {
            this._apply = apply;

            if (Action.search != null)
            {
                SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

                switch (search)
                {
                    case SEARCH.winreg_delete_entries:
                        DeleteEntry(true, Action.regex);
                        break;
                }
            }
            else
            {
                DeleteEntry();
            }
        }

        void DeleteEntry(bool delete_entries = false, string regex = null)
        {
            string reg_path = Action.path;
            string reg_root = reg_path.Substring(0, reg_path.IndexOf('\\'));
            string reg_subkey = reg_path.Substring(reg_path.IndexOf('\\') + 1, reg_path.Length - reg_root.Length - 1);
            string reg_entry = Action.name;

            if (delete_entries)
            {
                Dictionary<string, string> dic = RegistryHelper.I.GetNameValues(reg_root, reg_subkey);

                foreach (string key in dic.Keys)
                {
                    string val = dic[key];

                    if (RegistryHelper.I.IsNameExists(reg_root, reg_subkey, key))
                    {
                        Worker.I.EnqueTTD(new Model_ThingsToDelete()
                        {
                            reg_root = reg_root,
                            reg_name = key,
                            reg_subkey = reg_subkey,
                            reg_name_val = val,

                            FullPathName = reg_root + "\\" + reg_subkey + "\\" + key,

                            IsWhitelisted = false,
                            OverWrite = false,
                            WhatKind = THINGS_TO_DELETE.registry_name,
                            command = COMMANDS.winreg,
                            search = SEARCH.winreg_delete_entries,
                        });
                    }
                }
            }
            else
            {
                //Debug.WriteLine("Deleting " + reg_path);
                //RegistryHelper.I.DeleteEntries(reg_root, reg_subkey);

                if (RegistryHelper.I.IsSubkeyExists(reg_root, reg_subkey))
                {
                    // enqueue registry for deletion
                    Worker.I.EnqueTTD(new Model_ThingsToDelete()
                    {
                        reg_root = reg_root,
                        reg_name = reg_entry,
                        reg_subkey = reg_subkey,

                        FullPathName = reg_entry == null ?
                                    reg_root + "\\" + reg_subkey :
                                    reg_root + "\\" + reg_subkey + "\\" + reg_entry,

                        IsWhitelisted = false,
                        OverWrite = false,
                        WhatKind = THINGS_TO_DELETE.registry_key,
                        command = COMMANDS.winreg,
                        search = SEARCH.none,
                    });
                }
            }
        }
    }
}
