
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ActiveXComObjects : ScannerBase
    {
        public ActiveXComObjects() { }
        static ActiveXComObjects _i = new ActiveXComObjects();
        public static ActiveXComObjects I { get { return _i; } }

        //public async Task<bool> Clean(bool preview)
        //{
        //    if (preview)
        //    {
        //        await PreviewAsync();
        //    }
        //    else
        //    {
        //        Clean();
        //    }

        //    return true;
        //}

        //public async Task PreviewAsync()
        //{
        //    await Task.Run(() => Preview());
        //}

        public override void Clean()
        {
            Preview();

            foreach (InvalidKeys badkey in this.BadKeys)
            {
                using (RegistryKey root = badkey.Root.OpenSubKey(badkey.Subkey, true))
                {
                    if (root != null)
                    {
                        root.DeleteSubKey(badkey.Key);
                    }
                }
            }
        }

        public override void Preview()
        {
            this.BadKeys.Clear();

            try
            {
                // Scan all CLSID sub keys
                ScanCLSIDSubKey(Registry.ClassesRoot, "CLSID");
                ScanCLSIDSubKey(Registry.LocalMachine, "SOFTWARE\\Classes\\CLSID");
                ScanCLSIDSubKey(Registry.CurrentUser, "SOFTWARE\\Classes\\CLSID");
                if (Utils.Is64BitOS)
                {
                    ScanCLSIDSubKey(Registry.ClassesRoot, "Wow6432Node\\CLSID");
                    ScanCLSIDSubKey(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Classes\\CLSID");
                    ScanCLSIDSubKey(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Classes\\CLSID");
                }

                // Scan file extensions + progids
                ScanClasses(Registry.ClassesRoot, "");
                ScanClasses(Registry.LocalMachine, "SOFTWARE\\Classes");
                ScanClasses(Registry.CurrentUser, "SOFTWARE\\Classes");
                if (Utils.Is64BitOS)
                {
                    ScanClasses(Registry.ClassesRoot, ("Wow6432Node"));
                    ScanClasses(Registry.LocalMachine, ("SOFTWARE\\Wow6432Node\\Classes"));
                    ScanClasses(Registry.CurrentUser, ("SOFTWARE\\Wow6432Node\\Classes"));
                }

                // Scan appids
                ScanAppIds(Registry.ClassesRoot, ("AppID"));
                ScanAppIds(Registry.LocalMachine, ("SOFTWARE\\Classes\\AppID"));
                ScanAppIds(Registry.CurrentUser, ("SOFTWARE\\Classes\\AppID"));
                if (Utils.Is64BitOS)
                {
                    ScanAppIds(Registry.ClassesRoot, ("Wow6432Node\\AppID"));
                    ScanAppIds(Registry.LocalMachine, ("SOFTWARE\\Wow6432Node\\AppID"));
                    ScanAppIds(Registry.CurrentUser, ("SOFTWARE\\Wow6432Node\\AppID"));
                }

                // Scan explorer subkey
                ScanExplorer();
            }
            catch (Exception ex)
            {

            }
        }

        private void ScanCLSIDSubKey(RegistryKey root, string subkey)
        {
            RegistryKey regKey = root.OpenSubKey(subkey);

            if (regKey == null)
                return;

            foreach (string strCLSID in regKey.GetSubKeyNames())
            {
                RegistryKey rkCLSID = regKey.OpenSubKey(strCLSID);

                if (rkCLSID == null)
                    continue;

                ProgressWorker.I.EnQ("Scanning " + rkCLSID.ToString());

                // Check for valid AppID
                string strAppID = regKey.GetValue("AppID") as string;
                if (!string.IsNullOrEmpty(strAppID))
                {
                    if (!appidExists(strAppID))
                    {
                        //ScanDlg.StoreInvalidKey(Strings.MissingAppID, rkCLSID.ToString(), "AppID");

                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = root,
                            Subkey = subkey,
                            Key = strCLSID,
                            Name = string.Empty
                        });
                    }
                }

                // See if DefaultIcon exists
                using (RegistryKey regKeyDefaultIcon = rkCLSID.OpenSubKey("DefaultIcon"))
                {
                    if (regKeyDefaultIcon != null)
                    {
                        string iconPath = regKeyDefaultIcon.GetValue("") as string;

                        if (!string.IsNullOrEmpty(iconPath))
                        {
                            if (!FileOperations.IconExists(iconPath))
                            {
                                //if (!ScanDlg.IsOnIgnoreList(iconPath))
                                {
                                    //ScanDlg.StoreInvalidKey(Strings.InvalidFile, 
                                    //                        string.Format("{0}\\DefaultIcon", 
                                    //                                      rkCLSID.ToString()));

                                    this.BadKeys.Add(new InvalidKeys()
                                    {
                                        Root = root,
                                        Subkey = rkCLSID.ToString().Substring(rkCLSID.ToString().IndexOf('\\') + 1),
                                        Key = "DefaultIcon",
                                        Name = iconPath
                                    });
                                }
                            }
                        }
                    }
                }

                // Look for InprocServer files
                using (RegistryKey regKeyInprocSrvr = rkCLSID.OpenSubKey("InprocServer"))
                {
                    if (regKeyInprocSrvr != null)
                    {
                        string strInprocServer = regKeyInprocSrvr.GetValue("") as string;

                        if (!string.IsNullOrEmpty(strInprocServer))
                        {
                            if (!FileOperations.FileExists(strInprocServer))
                            {
                                //ScanDlg.StoreInvalidKey(Strings.InvalidInprocServer, regKeyInprocSrvr.ToString());

                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = root,
                                    Subkey = rkCLSID.ToString().Substring(rkCLSID.ToString().IndexOf('\\') + 1),
                                    Key = "InprocServer",
                                    Name = string.Empty
                                });
                            }
                        }

                        regKeyInprocSrvr.Close();
                    }
                }

                using (RegistryKey regKeyInprocSrvr32 = rkCLSID.OpenSubKey("InprocServer32"))
                {
                    if (regKeyInprocSrvr32 != null)
                    {
                        string strInprocServer32 = regKeyInprocSrvr32.GetValue("") as string;

                        if (!string.IsNullOrEmpty(strInprocServer32))
                        {
                            if (!FileOperations.FileExists(strInprocServer32))
                            {
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = root,
                                    Subkey = rkCLSID.ToString().Substring(rkCLSID.ToString().IndexOf('\\') + 1),
                                    Key = "InprocServer32",
                                    Name = strInprocServer32
                                });
                            }
                        }

                        regKeyInprocSrvr32.Close();
                    }
                }

                rkCLSID.Close();
            }

            regKey.Close();
            return;
        }

        /// <summary>
        /// Finds invalid File extensions + ProgIDs referenced
        /// </summary>
        private void ScanClasses(RegistryKey root, string subkey)
        {
            RegistryKey regKey = root;

            if(subkey!=string.Empty) regKey = root.OpenSubKey(subkey);

            if (regKey == null)
                return;

            //Main.Logger.WriteLine("Scanning " + regKey.Name + " for invalid Classes");

            foreach (string strSubKey in regKey.GetSubKeyNames())
            {
                // Skip any file (*)
                if (strSubKey == "*")
                    continue;

                ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.Name, strSubKey));

                if (strSubKey[0] == '.')
                {
                    // File Extension
                    using (RegistryKey rkFileExt = regKey.OpenSubKey(strSubKey))
                    {
                        if (rkFileExt != null)
                        {
                            // Find reference to ProgID
                            string strProgID = rkFileExt.GetValue("") as string;

                            if (!string.IsNullOrEmpty(strProgID))
                            {
                                if (!progIDExists(strProgID))
                                {
                                    //ScanDlg.StoreInvalidKey(Strings.MissingProgID, rkFileExt.ToString());

                                    string key = rkFileExt.ToString();
                                    key = key.Substring(key.IndexOf('\\') + 1);

                                    this.BadKeys.Add(new InvalidKeys()
                                    {
                                        Root = root,
                                        Subkey = key,
                                        Key = string.Empty,
                                        Name = string.Empty
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    // ProgID or file class

                    // See if DefaultIcon exists
                    using (RegistryKey regKeyDefaultIcon = regKey.OpenSubKey(string.Format("{0}\\DefaultIcon", strSubKey)))
                    {
                        if (regKeyDefaultIcon != null)
                        {
                            string iconPath = regKeyDefaultIcon.GetValue("") as string;

                            if (!string.IsNullOrEmpty(iconPath))
                            {
                                if (!FileOperations.IconExists(iconPath))
                                {
                                    //if (!ScanDlg.IsOnIgnoreList(iconPath))
                                    {
                                        //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKeyDefaultIcon.Name);

                                        string key = regKeyDefaultIcon.ToString();
                                        key = key.Substring(key.IndexOf('\\') + 1);

                                        this.BadKeys.Add(new InvalidKeys()
                                        {
                                            Root = root,
                                            Subkey = key,
                                            Key = "",
                                            Name = string.Empty
                                        });
                                    }
                                }
                            }
                        }
                    }

                    // Check referenced CLSID
                    using (RegistryKey rkCLSID = regKey.OpenSubKey(string.Format("{0}\\CLSID", strSubKey)))
                    {
                        if (rkCLSID != null)
                        {
                            string guid = rkCLSID.GetValue("") as string;

                            if (!string.IsNullOrEmpty(guid))
                            {
                                if (!clsidExists(guid))
                                {
                                    //ScanDlg.StoreInvalidKey(Strings.MissingCLSID, string.Format("{0}\\{1}", regKey.Name, strSubKey));

                                    string key = rkCLSID.ToString();
                                    key = key.Substring(key.IndexOf('\\') + 1);

                                    this.BadKeys.Add(new InvalidKeys()
                                    {
                                        Root = root,
                                        Subkey = RegistryHelper.I.GetKeyParent(key),
                                        Key = "",
                                        Name = string.Empty
                                    });
                                }
                            }
                        }
                    }
                }

                // Check for unused progid/extension
                using (RegistryKey rk = regKey.OpenSubKey(strSubKey))
                {
                    if (rk != null)
                    {
                        if (rk.ValueCount <= 0 && rk.SubKeyCount <= 0)
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidProgIDFileExt, rk.Name);
                            string key = rk.ToString();
                            key = key.Substring(key.IndexOf('\\') + 1);

                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = root,
                                Subkey = key,
                                Key = "",
                                Name = string.Empty
                            });
                        }
                    }
                }
            }

            regKey.Close();

            return;
        }

        /// <summary>
        /// Looks for invalid references to AppIDs
        /// </summary>
        private void ScanAppIds(RegistryKey root, string subkey)
        {
            RegistryKey regKey = root.OpenSubKey(subkey);

            if (regKey == null)
                return;

            //Main.Logger.WriteLine("Scanning " + regKey.Name + " for invalid AppID's");

            foreach (string strAppId in regKey.GetSubKeyNames())
            {
                using (RegistryKey rkAppId = regKey.OpenSubKey(strAppId))
                {
                    if (rkAppId != null)
                    {
                        // Update scan dialog
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}", rkAppId.ToString()));

                        // Check for reference to AppID
                        string strCLSID = rkAppId.GetValue("AppID") as string;

                        if (!string.IsNullOrEmpty(strCLSID))
                        {
                            if (!appidExists(strCLSID))
                            {
                                //ScanDlg.StoreInvalidKey(Strings.MissingAppID, rkAppId.ToString());

                                string key = rkAppId.ToString();
                                key = key.Substring(key.IndexOf('\\') + 1);

                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = root,
                                    Subkey = key,
                                    Key = string.Empty,
                                    Name = string.Empty
                                });
                            }
                        }
                    }
                }
            }

            regKey.Close();
        }

        /// <summary>
        /// Finds invalid windows explorer entries
        /// </summary>
        private void ScanExplorer()
        {
            // Check Browser Help Objects
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\explorer\\Browser Helper Objects"))
            {
                //Main.Logger.WriteLine("Checking for invalid browser helper objects");

                if (regKey != null)
                {
                    RegistryKey rkBHO = null;

                    foreach (string strGuid in regKey.GetSubKeyNames())
                    {
                        if ((rkBHO = regKey.OpenSubKey(strGuid)) != null)
                        {
                            ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.Name, strGuid));

                            if (!clsidExists(strGuid))
                            {
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = Registry.LocalMachine,
                                    Subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\explorer\\Browser Helper Objects",
                                    Key = strGuid,
                                    Name = string.Empty
                                });
                            }
                        }
                    }
                }
            }

            // Check IE Toolbars
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar"))
            {
                //Main.Logger.WriteLine("Checking for invalid explorer toolbars");

                if (regKey != null)
                {
                    foreach (string strGuid in regKey.GetValueNames())
                    {
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.Name, strGuid));

                        // Update scan dialog
                        //ScanDlg.CurrentScannedObject = "CLSID: " + strGuid;

                        if (!IEToolbarIsValid(strGuid))
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidToolbar, regKey.ToString(), strGuid);

                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.LocalMachine,
                                Subkey = "SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar",
                                Key = strGuid,
                                Name = string.Empty
                            });
                        }
                    }
                }
            }

            // Check IE Extensions
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Extensions"))
            {
                RegistryKey rkExt = null;

                //Main.Logger.WriteLine("Checking for invalid explorer extensions");

                if (regKey != null)
                {
                    foreach (string strGuid in regKey.GetSubKeyNames())
                    {
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.Name, strGuid));

                        if ((rkExt = regKey.OpenSubKey(strGuid)) != null)
                        {
                            // Update scan dialog
                            //ScanDlg.CurrentScannedObject = rkExt.ToString();

                            ValidateExplorerExt(rkExt);
                        }
                    }
                }
            }

            // Check Explorer File Exts
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts"))
            {
                RegistryKey rkFileExt = null;

                //Main.Logger.WriteLine("Checking for invalid explorer file extensions");

                if (regKey != null)
                {
                    foreach (string strFileExt in regKey.GetSubKeyNames())
                    {
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.Name, strFileExt));

                        if ((rkFileExt = regKey.OpenSubKey(strFileExt)) == null || strFileExt[0] != '.')
                            continue;

                        // Update scan dialog
                        //ScanDlg.CurrentScannedObject = rkFileExt.ToString();

                        ValidateFileExt(rkFileExt);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Checks if the AppID exists
        /// </summary>
        /// <param name="appID">The AppID or GUID</param>
        /// <returns>True if it exists</returns>
        private bool appidExists(string appID)
        {
            List<RegistryKey> listRegKeys = new List<RegistryKey>();

            listRegKeys.Add(Registry.ClassesRoot.OpenSubKey(@"AppID"));
            listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Classes\AppID"));
            listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Classes\AppID"));

            if (Utils.Is64BitOS)
            {
                listRegKeys.Add(Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\AppID"));
                listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Classes\AppID"));
                listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Classes\AppID"));
            }

            try
            {
                foreach (RegistryKey rk in listRegKeys)
                {
                    if (rk == null)
                        continue;

                    using (RegistryKey subKey = rk.OpenSubKey(appID))
                    {
                        // check if in ignore list
                        if (subKey != null)
                        {
                            //if (!ScanDlg.IsOnIgnoreList(subKey.ToString())) return true;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Sees if application exists
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <returns>True if it exists</returns>
        private bool appExists(string appName)
        {
            List<RegistryKey> listRegKeys = new List<RegistryKey>();

            listRegKeys.Add(Registry.ClassesRoot.OpenSubKey("Applications"));
            listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Classes\Applications"));
            listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications"));

            if (Utils.Is64BitOS)
            {
                listRegKeys.Add(Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\Applications"));
                listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Classes\Applications"));
                listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Classes\Applications"));
            }

            try
            {
                foreach (RegistryKey rk in listRegKeys)
                {
                    if (rk == null)
                        continue;

                    using (RegistryKey subKey = rk.OpenSubKey(appName))
                    {
                        // check if in ignore list
                        if (subKey != null)
                        {
                            //if (!ScanDlg.IsOnIgnoreList(subKey.ToString())) return true;

                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if the ProgID exists in Classes subkey
        /// </summary>
        /// <param name="progID">The ProgID</param>
        /// <returns>True if it exists</returns>
        private bool progIDExists(string progID)
        {
            List<RegistryKey> listRegKeys = new List<RegistryKey>();

            listRegKeys.Add(Registry.ClassesRoot);
            listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Classes"));
            listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Classes"));

            if (Utils.Is64BitOS)
            {
                listRegKeys.Add(Registry.ClassesRoot.OpenSubKey(@"Wow6432Node"));
                listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Classes"));
                listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Classes"));
            }

            try
            {
                foreach (RegistryKey rk in listRegKeys)
                {
                    if (rk == null)
                        continue;

                    using (RegistryKey subKey = rk.OpenSubKey(progID))
                    {
                        if (subKey != null)
                        {
                            //if (!ScanDlg.IsOnIgnoreList(subKey.ToString())) return true;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Sees if the specified CLSID exists
        /// </summary>
        /// <param name="clsid">The CLSID GUID</param>
        /// <returns>True if it exists</returns>
        private bool clsidExists(string clsid)
        {
            List<RegistryKey> listRegKeys = new List<RegistryKey>();

            listRegKeys.Add(Registry.ClassesRoot.OpenSubKey("CLSID"));
            listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Classes\CLSID"));
            listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID"));

            if (Utils.Is64BitOS)
            {
                listRegKeys.Add(Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\CLSID"));
                listRegKeys.Add(Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Classes\CLSID"));
                listRegKeys.Add(Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Classes\CLSID"));
            }

            try
            {
                foreach (RegistryKey rk in listRegKeys)
                {
                    if (rk == null)
                        continue;

                    using (RegistryKey subKey = rk.OpenSubKey(clsid))
                    {
                        if (subKey != null)
                        {
                            //if (!ScanDlg.IsOnIgnoreList(subKey.ToString())) return true;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks for inprocserver file
        /// </summary>
        /// <param name="regKey">The registry key contain Inprocserver subkey</param>
        /// <returns>False if Inprocserver is null or doesnt exist</returns>
        private static bool InprocServerExists(RegistryKey regKey)
        {
            if (regKey != null)
            {
                using (RegistryKey regKeyInprocSrvr = regKey.OpenSubKey("InprocServer"))
                {
                    if (regKeyInprocSrvr != null)
                    {
                        string strInprocServer = regKeyInprocSrvr.GetValue("") as string;

                        if (!string.IsNullOrEmpty(strInprocServer))
                            if (FileOperations.FileExists(strInprocServer))
                                return true;
                    }
                }

                using (RegistryKey regKeyInprocSrvr32 = regKey.OpenSubKey("InprocServer32"))
                {
                    if (regKeyInprocSrvr32 != null)
                    {
                        string strInprocServer32 = regKeyInprocSrvr32.GetValue("") as string;

                        if (!string.IsNullOrEmpty(strInprocServer32))
                            if (FileOperations.FileExists(strInprocServer32))
                                return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if IE toolbar GUID is valid
        /// </summary>
        private bool IEToolbarIsValid(string strGuid)
        {
            bool bRet = false;

            if (!clsidExists(strGuid))
                bRet = false;

            if (InprocServerExists(Registry.ClassesRoot.OpenSubKey("CLSID\\" + strGuid)))
                bRet = true;

            if (InprocServerExists(Registry.LocalMachine.OpenSubKey("Software\\Classes\\CLSID\\" + strGuid)))
                bRet = true;

            if (InprocServerExists(Registry.CurrentUser.OpenSubKey("Software\\Classes\\CLSID\\" + strGuid)))
                bRet = true;

            if (Utils.Is64BitOS)
            {
                if (InprocServerExists(Registry.ClassesRoot.OpenSubKey("Wow6432Node\\CLSID\\" + strGuid)))
                    bRet = true;

                if (InprocServerExists(Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Classes\\CLSID\\" + strGuid)))
                    bRet = true;

                if (InprocServerExists(Registry.CurrentUser.OpenSubKey("Software\\Wow6432Node\\Classes\\CLSID\\" + strGuid)))
                    bRet = true;
            }

            return bRet;
        }

        private void ValidateFileExt(RegistryKey regKey)
        {
            bool bProgidExists = false, bAppExists = false;

            // Skip if UserChoice subkey exists
            if (regKey.OpenSubKey("UserChoice") != null)
                return;

            // Parse and verify OpenWithProgId List
            using (RegistryKey rkProgids = regKey.OpenSubKey("OpenWithProgids"))
            {
                if (rkProgids != null)
                {
                    foreach (string strProgid in rkProgids.GetValueNames())
                    {
                        if (progIDExists(strProgid))
                            bProgidExists = true;
                    }
                }
            }

            // Check if files in OpenWithList exist
            using (RegistryKey rkOpenList = regKey.OpenSubKey("OpenWithList"))
            {
                if (rkOpenList != null)
                {
                    foreach (string strValueName in rkOpenList.GetValueNames())
                    {
                        if (strValueName == "MRUList")
                            continue;

                        string strApp = rkOpenList.GetValue(strValueName) as string;

                        if (appExists(strApp))
                            bAppExists = true;
                    }

                }
            }

            if (!bProgidExists && !bAppExists)
            {
                //ScanDlg.StoreInvalidKey(Strings.InvalidFileExt, regKey.ToString());

                string key = regKey.ToString();
                key = key.Substring(key.IndexOf('\\') + 1);

                this.BadKeys.Add(new InvalidKeys()
                {
                    Root = Registry.CurrentUser,
                    Subkey = key,
                    Key = string.Empty,
                    Name = string.Empty
                });
            }

            return;
        }

        private void ValidateExplorerExt(RegistryKey regKey)
        {
            try
            {
                // Sees if icon file exists
                string strHotIcon = regKey.GetValue("HotIcon") as string;
                if (!string.IsNullOrEmpty(strHotIcon))
                {
                    if (!FileOperations.IconExists(strHotIcon))
                    {
                        //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), "HotIcon");

                        string key = regKey.ToString();
                        key = key.Substring(key.IndexOf('\\') + 1);

                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = Registry.LocalMachine,
                            Subkey = key,
                            Key = string.Empty,
                            Name = strHotIcon
                        });
                    }
                }

                string strIcon = regKey.GetValue("Icon") as string;
                if (!string.IsNullOrEmpty(strIcon))
                {
                    if (!FileOperations.IconExists(strIcon))
                    {
                        //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), "Icon");
                        string key = regKey.ToString();
                        key = key.Substring(key.IndexOf('\\') + 1);

                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = Registry.LocalMachine,
                            Subkey = key,
                            Key = string.Empty,
                            Name = "Icon"
                        });
                    }
                }

                // Lookup CLSID extension
                string strClsidExt = regKey.GetValue("ClsidExtension") as string;
                if (!string.IsNullOrEmpty(strClsidExt))
                {
                    //ScanDlg.StoreInvalidKey(Strings.MissingCLSID, regKey.ToString(), "ClsidExtension");
                    string key = regKey.ToString();
                    key = key.Substring(key.IndexOf('\\') + 1);

                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = Registry.LocalMachine,
                        Subkey = key,
                        Key = string.Empty,
                        Name = "ClsidExtension"
                    });
                }

                // See if files exist
                string strExec = regKey.GetValue("Exec") as string;
                if (!string.IsNullOrEmpty(strExec))
                {
                    if (!FileOperations.FileExists(strExec))
                    {
                        //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), "Exec");
                        string key = regKey.ToString();
                        key = key.Substring(key.IndexOf('\\') + 1);

                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = Registry.LocalMachine,
                            Subkey = key,
                            Key = string.Empty,
                            Name = "Exec"
                        });
                    }
                }

                string strScript = regKey.GetValue("Script") as string;
                if (!string.IsNullOrEmpty(strScript))
                {
                    if (!FileOperations.FileExists(strScript))
                    {
                        //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), "Script");
                        string key = regKey.ToString();
                        key = key.Substring(key.IndexOf('\\') + 1);

                        this.BadKeys.Add(new InvalidKeys()
                        {
                            Root = Registry.LocalMachine,
                            Subkey = key,
                            Key = string.Empty,
                            Name = "Script"
                        });
                    }
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
