using CodeBureau;

using mCleaner.Logics;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;

using System;
using System.IO;
using System.Xml.Serialization;

namespace action_test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Worker.I.DoWork();
            Worker.I.Preview = true;
            Model_CleanerML CleanerML = new Model_CleanerML();

            XmlSerializer srlzr = new XmlSerializer(typeof(cleaner));

            FileInfo fi = new FileInfo(@"D:\Clients\oDesk\2015\Frank\mCleaner\mCleaner\mCleaner\Cleaners\freerider.xml");

            //using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(mCleaner.App.testcleaner)))
            {
                cleaner clnr = (cleaner)srlzr.Deserialize(fi.OpenText());
                CleanerML.CleanerML = clnr;

                foreach (option o in clnr.option)
                {
                    foreach (action a in o.action)
                    {
                        Console.WriteLine("Executing '{0}' command with '{1}' search parameter in '{2}' path", a.command, a.search, a.path);

                        COMMANDS cmd = (COMMANDS)StringEnum.Parse(typeof(COMMANDS), a.command);

                        iActions axn = null;

                        switch (cmd)
                        {
                            case COMMANDS.delete:
                                axn = new CommandLogic_Delete();
                                break;
                            case COMMANDS.sqlite_vacuum:
                                break;
                            case COMMANDS.truncate:
                                break;
                            case COMMANDS.winreg:
                                break;
                        }

                        if (axn != null)
                        {
                            axn.Action = a;
                            axn.Execute();
                        }
                    }
                }
            }

            Worker.I.PreviewWork();

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}
