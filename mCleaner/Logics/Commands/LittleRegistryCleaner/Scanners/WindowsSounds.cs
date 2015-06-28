
using Microsoft.Win32;
using System.IO;
namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class WindowsSounds : ScannerBase
    {
        public WindowsSounds() { }
        static WindowsSounds _i = new WindowsSounds();
        public static WindowsSounds I { get { return _i; } }

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

        public void Clean()
        {
            Preview();

            foreach (InvalidKeys k in this.BadKeys)
            {
                using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                {
                    key.DeleteValue(k.Name);
                }
            }
        }

        public void Preview()
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
            catch (System.Security.SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
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
