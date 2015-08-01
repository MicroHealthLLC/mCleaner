using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Clipboard : CommandLogic_Base, iActions
    {
        bool _apply = false;

        public CommandLogic_Clipboard() { }
        static CommandLogic_Clipboard _i = new CommandLogic_Clipboard();
        public static CommandLogic_Clipboard I { get { return _i; } }

        public void Enqueue(bool apply = false)
        {
            this._apply = apply;

            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.clipboard_clear:
                    // enqueue file for deletion
                    Worker.I.EnqueTTD(new Model_ThingsToDelete()
                    {
                        WhatKind = THINGS_TO_DELETE.system,
                        command = COMMANDS.clipboard,
                        search = SEARCH.clipboard_clear,
                        level = Action.parent_option.level,
                        cleaner_name = Action.parent_option.label
                    });

                    break;
            }
        }

        public async Task<bool> ExecuteCommand(bool preview = false)
        {
            bool ret = false;
            string text = "Clean clipboard";

            if (preview)
            {
                UpdateProgressLog(text, text);
                ret = true;
            }
            else
            {
                Thread t = new Thread(new ThreadStart(() => { Clipboard.Clear(); }));
                t.SetApartmentState(ApartmentState.STA); 
                t.Start();
                ret = true;
            }

            return ret;
        }
    }
}
