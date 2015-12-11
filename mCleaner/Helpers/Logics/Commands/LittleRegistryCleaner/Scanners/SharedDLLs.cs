
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class SharedDLLs : ScannerBase
    {
        public SharedDLLs() { }
        static SharedDLLs _i = new SharedDLLs();
        public static SharedDLLs I { get { return _i; } }

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
                            if (key != null)
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
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\SharedDLLs");

                if (regKey == null)
                    return;

                // Validate Each DLL from the value names
                foreach (string strFilePath in regKey.GetValueNames())
                {
                    ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                    if (!string.IsNullOrEmpty(strFilePath))
                    {
                        if (!File.Exists(strFilePath))
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, strFilePath);

                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.LocalMachine,
                                Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\SharedDLLs",
                                Key = string.Empty,
                                Name = strFilePath
                            });
                        }
                    }
                }

                regKey.Close();
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
