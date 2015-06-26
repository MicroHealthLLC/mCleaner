using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace mCleaner.Helpers
{
    public static class Win32API
    {
        // used for converting file length to short file size
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(long fileSize, StringBuilder buffer, int bufferSize);

        // used for reading/writing INI file
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, StringBuilder lpszReturnBuffer, int nSize, string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, IntPtr lpReturnedString, uint nSize, string lpFileName);

        public static string FormatByteSize(long filesize)
        {
            StringBuilder sb = new StringBuilder(11);
            StrFormatByteSize(filesize, sb, sb.Capacity);
            return sb.ToString();
        }

        public static class IniHelper
        {
            const int MaxSectionSize = 32767; // 32 KB

            static string OpenIni(string filename)
            {
                string res = string.Empty;
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                {
                    using (StreamReader reader = fi.OpenText())
                    {
                        res = reader.ReadToEnd();
                    }
                }
                return res;
            }

            public static bool IsSectionExists(string filename, string section)
            {
                bool res = false;
                string regex = @"\[(?<section>.*)\]";
                RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
                Regex reg = new Regex(regex, options);
                string inicontent = OpenIni(filename);
                if (reg.IsMatch(inicontent))
                {
                    foreach (Match m in reg.Matches(inicontent))
                    {
                        if (m.Groups["section"].Value == section)
                        {
                            res = true;
                            break;
                        }
                    }
                }

                return res;
            }

            public static string GetValue(string filename, string section, string name)
            {
                StringBuilder val = new StringBuilder(255);
                int i = GetPrivateProfileString(section, name, string.Empty, val, val.Capacity, filename);

                return val.ToString();
            }

            public static void SetValue(string filename, string section, string name, string val)
            {
                WritePrivateProfileString(section, name, val, filename);
            }

            //public static List<string> GetSections(string iniFile)
            //{
            //    string returnString = new string(' ', 65536);
            //    GetPrivateProfileString(null, null, null, returnString, 65536, iniFile);
            //    List<string> result = new List<string>(returnString.Split('\0'));
            //    result.RemoveRange(result.Count - 2, 2);
            //    return result;
            //}

            public static string[] GetKeyNames(string sectionName, string filename)
            {
                int len;
                string[] retval;

                if (sectionName == null)
                    throw new ArgumentNullException("sectionName");

                //Allocate a buffer for the returned section names.
                IntPtr ptr = Marshal.AllocCoTaskMem(MaxSectionSize);

                try
                {
                    //Get the section names into the buffer.
                    len = GetPrivateProfileString(sectionName,
                                                null,
                                                null,
                                                ptr,
                                                MaxSectionSize,
                                                filename);

                    retval = ConvertNullSeperatedStringToStringArray(ptr, len);
                }
                finally
                {
                    //Free the buffer
                    Marshal.FreeCoTaskMem(ptr);
                }

                return retval;
            }

            /// <summary>
            /// Converts the null seperated pointer to a string into a string array.
            /// </summary>
            /// <param name="ptr">A pointer to string data.</param>
            /// <param name="valLength">
            /// Length of the data pointed to by <paramref name="ptr"/>.
            /// </param>
            /// <returns>
            /// An array of strings; one for each null found in the array of characters pointed
            /// at by <paramref name="ptr"/>.
            /// </returns>
            private static string[] ConvertNullSeperatedStringToStringArray(IntPtr ptr, int valLength)
            {
                string[] retval;

                if (valLength == 0)
                {
                    //Return an empty array.
                    retval = new string[0];
                }
                else
                {
                    //Convert the buffer into a string.  Decrease the length 
                    //by 1 so that we remove the second null off the end.
                    string buff = Marshal.PtrToStringAuto(ptr, valLength - 1);

                    //Parse the buffer into an array of strings by searching for nulls.
                    retval = buff.Split('\0');
                }

                return retval;
            }
        }
    }
}
