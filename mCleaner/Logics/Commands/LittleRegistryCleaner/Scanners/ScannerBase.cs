using System.Collections.Generic;
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

        public async Task<bool> Clean(bool preview)
        {
            if (preview)
            {
                await PreviewAsync();
            }
            else
            {
                Clean();
            }

            return true;
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