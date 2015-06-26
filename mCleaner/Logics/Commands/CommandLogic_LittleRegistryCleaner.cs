using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using System;

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

        public void Execute(bool apply = false)
        {
            //SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            //switch (search)
            //{
            //    case SEARCH.lrc_app_inf:
            //        break;
                    
            //}


        }
    }
}
