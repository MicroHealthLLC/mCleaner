using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace mCleaner.Helpers
{
    public class FileOperations
    {
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PathGetArgs([In] string path);
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern void PathUnquoteSpaces([In, Out] StringBuilder path);
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern void PathRemoveArgs([In, Out] StringBuilder path);
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private extern static bool PathFileExists(string path);
        [DllImport("kernel32.dll")]
        public static extern int SearchPath(string strPath, string strFileName, string strExtension, uint nBufferLength, StringBuilder strBuffer, string strFilePart);
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int PathParseIconLocation([In, Out] StringBuilder path);

        #region Interop (IShellLink and IPersistFile)
        [Flags()]
        enum SLGP_FLAGS
        {
            /// <summary>Retrieves the standard short (8.3 format) file name</summary>
            SLGP_SHORTPATH = 0x1,
            /// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
            SLGP_UNCPRIORITY = 0x2,
            /// <summary>Retrieves the raw path name. A raw path is something that might not exist and may include environment variables that need to be expanded</summary>
            SLGP_RAWPATH = 0x4
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public long ftCreationTime;
            public long ftLastAccessTime;
            public long ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [Flags()]

        enum SLR_FLAGS
        {
            /// <summary>
            /// Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
            /// the high-order word of fFlags can be set to a time-out value that specifies the
            /// maximum amount of time to be spent resolving the link. The function returns if the
            /// link cannot be resolved within the time-out duration. If the high-order word is set
            /// to zero, the time-out duration will be set to the default value of 3,000 milliseconds
            /// (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
            /// duration, in milliseconds.
            /// </summary>
            SLR_NO_UI = 0x1,
            /// <summary>Obsolete and no longer used</summary>
            SLR_ANY_MATCH = 0x2,
            /// <summary>If the link object has changed, update its path and list of identifiers.
            /// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
            /// whether or not the link object has changed.</summary>
            SLR_UPDATE = 0x4,
            /// <summary>Do not update the link information</summary>
            SLR_NOUPDATE = 0x8,
            /// <summary>Do not execute the search heuristics</summary>
            SLR_NOSEARCH = 0x10,
            /// <summary>Do not use distributed link tracking</summary>
            SLR_NOTRACK = 0x20,
            /// <summary>Disable distributed link tracking. By default, distributed link tracking tracks
            /// removable media across multiple devices based on the volume name. It also uses the
            /// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
            /// has changed. Setting SLR_NOLINKINFO disables both types of tracking.</summary>
            SLR_NOLINKINFO = 0x40,
            /// <summary>Call the Microsoft Windows Installer</summary>
            SLR_INVOKE_MSI = 0x80
        }


        /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
        interface IShellLinkW
        {
            /// <summary>Retrieves the path and file name of a Shell link object</summary>
            void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);
            /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
            void GetIDList(out IntPtr ppidl);
            /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
            void SetIDList(IntPtr pidl);
            /// <summary>Retrieves the description string for a Shell link object</summary>
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            /// <summary>Sets the name of the working directory for a Shell link object</summary>
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            /// <summary>Sets the command-line arguments for a Shell link object</summary>
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            /// <summary>Retrieves the hot key for a Shell link object</summary>
            void GetHotkey(out short pwHotkey);
            /// <summary>Sets a hot key for a Shell link object</summary>
            void SetHotkey(short wHotkey);
            /// <summary>Retrieves the show command for a Shell link object</summary>
            void GetShowCmd(out int piShowCmd);
            /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
            void SetShowCmd(int iShowCmd);
            /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath, out int piIcon);
            /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            /// <summary>Sets the relative path to the Shell link object</summary>
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
            void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);
            /// <summary>Sets the path and file name of a Shell link object</summary>
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);

        }

        [ComImport, Guid("0000010c-0000-0000-c000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersist
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
        }


        [ComImport, Guid("0000010b-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistFile : IPersist
        {
            new void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();

            [PreserveSig]
            void Load([In, MarshalAs(UnmanagedType.LPWStr)]
            string pszFileName, uint dwMode);

            [PreserveSig]
            void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

            [PreserveSig]
            void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            [PreserveSig]
            void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        const uint STGM_READ = 0;
        const int MAX_PATH = 260;

        // CLSID_ShellLink from ShlGuid.h 
        [
            ComImport(),
            Guid("00021401-0000-0000-C000-000000000046")
        ]
        public class ShellLink
        {
        }

        #endregion

        public FileOperations()
        {

        }
        private static FileOperations _i = new FileOperations();
        public static FileOperations I { get { return _i; } }

        /// <summary>
        /// just convert the $ENVIRONMENT_VARIABLE to Windows path
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public string GetSpecialFolderPath(string var)
        {
            string res = var;

            string _var = res.Substring(1, res.IndexOf('\\') - 1);

            res = Environment.GetEnvironmentVariable(_var) + "" + var.Substring(_var.Length + 1);

            return res;
        }

        public List<string> GetFilesRecursive(string b, List<string> allowedextension)
        {
            List<string> result = new List<string>();
            Stack<string> stack = new Stack<string>();

            stack.Push(b);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                try
                {
                    result.AddRange(Directory.GetFiles(dir, "*.*"));

                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                        DirectoryInfo di = new DirectoryInfo(dn);
                    }

                    foreach (string fn in Directory.GetFiles(dir, "*.*"))
                    {
                        FileInfo fi = new FileInfo(fn);
                        if (allowedextension.Count == 0)
                        {
                            result.Add(fi.Name);
                        }
                        else
                        {
                            if (allowedextension.Contains(fi.Extension.ToLower()))
                            {
                                result.Add(fi.Name);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return result;
        }

        // https://msdn.microsoft.com/en-us/library/bb513869.aspx
        public List<string> GetFilesRecursive(string b, string regex = "")
        {
            List<string> result = new List<string>();

            // Data structure to hold names of subfolders to be 
            // examined for files.
            Stack<string> dirs = new Stack<string>(20);

            if (!System.IO.Directory.Exists(b))
            {
                throw new ArgumentException();
            }
            dirs.Push(b);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                // An UnauthorizedAccessException exception will be thrown if we do not have 
                // discovery permission on a folder or file. It may or may not be acceptable  
                // to ignore the exception and continue enumerating the remaining files and  
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                // will be raised. This will happen if currentDir has been deleted by 
                // another application or thread after our call to Directory.Exists. The  
                // choice of which exceptions to catch depends entirely on the specific task  
                // you are intending to perform and also on how much you know with certainty  
                // about the systems on which this code will run. 
                catch (UnauthorizedAccessException e)
                {                    
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Perform the required action on each file here. 
                // Modify this block to perform your required task. 
                foreach (string file in files)
                {
                    try
                    {
                        // Perform whatever action is required in your scenario.

                        if (regex != null && regex != string.Empty)
                        {
                            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
                            Regex reg = new Regex(regex, options);
                            if (reg.IsMatch(file))
                            {
                                result.Add(file);
                            }
                        }
                        else
                        {
                            result.Add(file);
                        }
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            return result;
        }

        public List<string> GetFoldersRecursive(string b)
        {
            List<string> result = new List<string>();

            // Data structure to hold names of subfolders to be 
            // examined for files.
            Stack<string> dirs = new Stack<string>();

            if (!System.IO.Directory.Exists(b))
            {
                throw new ArgumentException();
            }
            dirs.Push(b);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }

                // An UnauthorizedAccessException exception will be thrown if we do not have 
                // discovery permission on a folder or file. It may or may not be acceptable  
                // to ignore the exception and continue enumerating the remaining files and  
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                // will be raised. This will happen if currentDir has been deleted by 
                // another application or thread after our call to Directory.Exists. The  
                // choice of which exceptions to catch depends entirely on the specific task  
                // you are intending to perform and also on how much you know with certainty  
                // about the systems on which this code will run. 
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                    result.Add(str);
                }
            }

            return result;
        }

        public List<string> GetEmptyDirectories(string root)
        {
            List<string> ret = new List<string>();

            foreach (var directory in Directory.GetDirectories(root))
            {
                
                if (Directory.GetFiles(directory).Length == 0 
                    //&&
                    //Directory.GetDirectories(directory).Length == 0
                   )
                {
                    ret.Add(directory);
                }

                GetEmptyDirectories(directory);
            }

            return ret;
        }

        public void DeleteEmptyDirectories(string dir, Action<string> callback)
        {
            foreach (var directory in Directory.GetDirectories(dir))
            {
                DeleteEmptyDirectories(directory, callback);
                if (Directory.GetFiles(directory).Length == 0)
                {
                    try
                    {
                        Directory.Delete(directory, false);

                        callback(directory);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR while deleting a folder '{0}': " + ex.Message, directory);
                    }
                }
            }
        }

        public bool ResolveShortcut(string shortcut, out string filepath, out string arguments)
        {
            ShellLink link = new ShellLink();
            ((IPersistFile)link).Load(shortcut, STGM_READ);
            // TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
            // ((IShellLinkW)link).Resolve(hwnd, 0) 
            StringBuilder path = new StringBuilder(MAX_PATH);
            WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();
            ((IShellLinkW)link).GetPath(path, path.Capacity, out data, 0);

            StringBuilder args = new StringBuilder(MAX_PATH);
            ((IShellLinkW)link).GetArguments(args, args.Capacity);

            filepath = path.ToString();
            arguments = args.ToString();

            if (!File.Exists(filepath))
                return false;

            return true;
        }

        /// <summary>
        /// Uses PathGetArgs and PathRemoveArgs API to extract file arguments
        /// </summary>
        /// <param name="cmdLine">Command Line</param>
        /// <param name="filePath">file path</param>
        /// <param name="fileArgs">arguments</param>
        /// <exception cref="ArgumentNullException">Thrown when cmdLine is null or empty</exception>
        /// <returns>False if the path doesnt exist</returns>
        public static bool ExtractArguments(string cmdLine, out string filePath, out string fileArgs)
        {
            StringBuilder strCmdLine = new StringBuilder(cmdLine.ToLower().Trim());

            filePath = fileArgs = "";

            if (string.IsNullOrEmpty(cmdLine))
                return false;

            fileArgs = Marshal.PtrToStringAuto(PathGetArgs(strCmdLine.ToString()));
            //fileArgs = string.Copy(PathGetArgs(strCmdLine.ToString()));

            PathRemoveArgs(strCmdLine);

            filePath = string.Copy(strCmdLine.ToString());

            if (!string.IsNullOrEmpty(filePath))
                if (File.Exists(filePath))
                    return true;

            return false;
        }

        public static bool ExtractArguments2(string cmdLine, out string filePath, out string fileArgs)
        {
            string strCmdLine = string.Copy(cmdLine.ToLower().Trim());
            bool bRet = false;

            filePath = fileArgs = "";

            if (string.IsNullOrEmpty(strCmdLine))
                return false;

            //if (strCmdLine.Contains("spotify"))
            //{
            //    Debug.WriteLine("t");
            //}

            // Remove Quotes
            strCmdLine = UnqouteSpaces(strCmdLine);

            // Expand variables
            strCmdLine = Environment.ExpandEnvironmentVariables(strCmdLine);

            // Try to see file exists by combining parts
            StringBuilder strFileFullPath = new StringBuilder();
            foreach (char ch in strCmdLine.ToCharArray())
            {
                strFileFullPath.Append(ch);

                // See if part exists
                //FileInfo fi = new FileInfo
                bool filexists = PathFile_Exists(strFileFullPath.ToString()); // File.Exists(strFileFullPath.ToString());
                //bool filexists = File.Exists(strFileFullPath.ToString());

                if (filexists)
                {
                    //filePath = string.Copy(strFileFullPath.ToString());
                    bRet = true;
                    break;
                }
            }

            //if (bRet && nPos > 0)
            //    fileArgs = strCmdLine.Remove(0, nPos).Trim();

            return bRet;
        }

        public static bool PathFile_Exists(string pathfile)
        {
            return PathFileExists(pathfile);
        }

        /// <summary>
        /// Removes quotes from the path
        /// </summary>
        /// <param name="Path">Path w/ quotes</param>
        /// <returns>Path w/o quotes</returns>
        private static string UnqouteSpaces(string Path)
        {
            //StringBuilder sb = new StringBuilder(Path);

            //PathUnquoteSpaces(sb);

            //return string.Copy(sb.ToString());

            return Path.Replace("\"", string.Empty);
        }

        public static bool SearchPath(string fileName, string Path, out string retPath)
        {
            StringBuilder strBuffer = new StringBuilder(260);

            int ret = SearchPath(((!string.IsNullOrEmpty(Path)) ? (Path) : (null)), fileName, null, 260, strBuffer, null);

            if (ret != 0)
            {
                retPath = strBuffer.ToString();
                return true;
            }
            else
                retPath = "";

            return false;
        }

        /// <summary>
        /// Gets the icon path and sees if it exists
        /// </summary>
        /// <param name="IconPath">The icon path</param>
        /// <returns>True if it exists</returns>
        public static bool IconExists(string IconPath)
        {
            string strFileName = string.Copy(IconPath.Trim().ToLower());

            // Remove quotes
            strFileName = UnqouteSpaces(strFileName);

            // Remove starting @
            if (strFileName.StartsWith("@"))
                strFileName = strFileName.Substring(1);

            // Return true if %1
            if (strFileName == "%1")
                return true;

            // Get icon path
            int nSlash = strFileName.IndexOf(',');
            if (nSlash > -1)
            {
                strFileName = strFileName.Substring(0, nSlash);

                return FileExists(strFileName);
            }
            else
            {
                StringBuilder sb = new StringBuilder(strFileName);
                if (PathParseIconLocation(sb) >= 0)
                {
                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        return FileExists(sb.ToString());
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sees if the file exists
        /// </summary>
        /// <remarks>Always use this to check for files in the scanners!</remarks>
        /// <param name="filePath">The filename (including path)</param>
        /// <returns>
        /// True if it exists or if the path should be skipped. Otherwise, false if the file path is empty or doesnt exist
        /// </returns>
        public static bool FileExists(string filePath)
        {
            if (filePath.Contains("iyuv_32.dll"))
            {
                int a = 0;
            }

            if (string.IsNullOrEmpty(filePath))
                return false;

            string strFileName = string.Copy(filePath.Trim().ToLower());

            // Remove quotes
            strFileName = UnqouteSpaces(strFileName);

            // Expand environment variables
            strFileName = Environment.ExpandEnvironmentVariables(strFileName);

            // Check for illegal characters
            if (FindAnyIllegalChars(strFileName))
                return false;

            //// Check Drive Type
            //VDTReturn ret = ValidDriveType(strFileName);
            //if (ret == VDTReturn.InvalidDrive)
            //    return false;
            //else if (ret == VDTReturn.SkipCheck)
            //    return true;

            //// See if it is on exclude list
            //if (ScanDlg.IsOnIgnoreList(strFileName))
            //    return true;

            // Now see if file exists
            if (File.Exists(strFileName))
                return true;

            if (PathFileExists(strFileName))
                return true;

            if (SearchPath(strFileName))
                return true;

            return false;
        }

        /// <summary>
        /// Parses the path and checks for any illegal characters
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>Returns true if it contains illegal characters</returns>
        private static bool FindAnyIllegalChars(string path)
        {
            // Get directory portion of the path.
            string dirName = path;
            string fullFileName = "";
            int pos = 0;
            if ((pos = path.LastIndexOf(Path.DirectorySeparatorChar)) >= 0)
            {
                dirName = path.Substring(0, pos);

                // Get filename portion of the path.
                if (pos >= 0 && (pos + 1) < path.Length)
                    fullFileName = path.Substring(pos + 1);
            }

            // Find any characters in the directory that are illegal.
            if (dirName.IndexOfAny(Path.GetInvalidPathChars()) != -1) // Found invalid character in directory
                return true;

            // Find any characters in the filename that are illegal.
            if (!string.IsNullOrEmpty(fullFileName))
                if (fullFileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) // Found invalid character in filename
                    return true;

            return false;
        }

        public static bool SearchPath(string fileName)
        {
            string retPath = "";

            return SearchPath(fileName, null, out retPath);
        }

        public static void Delete(string filename)
        {
            FileInfo fi = new FileInfo(filename);

            if (fi.Exists)
            {
                if (Properties.Settings.Default.ShredFiles)
                {
                    WipeFile(fi.FullName, 5);
                }
                else
                {
                    fi.Delete();
                }
            }
        }

        /// <summary>
        /// Deletes a file in a secure way by overwriting it with
        /// random garbage data n times.
        /// </summary>
        /// <param name="filename">Full path of the file to be deleted</param>
        /// <param name="timesToWrite">Specifies the number of times the file should be overwritten</param>
        public static void WipeFile(string filename, int timesToWrite)
        {
            try
            {
                if (File.Exists(filename))
                {
                    // Set the files attributes to normal in case it's read-only.
                    File.SetAttributes(filename, FileAttributes.Normal);

                    // Calculate the total number of sectors in the file.
                    double sectors = Math.Ceiling(new FileInfo(filename).Length / 512.0);

                    // Create a dummy-buffer the size of a sector.
                    byte[] dummyBuffer = new byte[512];

                    // Create a cryptographic Random Number Generator.
                    // This is what I use to create the garbage data.
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                    // Open a FileStream to the file.
                    FileStream inputStream = new FileStream(filename, FileMode.Open);
                    for (int currentPass = 0; currentPass < timesToWrite; currentPass++)
                    {
                        //UpdatePassInfo(currentPass + 1, timesToWrite);

                        // Go to the beginning of the stream
                        inputStream.Position = 0;

                        // Loop all sectors
                        for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                        {
                            //UpdateSectorInfo(sectorsWritten + 1, (int)sectors);

                            // Fill the dummy-buffer with random data
                            rng.GetBytes(dummyBuffer);
                            // Write it to the stream
                            inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                        }
                    }
                    // Truncate the file to 0 bytes.
                    // This will hide the original file-length if you try to recover the file.
                    inputStream.SetLength(0);
                    // Close the stream.
                    inputStream.Close();

                    // As an extra precaution I change the dates of the file so the
                    // original dates are hidden if you try to recover the file.
                    DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
                    File.SetCreationTime(filename, dt);
                    File.SetLastAccessTime(filename, dt);
                    File.SetLastWriteTime(filename, dt);

                    File.SetCreationTimeUtc(filename, dt);
                    File.SetLastAccessTimeUtc(filename, dt);
                    File.SetLastWriteTimeUtc(filename, dt);

                    // Finally, delete the file
                    File.Delete(filename);

                    //WipeDone();
                }
            }
            catch (Exception e)
            {
                //WipeError(e);
                Debug.WriteLine("FileOperations.WipeFile()");
            }
        }
    }
}