﻿using System;

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
