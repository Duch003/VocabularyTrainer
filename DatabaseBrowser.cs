using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocabularyDatabase;

namespace VocabularyTrainer
{
    public static class DatabaseBrowser
    {
        public static void EntryPoint(IEnumerable<Question> q)
        {
            var pattern = "";
            var posLeft = 0;
            var posTop = 0;
            while (true)
            {
                Console.Clear();
                Console.Write("Type your pattern or ID or command here: " + pattern);
                posLeft = Console.CursorLeft;
                posTop = Console.CursorTop;
                var key = Console.ReadKey();
                if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar))
                    pattern += key.KeyChar;
                else if (key.Key == ConsoleKey.Backspace)
                    pattern = pattern.Remove(pattern.Length - 1, 1);
                if (pattern.Length == 0) continue;
                var ans = from z in q
                    where z.ID.ToString() == pattern ||
                          z.Polish.Contains(pattern) ||
                          z.English.Contains(pattern) ||
                          z.Category.Contains(pattern)
                    select z;

                TableParser.PrintListOfQuestions(ans);




            }
            
        }


    }
}
