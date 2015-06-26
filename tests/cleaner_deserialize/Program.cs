using mCleaner.Model;

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace cleaner_deserialize
{
    class Program
    {
        static void Main(string[] args)
        {
            Model_CleanerML CleanerML = new Model_CleanerML();

            XmlSerializer srlzr = new XmlSerializer(typeof(cleaner));

            //using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(mCleaner.App.testcleaner)))
            //{
            //    cleaner clnr = (cleaner)srlzr.Deserialize(stream);
            //    CleanerML.CleanerML = clnr;

            //    Console.WriteLine("issupported=" + CleanerML.isSupported);
            //    Console.WriteLine("id=" + clnr.id);
            //    Console.WriteLine("label=" + clnr.label);
            //    Console.WriteLine("description=" + clnr.description);

            //    foreach (running r in clnr.running)
            //    {
            //        Console.WriteLine("type=" + r.type + ">" + r.text);
            //    }

            //    foreach (option o in clnr.option)
            //    {
            //        Console.WriteLine("\tid=" + o.id);
            //        Console.WriteLine("\tlabel=" + o.label);
            //        Console.WriteLine("\tdescription=" + o.description);
            //        foreach (action a in o.action)
            //        {
            //            Console.WriteLine("\t\tcommand=" + a.command + " search=" + a.search + " regex=" + a.regex + " path=" + a.path);
            //        }
            //    }
            //}

            Console.WriteLine("\r\n\r\n\r\ndeserialization done");
            Console.ReadLine();
        }
    }
}
