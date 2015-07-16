using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HashFile
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo fi1 = new FileInfo(@"G:\Astro\Nov 28, 2014\Andromeda\Bias\DSC_2971_ekek.NEF");
            FileInfo fi2 = new FileInfo(@"G:\Astro\Nov 28, 2014\Orion\Bias\DSC_2970.NEF");

            Program p = new Program();

            Console.WriteLine("hasing file 1");
            string fi1_hash = p.HashFile(fi1.FullName);
            Console.WriteLine("hasing file 2");
            string fi2_hash = p.HashFile(fi2.FullName);

            Console.WriteLine(fi1_hash);
            Console.WriteLine(fi2_hash);

            Console.ReadLine();
        }

        public string HashFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return HashFile(fs);
            }
        }

        public string HashFile(FileStream stream)
        {
            StringBuilder sb = new StringBuilder();

            if (stream != null)
            {
                stream.Seek(0, SeekOrigin.Begin);

                MD5 md5 = MD5CryptoServiceProvider.Create();
                byte[] hash = md5.ComputeHash(stream);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                stream.Seek(0, SeekOrigin.Begin);
            }

            return sb.ToString();
        }
    }
}
