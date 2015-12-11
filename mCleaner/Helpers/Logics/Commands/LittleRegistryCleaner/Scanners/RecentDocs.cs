using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class RecentDocs : ScannerBase
    {
        public RecentDocs() { }
        static RecentDocs _i = new RecentDocs();
        public static RecentDocs I { get { return _i; } }

        public override void Clean()
        {
            try
            {
                Preview();

                foreach (InvalidKeys k in this.BadKeys)
                {
                    try
                    {
                        //using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                        RegistryKey key = k.Root.OpenSubKey(k.Subkey, true);
                        {

                            if (k.Key == string.Empty)
                            {
                                // ?
                            }
                            else
                            {
                                key = key.OpenSubKey(k.Key, true);
                            }

                            if (k.Name != string.Empty)
                            {
                                BackUpRegistrykey(k);
                                key.DeleteValue(k.Name);
                            }
                            else
                            {
                                key.DeleteSubKey(k.Key);
                            }
                        }
                        key.Close();
                        key.Dispose();
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

            ScanMUICache();
            ScanExplorerDocs();
        }

        /// <summary>
        /// Checks MUI Cache for invalid file references (XP Only)
        /// </summary>
        private void ScanMUICache()
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\ShellNoRoam\MUICache"))
                {
                    if (regKey == null)
                        return;

                    foreach (string valueName in regKey.GetValueNames())
                    {
                        if (valueName.StartsWith("@") || valueName == "LangID")
                            continue;

                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                        if (!File.Exists(valueName))
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, valueName);

                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.CurrentUser,
                                Subkey = "Software\\Microsoft\\Windows\\ShellNoRoam\\MUICache",
                                Key = string.Empty,
                                Name = valueName
                            });
                        }
                    }
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Recurses through the recent documents registry keys for invalid files
        /// </summary>
        private void ScanExplorerDocs()
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RecentDocs"))
                {
                    if (regKey == null)
                        return;

                    EnumMRUList(regKey);

                    foreach (string strSubKey in regKey.GetSubKeyNames())
                    {
                        RegistryKey subKey = regKey.OpenSubKey(strSubKey);

                        if (subKey == null)
                            continue;

                        EnumMRUList(subKey, strSubKey);
                    }
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void EnumMRUList(RegistryKey regKey, string subkey = "")
        {
            foreach (string strValueName in regKey.GetValueNames())
            {
                ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                string filePath, fileArgs;

                // Ignore MRUListEx and others
                if (!Regex.IsMatch(strValueName, "[0-9]"))
                    continue;

                string fileName = ExtractUnicodeStringFromBinary(regKey.GetValue(strValueName));
                string shortcutPath = string.Format("{0}\\{1}.lnk", Environment.GetFolderPath(Environment.SpecialFolder.Recent), fileName);

                // See if file exists in Recent Docs folder
                if (!string.IsNullOrEmpty(fileName))
                {
                    //ScanDlg.StoreInvalidKey(Strings.InvalidRegKey, regKey.ToString(), strValueName);
                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = Registry.CurrentUser,
                        Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RecentDocs",
                        Key = subkey != string.Empty ? subkey : string.Empty,
                        Name = strValueName
                    });
                    continue;
                }

                if (!File.Exists(shortcutPath) || !FileOperations.I.ResolveShortcut(shortcutPath, out filePath, out fileArgs))
                {
                    //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), strValueName);
                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = Registry.CurrentUser,
                        Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RecentDocs",
                        Key = subkey != string.Empty ? subkey : string.Empty,
                        Name = strValueName
                    });
                    continue;
                }
            }
        }

        /// <summary>
        /// Converts registry value to filename
        /// </summary>
        /// <param name="keyObj">Value from registry key</param>
        private string ExtractUnicodeStringFromBinary(object keyObj)
        {
            string Value = keyObj.ToString();    //get object value 
            string strType = keyObj.GetType().Name;  //get object type

            if (strType == "Byte[]")
            {
                Value = "";
                byte[] Bytes = (byte[])keyObj;
                //this seems crude but cannot find a way to 'cast' a Unicode string to byte[]
                //even in case where we know the beginning format is Unicode
                //so do it the hard way

                char[] chars = Encoding.Unicode.GetChars(Bytes);
                foreach (char bt in chars)
                {
                    if (bt != 0)
                    {
                        Value = Value + bt; //construct string one at a time
                    }
                    else
                    {
                        break;  //apparently found 0,0 (end of string)
                    }
                }
            }
            return Value;
        }
    }
}
