using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace mCleaner.Helpers
{
    public class FileOperations
    {
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

        public void DeleteFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                fi.Delete();
            }
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
    }
}
