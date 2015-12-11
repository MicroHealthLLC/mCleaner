using System;
using System.Diagnostics;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ApplicationSettings : ScannerBase
    {
        public ApplicationSettings() { }
        static ApplicationSettings _i = new ApplicationSettings();
        public static ApplicationSettings I { get { return _i; } }

        public override void Clean()
        {
            try
            {
                Preview();

                foreach (InvalidKeys k in this.BadKeys)
                {
                    try
                    {

                        using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                        {
                            BackUpRegistrykey(k);
                            key.DeleteSubKey(k.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
                

        }

        public override void Preview()
        {
            this.BadKeys.Clear();

            ScanRegistryKey(Registry.LocalMachine, "SOFTWARE");
            ScanRegistryKey(Registry.CurrentUser, "SOFTWARE");

            if (Utils.Is64BitOS)
            {
                ScanRegistryKey(Registry.LocalMachine, @"SOFTWARE\Wow6432Node");
                ScanRegistryKey(Registry.CurrentUser, @"SOFTWARE\Wow6432Node");
            }
        }

        private void ScanRegistryKey(RegistryKey root, string subkey)
        {
            RegistryKey baseRegKey = root.OpenSubKey(subkey);

            if (baseRegKey == null)
                return;

            foreach (string strSubKey in baseRegKey.GetSubKeyNames())
            {
                // Skip needed keys, we dont want to mess the system up
                //if (strSubKey == "Microsoft" ||
                //    strSubKey == "Policies" ||
                //    strSubKey == "Classes" ||
                //    strSubKey == "Printers" ||
                //    strSubKey == "Wow6432Node")
                //    continue;

                ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", baseRegKey.ToString(), strSubKey));

                if (IsEmptyRegistryKey(baseRegKey.OpenSubKey(strSubKey, true)))
                {
                    //ScanDlg.StoreInvalidKey(Strings.NoRegKey, baseRegKey.Name + "\\" + strSubKey);

                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = root,
                        Subkey = subkey,
                        Key = strSubKey,
                        Name = string.Empty
                    });
                }
            }

            baseRegKey.Close();
            return;
        }

        /// <summary>
        /// Recursively goes through the registry keys and finds how many values there are
        /// </summary>
        /// <param name="regKey">The base registry key</param>
        /// <returns>True if the registry key is emtpy</returns>
        private bool IsEmptyRegistryKey(RegistryKey regKey)
        {
            if (regKey == null)
                return false;

            int nValueCount = regKey.ValueCount;
            int nSubKeyCount = regKey.SubKeyCount;

            if (regKey.ValueCount == 0)
                if (regKey.GetValue("") != null)
                    nValueCount = 1;

            return (nValueCount == 0 && nSubKeyCount == 0);
        }
    }
}
