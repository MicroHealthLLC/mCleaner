using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ApplicationPaths : ScannerBase
    {
        public ApplicationPaths() { }
        static ApplicationPaths _i = new ApplicationPaths();
        public static ApplicationPaths I { get { return _i; } }

        public void Preview()
        {
            this.BadKeys.Clear();

            ScanInstallFolders();
            ScanAppPaths();
        }

        void ScanInstallFolders()
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders");

            if (regKey == null)
                return;

            foreach (string strFolder in regKey.GetValueNames())
            {
                //ScanDlg.CurrentScannedObject = strFolder;

                //if (!Utils.DirExists(strFolder))
                //    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, strFolder);
                if (!Directory.Exists(strFolder))
                {
                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Key = regKey.ToString(),
                        Name = strFolder
                    });
                }
            }
        }

        void ScanAppPaths()
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");

            if (regKey == null)
                return;

            foreach (string strSubKey in regKey.GetSubKeyNames())
            {
                RegistryKey regKey2 = regKey.OpenSubKey(strSubKey);

                if (regKey2 != null)
                {
                    if (Convert.ToInt32(regKey2.GetValue("BlockOnTSNonInstallMode")) == 1)
                        continue;

                    if (string.IsNullOrEmpty(Convert.ToString(regKey2.GetValue("CmstpExtensionDll"))))
                        continue;

                    string strAppPath = regKey2.GetValue("") as string;
                    string strAppDir = regKey2.GetValue("Path") as string;

                    if (string.IsNullOrEmpty(strAppPath))
                    {
                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Key = regKey2.ToString()
                        });
                        continue;
                    }

                    if (!File.Exists(strAppPath))
                    {
                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Key = regKey2.ToString(), Name = strAppPath
                        });
                        continue;
                    }
                }
            }
        }
    }
}
