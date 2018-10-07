using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocabularyDatabase;

namespace VocabularyTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var vocab = new QuestionsSet();
            vocab.GetAllRecords();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
