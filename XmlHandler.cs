using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VocabularyDatabase;

namespace VocabularyTrainer
{
    public static class XmlHandler
    {
        private static readonly string _fileNotFoundExceptionMessage =
            "Plik records.xml nie istnieje w podanej lokalizacji: " + Environment.CurrentDirectory;

        public static void CreateNewTemplate()
        {
            FileStream fileStream = null;
            fileStream = !File.Exists("records.xml") 
                ? File.Create("records.xml") 
                : new FileStream("records.xml", FileMode.OpenOrCreate);
            

            var emptyQuestion = new Question[]
            {
                new Question
                {
                    Category = "fill with category",
                    Polish = "fill with polish sentence here",
                    English = "fill with english sentence here",
                    ID = 0,
                    LevelOfRecognition = 0
                }
            };
            var serializer = new XmlSerializer(typeof(Question[]));
            serializer.Serialize(fileStream, emptyQuestion);
            fileStream.Close();
        }

        public static void Delete() => File.Delete("records.xml");

        public static void Serialize(IEnumerable<Question> q)
        {
            var serializer = new XmlSerializer(typeof(List<Question>));
            var stream = new FileStream("records.xml", FileMode.OpenOrCreate);
            serializer.Serialize(stream, q);
            stream.Close();
        }

        public static bool CreateCopy()
        {
            var result = true;
            try
            {
                File.Copy("records.xml", "CopyOfRecords.xml");
            }
            catch
            {
                result = true;
            }

            return result;
        }

        public static Question[] Deserialize()
        {
            if (!File.Exists("records.xml"))
                throw new FileNotFoundException(_fileNotFoundExceptionMessage);

            var serializer = new XmlSerializer(typeof(Question[]));
            Question[] ans = null;
            try
            {
                ans = (Question[]) serializer.Deserialize(new FileStream("records.xml", FileMode.Open));
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message, Color.Red);
                Console.ReadLine();
            }

            return ans;

        }
    }
}
