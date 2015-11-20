
using System.Diagnostics;
using System.IO;
using System.Security;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class SystemDrivers : ScannerBase
    {
        public SystemDrivers() { }
        static SystemDrivers _i = new SystemDrivers();
        public static SystemDrivers I { get { return _i; } }

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

        //public async Task<bool> PreviewAsync()
        //{
        //    await Task.Run(() => Preview());
        //    return true;
        //}

        public override void Clean()
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

        public override void Preview()
        {
            this.BadKeys.Clear();

            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Drivers32"))
                {
                    if (regKey == null)
                        return;

                    foreach (string strDriverName in regKey.GetValueNames())
                    {
                        ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                        string strValue = regKey.GetValue(strDriverName) as string;

                        if (!string.IsNullOrEmpty(strValue))
                        {
                            string outfile = string.Empty;
                            if (!File.Exists(strValue) && !FileOperations.SearchPath(strValue, null, out outfile))
                            {
                                //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, strDriverName);
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = Registry.LocalMachine,
                                    Subkey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Drivers32",
                                    Key = string.Empty,
                                    Name = strDriverName
                                });
                            }
                        }
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
