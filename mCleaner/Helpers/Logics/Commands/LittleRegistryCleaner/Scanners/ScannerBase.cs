using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public abstract class ScannerBase
    {
        public struct InvalidKeys
        {
            public RegistryKey Root;
            public string Subkey;
            public string Key;
            public string Name;
            public string Value;
        }

        public List<InvalidKeys> BadKeys = new List<InvalidKeys>();
        public DataTable dtRegistryKeyDeleted;
        public  string strRegistryBackupFolderPath=String.Empty;

        public async Task<bool> Clean(bool preview)
        {
            try
            {
                if (preview)
                {
                    await PreviewAsync();
                }
                else
                {
                    Clean();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return true;
        }

        public void BackUpRegistrykey(InvalidKeys badkey)
        {
            DataRow dr = dtRegistryKeyDeleted.NewRow();
            string strKey = String.Empty;
            string strCommnd = String.Empty;
            string strLocationToSave = Path.Combine(strRegistryBackupFolderPath, Guid.NewGuid().ToString() + ".dat");
            if (badkey.Root.Name == "HKEY_CLASSES_ROOT")
                strKey = "HKCR " + badkey.Subkey;
            else if (badkey.Root.Name == "HKEY_LOCAL_MACHINE")
            {
                strKey = "HKLM " + badkey.Subkey;
                strCommnd = "RegSaveRestore /S " + strKey + " " + strLocationToSave;
            }

            else if (badkey.Root.Name == "HKEY_CURRENT_USER")
            {
                strKey = "HKCU " + badkey.Subkey;
                strCommnd = "RegSaveRestore /S " + strKey + " " + strLocationToSave;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            var process = new Process { StartInfo = startInfo };

            process.Start();
            process.StandardInput.WriteLine("cd " + AppDomain.CurrentDomain.BaseDirectory);
            process.StandardInput.WriteLine(strCommnd);
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            dr["RegistryKeyFullPath"] = strKey;
            dr["Location"] = strLocationToSave;
            dtRegistryKeyDeleted.Rows.Add(dr);
        }

        public async Task<bool> PreviewAsync()
        {
            await Task.Run(() => Preview());
            return true;
        }

        public async Task<bool> CleanAsync()
        {
            await Task.Run(() => Clean());
            return true;
        }

        abstract public void Clean();

        abstract public void Preview();
    }
}