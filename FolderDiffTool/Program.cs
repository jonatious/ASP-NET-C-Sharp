using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using System.IO;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter directory path int this format: X:\\YYYY\\ZZZZZ");
            string source = Console.ReadLine();
            source += "\\";
            source = source.Replace("\\", "\\\\");

            Console.WriteLine("Enter the word you want replaced");
            string convert = Console.ReadLine();

            Console.WriteLine("Enter the word you want to replace to");
            string to = Console.ReadLine();

            if (Directory.Exists(source))
            {
                string dest = source + "output\\";
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);

                DirectoryInfo di = new DirectoryInfo(source);
                IEnumerable<FileInfo> files = di.EnumerateFiles();

                foreach (FileInfo file in files)
                {
                    VerySimpleReplaceText(source + file.ToString(), dest + file.ToString(), convert, to);
                }
                Console.WriteLine("PDF edit successfull!! Converted all '" + convert + "' to '" + to + "'!!");

            }
            else
                Console.WriteLine("Invalid path or Directory does not exist!!");
            Console.ReadLine();
        }
        static void VerySimpleReplaceText(string OrigFile, string ResultFile, string origText, string replaceText)
        {
            using (PdfReader reader = new PdfReader(OrigFile))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    byte[] contentBytes = reader.GetPageContent(i);
                    string contentString = PdfEncodings.ConvertToString(contentBytes, PdfObject.TEXT_PDFDOCENCODING);
                    contentString = contentString.Replace(origText, replaceText);
                    reader.SetPageContent(i, PdfEncodings.ConvertToBytes(contentString, PdfObject.TEXT_PDFDOCENCODING));
                }
                new PdfStamper(reader, new FileStream(ResultFile, FileMode.Create, FileAccess.Write)).Close();
            }
        }
    }
}
