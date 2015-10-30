
using System.Diagnostics;
using System.IO;
using System.Security;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class WindowsHelpFiles : ScannerBase
    {
        public WindowsHelpFiles() { }
        static WindowsHelpFiles _i = new WindowsHelpFiles();
        public static WindowsHelpFiles I { get { return _i; } }

        //public void Clean(bool preview)
        //{
        //    if (preview)
        //    {
        //        Preview(); 
        //    }
        //    else
        //    {
        //        Clean();
        //    }
        //}

        public override void Clean()
        {
            Preview();

            foreach (InvalidKeys k in this.BadKeys)
            {
                using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                {
                    key.DeleteValue(k.Name);
                }
            }
        }

        public override void Preview()
        {
            this.BadKeys.Clear();

            try
            {
                CheckHelpFiles(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\HTML Help");
                CheckHelpFiles(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\Help");
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CheckHelpFiles(RegistryKey root, string subkey)
        {
            RegistryKey regKey = root.OpenSubKey(subkey);

            if (regKey == null)
                return;

            foreach (string strHelpFile in regKey.GetValueNames())
            {
                ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                string strHelpPath = regKey.GetValue(strHelpFile) as string;

                if (!HelpFileExists(strHelpFile, strHelpPath))
                {
                    //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), strHelpFile);

                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = Registry.LocalMachine,
                        Subkey = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts",
                        Key = string.Empty,
                        Name = strHelpFile
                    });
                }
            }

            regKey.Close();
            regKey.Dispose();

            return;
        }

        /// <summary>
        /// Sees if the help file exists
        /// </summary>
        /// <param name="helpFile">Should contain the filename</param>
        /// <param name="helpPath">Should be the path to file</param>
        /// <returns>True if it exists</returns>
        private static bool HelpFileExists(string helpFile, string helpPath)
        {
            if (string.IsNullOrEmpty(helpFile) || string.IsNullOrEmpty(helpPath))
                return true;

            string sout = string.Empty;

            if (FileOperations.SearchPath(helpPath, null, out sout))
                return true;

            if (FileOperations.SearchPath(helpFile, null, out sout))
                return true;

            if (File.Exists(Path.Combine(helpPath, helpFile)))
                return true;

            return false;
        }
    }
}
