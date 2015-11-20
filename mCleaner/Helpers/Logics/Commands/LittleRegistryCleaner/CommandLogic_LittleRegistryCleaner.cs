using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_LittleRegistryCleaner : iActions
    {
        action _Action = new action();
        public action Action
        {
            get
            {
                return this._Action;
            }
            set
            {
                this._Action = value;
            }
        }

        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            string reg_path = Action.path;
            string reg_root = reg_path.Substring(0, reg_path.IndexOf('\\'));
            string reg_subkey = reg_path.Substring(reg_path.IndexOf('\\') + 1, reg_path.Length - reg_root.Length - 1);
            string reg_entry = Action.name;

            if (RegistryHelper.I.IsSubkeyExists(reg_root, reg_subkey))
            {
                // enqueue action for cleaning
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
                    WhatKind = THINGS_TO_DELETE.littlregistrycleaner,
                    command = COMMANDS.littleregistry,
                    search = search,
                });
            }
        }
    }
}
