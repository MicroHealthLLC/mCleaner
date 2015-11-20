using CodeBureau;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_WindowsUninstaller : CommandLogic_Base, iActions
    {
        #region vars

        #endregion

        #region properties
        public ViewModel_Uninstaller WinUninstaller
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Uninstaller>();
            }
        }
        #endregion

        #region commands

        #endregion
        
        #region ctor
        public CommandLogic_WindowsUninstaller()
        {

        }
        private static CommandLogic_WindowsUninstaller _i = new CommandLogic_WindowsUninstaller();
        public static CommandLogic_WindowsUninstaller I { get { return _i; } }
        #endregion

        #region command methods

        #endregion

        #region methods
        public void Enqueue(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.dupchecker_all:
                    EnqueueCustomPath(Action.path);
                    break;
            }
        }

       
        public void EnqueueCustomPath(string path)
        {
            // enqueue file for deletion
            Worker.I.EnqueTTD(new Model_ThingsToDelete()
            {
                FullPathName = path,
                IsWhitelisted = false,
                OverWrite = false,
                WhatKind = THINGS_TO_DELETE.system,
                command = COMMANDS.dupchecker,
                search = SEARCH.dupchecker_all,
                path = string.Empty,
                level = Action.parent_option.level,
                cleaner_name = Action.parent_option.label
            });
        }

        #endregion
    }
}
