using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCleaner.Helpers
{
    public class Utils
    {
        public static bool Is64BitOS
        {
            get { return (IntPtr.Size == 8); }
        }
    }
}
