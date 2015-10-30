using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using Microsoft.Win32;

namespace mCleaner.Helpers
{
    public class RegistryHelper
    {
        public RegistryHelper()
        {

        }
        private static RegistryHelper _i = new RegistryHelper();
        public static RegistryHelper I { get { return _i; } }

        public RegistryKey ToRegistryRoot(string path)
        {
            RegistryKey root = Registry.CurrentUser;

            switch (path)
            {
                case "HKCU":
                    root = Registry.CurrentUser;
                    break;
                case "HKLM":
                    root = Registry.LocalMachine;
                    break;
                case "HKCR":
                    root = Registry.ClassesRoot;
                    break;
                case "HKCC":
                    root = Registry.CurrentConfig;
                    break;
                case "HKU":
                    root = Registry.CurrentUser;
                    break;
                default:
                    root = null;
                    break;
            }

            return root;
        }

        public IEnumerable<string> GetEntries(string root, string subkey)
        {
            return ToRegistryRoot(root).OpenSubKey(subkey).GetValueNames();
        }

        public Dictionary<string, string> GetNameValues(string root, string subkey)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            RegistryKey key = ToRegistryRoot(root);
            if (key != null)
            {
                if (key.OpenSubKey(subkey) != null)
                {
                    key = key.OpenSubKey(subkey);
                    foreach (string name in key.GetValueNames())
                    {
                        ret.Add(name, key.GetValue(name).ToString());
                    }
                }
            }

            return ret;
        }

        public void DeleteEntries(string root, string subkey, string name = "")
        {
            RegistryKey key = ToRegistryRoot(root);

            if (key != null)
            {
                if (name == string.Empty)
                {
                    if (key.OpenSubKey(subkey) != null)
                    {
                        key.DeleteSubKeyTree(subkey);
                    }
                }
                else
                {
                    key = key.OpenSubKey(subkey, true);
                    if (key.GetValue(name) != null)
                    {
                        key.DeleteValue(name);
                    }
                }
            }
        }

        public bool IsSubkeyExists(string root, string subkey)
        {
            bool res = false;

            RegistryKey key = ToRegistryRoot(root);

            if (key != null)
            {
                if (key.OpenSubKey(subkey) != null) res = true;
            }

            return res;
        }

        public bool IsNameExists(string root, string subkey, string name)
        {
            bool res = false;

            RegistryKey key = ToRegistryRoot(root);

            if (key != null)
            {
                if (key.OpenSubKey(subkey) != null)
                {
                    key = key.OpenSubKey(subkey);
                    if (key.GetValue(name) != null) res = true;
                }
            }

            return res;
        }

        public string GetKeyParent(string key)
        {
            return key.Substring(0, key.LastIndexOf("\\"));
        }

        public void RegisterStartup(bool create = true)
        {
            string strStartUpFolderLocation = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string strshortcutAddress = strStartUpFolderLocation + @"\mCleaner.lnk";
            if (create)
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut) shell.CreateShortcut(strshortcutAddress);
                shortcut.Description = "mClener Shortcut for startup";
                shortcut.TargetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcleaner.exe");
                shortcut.Save();
            }
            else
            {
                if(System.IO.File.Exists(strshortcutAddress))
                    System.IO.File.Delete(strshortcutAddress);
            }

        }
    }
}
