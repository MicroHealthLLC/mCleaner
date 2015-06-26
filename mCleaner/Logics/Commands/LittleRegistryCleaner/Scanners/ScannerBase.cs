using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ScannerBase
    {
        public struct InvalidKeys
        {
            public string Key;
            public string Name;
            public string Value;
        }

        public List<InvalidKeys> BadKeys = new List<InvalidKeys>();
    }
}