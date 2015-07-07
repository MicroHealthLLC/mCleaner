using mCleaner.Helpers;
using mCleaner.Logics.Clam;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public static string testcleaner = mCleaner.Properties.Resources.testcleaner;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!Permissions.IsUserAdministrator)
            {
                MessageBox.Show("You must be an administrator to run this program", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Process.GetCurrentProcess().Kill();
                return;
            }

            // Enable needed privileges
            Permissions.SetPrivileges(true);

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string s_version = version.ToString();

            if (s_version != mCleaner.Properties.Settings.Default.Version)
            {
                mCleaner.Properties.Settings.Default.Upgrade();
                mCleaner.Properties.Settings.Default.Version = s_version;
                mCleaner.Properties.Settings.Default.Save();
            }

            // check for clamwin installation and
            // decide which database to use
            CommandLogic_Clam.I.CheckClamWinInstallation();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Disable needed privileges
            Permissions.SetPrivileges(false);
        }
    }
}
