using Microsoft.Win32;
using System.Linq;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ApplicationInfo : ScannerBase
    {
        public ApplicationInfo()
        {
            
        }
        static ApplicationInfo _i = new ApplicationInfo();
        public static ApplicationInfo I { get { return _i; } }

        void Preview()
        {
            this.BadKeys.Clear();

            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
            {
                if (regKey == null)
                    return;

                foreach (string strProgName in regKey.GetSubKeyNames())
                {
                    using (RegistryKey regKey2 = regKey.OpenSubKey(strProgName, true))
                    {
                        if (regKey2 != null)
                        {
                            // check entries if it has "DisplayName"
                            // without it, it's not a valid UninstallKey

                            string[] ValueNames = regKey2.GetValueNames();
                            if (!ValueNames.Contains("DisplayName"))
                            {
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = Registry.LocalMachine,
                                    Subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
                                    Key = strProgName
                                });
                            }
                        }
                    }
                }
            }

            // didn't have these keys in my local machine
            // TODO: What are these?
            //checkARPCache(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache\"));
            //checkARPCache(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache\"));
        }

        void Clean()
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

        public void Clean(bool preview)
        {
            if (preview)
            {
                Preview();
            }
            else
            {
                Clean();
            }
        }
    }
}
