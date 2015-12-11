using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ApplicationPaths : ScannerBase
    {
        public ApplicationPaths() { }
        static ApplicationPaths _i = new ApplicationPaths();
        public static ApplicationPaths I { get { return _i; } }

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
                        Root = Registry.LocalMachine,
                        Subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\Folders",
                        Key = string.Empty,
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
                    ProgressWorker.I.EnQ("Scanning " + regKey2.ToString());

                    if (Convert.ToInt32(regKey2.GetValue("BlockOnTSNonInstallMode")) == 1)
                        continue;

                    if (!string.IsNullOrEmpty(Convert.ToString(regKey2.GetValue("CmstpExtensionDll"))))
                        continue;

                    string strAppPath = regKey2.GetValue("") as string;
                    strAppPath = strAppPath != null ? strAppPath.Replace("\"", string.Empty) : null;
                    string strAppDir = regKey2.GetValue("Path") as string;
                    strAppDir = strAppDir != null ? strAppDir.Replace("\"", string.Empty) : null;

                    if (string.IsNullOrEmpty(strAppPath))
                    {
                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = Registry.LocalMachine,
                            Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\",
                            Key = strSubKey,
                            Name = string.Empty
                        });
                        continue;
                    }

                    if (!File.Exists(strAppPath))
                    {
                        bool add = true;
                        string path = strAppPath;

                        // some default entries has filename only and does not include the fullpath, so 
                        // we want to check that as well by combining Path entry and the default entry.
                        if (strAppDir != null)
                        {
                            string path2 = Path.Combine(strAppDir, strAppPath);
                            if (File.Exists(path)) // if that exists, then
                            {
                                add = false; // flag it to not add in BadKeys collection
                            }
                        }

                        if (add)
                        {
                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.LocalMachine,
                                Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\",
                                Key = strSubKey,
                                Name = path
                            });
                        }
                        continue;
                    }
                }
            }
        }
    }
}
