
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class WindowsFonts : ScannerBase
    {
        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder strPath, int nFolder, bool fCreate);

        const int CSIDL_FONTS = 0x0014;    // windows\fonts 

        public WindowsFonts() { }
        static WindowsFonts _i = new WindowsFonts();
        public static WindowsFonts I { get { return _i; } }

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
                            BackUpRegistrykey(k);
                            key.DeleteValue(k.Name);
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

            StringBuilder strPath = new StringBuilder(260);

            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts"))
                {
                    if (regKey == null)
                        return;

                    if (!SHGetSpecialFolderPath(IntPtr.Zero, strPath, CSIDL_FONTS, false))
                        return;

                    ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                    foreach (string strFontName in regKey.GetValueNames())
                    {
                        string strValue = regKey.GetValue(strFontName) as string;

                        // Skip if value is empty
                        if (string.IsNullOrEmpty(strValue))
                            continue;

                        string sout = string.Empty;

                        // Check value by itself
                        if (File.Exists(strValue))
                            continue;

                        // Check for font in fonts folder
                        string strFontPath = String.Format("{0}\\{1}", strPath.ToString(), strValue);

                        if (!File.Exists(strFontPath))
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), strFontName);
                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.LocalMachine,
                                Subkey = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts",
                                Key = string.Empty,
                                Name = strFontName
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
    }
}
