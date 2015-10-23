using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using mCleaner.Helpers;
using mCleaner.Logics.Clam;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //public static string testcleaner = mCleaner.Properties.Resources.testcleaner;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length == 2 && e.Args[0] == "shred")
            {
                mCleaner.Logics.StaticResources.strShredLocation = e.Args[1];
            }
            else
            {
                mCleaner.Logics.StaticResources.strShredLocation = string.Empty;
                // force one instance of this application
                bool force_terminate = false;
                Process curr = Process.GetCurrentProcess();
                Process[] procs = Process.GetProcessesByName(curr.ProcessName);
                foreach (Process p in procs)
                {
                    if ((p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName))
                    {
                        force_terminate = true;
                        curr = p;
                        break;
                    }
                }
                if (force_terminate)
                {
                    MessageBox.Show("mCleaner is already running", "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetForegroundWindow(curr.MainWindowHandle);
                    Process.GetCurrentProcess().Kill();
                }
            }

            // check permission
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
            if (e.Args.Length == 2 && e.Args[0] == "shred")
            {
                //do nothing in this case
            }
            else
             CommandLogic_Clam.I.CheckClamWinInstallation();

            if (mCleaner.Properties.Settings.Default.DupChecker_DuplicateFolderPath == string.Empty)
            {
                mCleaner.Properties.Settings.Default.DupChecker_DuplicateFolderPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Duplicates");
                mCleaner.Properties.Settings.Default.Save();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Disable needed privileges
            Permissions.SetPrivileges(false);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
         
        }
    }
}
