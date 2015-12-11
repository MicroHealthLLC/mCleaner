
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class WindowsSounds : ScannerBase
    {
        public WindowsSounds() { }
        static WindowsSounds _i = new WindowsSounds();
        public static WindowsSounds I { get { return _i; } }

        public override void Clean()
        {
            try
            {
                Preview();

                foreach (InvalidKeys k in this.BadKeys)
                {
                    using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                    {
                        BackUpRegistrykey(k);
                        key.DeleteValue(k.Name);
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

            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("AppEvents\\Schemes\\Apps"))
                {
                    if (regKey != null)
                    {
                        ParseSoundKeys(regKey);
                    }
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ParseSoundKeys(RegistryKey rk)
        {
            foreach (string strSubKey in rk.GetSubKeyNames())
            {

                // Ignores ".Default" Subkey
                if ((strSubKey.CompareTo(".Current") == 0) || (strSubKey.CompareTo(".Modified") == 0))
                {
                    // Gets the (default) key and sees if the file exists
                    RegistryKey rk2 = rk.OpenSubKey(strSubKey);

                    if (rk2 != null)
                    {
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", rk2.ToString(), string.Empty));

                        string strSoundPath = rk2.GetValue("") as string;

                        if (!string.IsNullOrEmpty(strSoundPath))
                        {
                            if (!File.Exists(strSoundPath))
                            {
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = Registry.LocalMachine,
                                    Subkey = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts",
                                    Key = string.Empty,
                                    Name = "(default)"
                                });
                            }
                        }
                    }

                }
                else if (!string.IsNullOrEmpty(strSubKey))
                {
                    RegistryKey rk2 = rk.OpenSubKey(strSubKey);
                    if (rk2 != null)
                    {
                        ParseSoundKeys(rk2);
                    }
                }

            }

            return;
        }
    }
}
