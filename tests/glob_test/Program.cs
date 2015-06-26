using System;
using System.Collections.Generic;

namespace glob_test
{
    class Program
    {
        static void Main(string[] args)
        {
            string glob_path = @"C:/Users/Jayson/AppData/Local/Mozilla/Firefox/Profiles/*.default*/OfflineCache/index.sqlite";

            IEnumerable<string> matches = GlobDir.Glob.GetMatches(glob_path, GlobDir.Glob.Constants.IgnoreCase);
            foreach (string m in matches)
            {
                Console.WriteLine(m);
            }

            Console.Write("asd");
            Console.ReadLine();
        }
    }
}
