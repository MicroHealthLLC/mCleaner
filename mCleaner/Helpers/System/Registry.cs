using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
