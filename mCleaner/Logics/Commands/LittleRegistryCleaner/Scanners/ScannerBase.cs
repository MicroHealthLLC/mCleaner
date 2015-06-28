using Microsoft.Win32;
using System.Collections.Generic;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ScannerBase
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
    }
}