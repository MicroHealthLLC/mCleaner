using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string bookmark = @"C:\Users\Jayson\AppData\Local\Google\Chrome\User Data\Default\Bookmarks";
            string json = OpenJSONFiel(bookmark);

            JObject basenode = JObject.Parse(json);
            JToken basetoken = basenode["roots"]["bookmark_bar"]["children"];

            Stack<JToken> nodes = new Stack<JToken>();
            nodes.Push(basetoken);

            while (nodes.Count > 0)
            {
                JToken node = nodes.Pop();

                foreach (JToken t in node.Children())
                {
                    if (t.SelectToken("type") == null) continue;

                    if (t.SelectToken("type").ToString() == "folder")
                    {
                        nodes.Push(t["children"]);
                    }
                    else if (t.SelectToken("type").ToString() == "url")
                    {
                        Console.WriteLine(t.SelectToken("url"));
                    }
                }
            }            

            Console.ReadLine();
        }

        public static string OpenJSONFiel(string filename)
        {
            string json = string.Empty;

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                using (StreamReader reader = fi.OpenText())
                {
                    json = reader.ReadToEnd();
                }
            }

            return json;
        }
    }
}
