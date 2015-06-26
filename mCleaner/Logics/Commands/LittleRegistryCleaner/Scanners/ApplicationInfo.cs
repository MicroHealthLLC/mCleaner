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

        public void Preview()
        {
            this.BadKeys.Clear();

            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
            {
                if (regKey == null)
                    return;

                foreach (string strProgName in regKey.GetSubKeyNames())
                {
                    using (RegistryKey regKey2 = regKey.OpenSubKey(strProgName))
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
                                    Key = regKey2.ToString()
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
    }
}
