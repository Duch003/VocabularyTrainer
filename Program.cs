using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Colorful;
using Microsoft.SqlServer.Server;
using System.Security.Cryptography;
using VocabularyDatabase;
using Console = Colorful.Console;

namespace VocabularyTrainer
{
    class Program
    {
        private static Color _commandAccentuation = Color.Red;
        private static Color _titleColor = Color.Chartreuse;
        private static Color _loading = Color.SkyBlue;
        private static Color _success = Color.Green;
        private static Color _fail = Color.Red;
        private static float _unitOfRecognition = (float)1;

        private static string[][] _commands = new string[][]
        {
            new [] {"play", "Play quiz" },
            new [] {"add", "Add new question to database" },
            new [] {"add from file", "Deserialize and add records from records.xml" },
            new [] {"new template", "Creates new template to fill with new records" },
            new [] {"edit", "Edit question with particular ID" },
            new [] {"save all to file", "Saves all questions into records.xml" },
            new [] {"overwrite from file", "Reads all records from records.xml and overwrite all given records by ID" },
            new [] {"delete", "Removes from database record with particular ID" },
            new [] {"list", "Print list of all used categories" },
            new [] {"list vocabulary", "Prints all records related to given category" },
            new [] {"quit", "Close application" }
        };

        private static QuestionsSet _engine;

        private static void Main()
        {
            _engine = new QuestionsSet();
            while (true)
            {
                PrintDetails();
                var command = Console.ReadLine().ToLower();
                switch (command)
                {
                    case "play":
                        Play();
                        break;
                    case "add":
                        Add();
                        break;
                    case "add from file":
                        AddNewQuestion();
                        break;
                    case "new template":
                        CreateNewTemplate();
                        break;
                    case "edit":
                        Edit();
                        break;
                    case "delete":
                        Delete();
                        break;
                    case "list":
                        List();
                        break;
                    case "save all to file":
                        SaveAllToFile();
                        break;
                    case "overwrite from file":
                        OverwriteFromFile();
                        break;
                    case "list vocabulary":
                        ListVocabulary();
                        break;
                    case "quit":
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Play()
        {
            var userType = "";
            var listOfCategories = _engine.ListAllCategories();

            var listOfQuestions = new List<Question>();
            do
            {
                Console.Clear();
                Console.WriteLine(listOfCategories.ToStringTable(new[] {"Category"}, z => z));
                Console.WriteLine("Type your category below. If You want to back to main menu, type \"EXIT:\"",
                    _loading);
                userType = Console.ReadLine();
                listOfQuestions = ChoiceCategory(userType);
                if (userType.ToLower() == "exit")
                    break;
                if (listOfQuestions != null && listOfQuestions.Any())
                    continue;
                Console.WriteLine("Program can not find given category. Try again.", _fail);
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
            } while (userType.ToLower() == "exit" || (listOfQuestions == null || !listOfQuestions.Any()));

            if (userType.ToLower() == "exit")
                return;
            listOfQuestions = MyExtensions.Shuffle(listOfQuestions).ToList();

            do
            {
                var question = Draw(listOfQuestions);
                if (Ask(question, listOfQuestions.Count()))
                {
                    question.LevelOfRecognition += _unitOfRecognition;
                    if (question.LevelOfRecognition >= 1)
                        listOfQuestions.Remove(question);

                }
                //else
                        //question.LevelOfRecognition -= _unitOfRecognition;
                
            } while (listOfQuestions.Any());

            //Console.Clear();
            Console.WriteLine("You have finished! Well done!", _success);
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }

        private static void ListVocabulary()
        {
            Console.Clear();
            Console.WriteLine("Type the category You want to retrieve from records: ");
            var ans = Console.ReadLine();
            _engine.Find(ans);
            PrintListOfQuestions(_engine.Questions);
            Console.ReadLine();
        }

        private static void SaveAllToFile()
        {
            Console.Clear();
            Console.WriteLine("Creating a copy of existing file...", _loading);
            if (!XmlHandler.CreateCopy())
            {
                Console.WriteLine("Failure!", _fail);
                Console.WriteLine("An error occur during copying the file. Copy the file records.xml manually, then press ENTER.");
                Console.ReadLine();
            }
            else
                Console.WriteLine("Success", _success);
            XmlHandler.Delete();
            XmlHandler.CreateNewTemplate();
            _engine.GetAllRecords();
            XmlHandler.Serialize(_engine.Questions);
            Console.WriteLine("Done. All records from the database are in records.xml file.", _success);
            Console.WriteLine("To back to main menu, press ENTER...");
            Console.ReadLine();
        }

        private static void OverwriteFromFile()
        {
            Console.WriteLine("Deserializing recodrs.xml...", _loading);
            var questions = XmlHandler.Deserialize();
            Console.WriteLine("Success", _success);
            var listOfChanges = new List<Question>();
            foreach (var z in questions)
            {
                _engine.GetAllRecords();
                var questionFromDatabase = _engine.Questions.Find(y => y.ID == z.ID && y.ID != 0);
                if (questionFromDatabase == null)
                {
                    z.ID = 0;
                    _engine.Add(z);
                    listOfChanges.Add(z);
                    continue;
                }
                if (questionFromDatabase.English == z.English && questionFromDatabase.Polish == z.Polish && z.Category == questionFromDatabase.Category)
                    continue;
                _engine.Overwrite(z);
                listOfChanges.Add(z);


            }
            Console.WriteLine("Done. All records from the file overwrited.", _success);

            PrintListOfQuestions(listOfChanges);
            Console.WriteLine("To back to main menu, press ENTER...");
            Console.ReadLine();
        }

        private static bool Ask(Question q, int remaining)
        {
            //Liczba pozostałych słów
            //Tylko pl => eng

            Console.Clear();
            Console.WriteLine(q.Category, _titleColor);
            Console.WriteLine();
            Console.Write("Question ID:          ");
            Console.Write(q.ID, Color.DeepPink);
            Console.ForegroundColor = Color.LightGray;
            Console.WriteLine();
            Console.Write("Remaining:            ");
            Console.Write(remaining, Color.Orange);
            Console.ForegroundColor = Color.LightGray;
            Console.WriteLine();
            Console.Write("Level of recognition: ");
            Console.Write(q.LevelOfRecognition, Color.OrangeRed);
            Console.ForegroundColor = Color.LightGray;
            Console.Write(" / ");
            Console.Write("1 ", Color.Purple);
            Console.ForegroundColor = Color.LightGray;
            Console.Write("(" + _unitOfRecognition + " ppca)");
            Console.ForegroundColor = Color.LightGray;
            Console.WriteLine();
            Console.WriteLine();
            var anwser = "";
            var engToPl = false;

            Console.WriteLine("For given polish word/sentence:", _loading);
            Console.WriteLine("     " + q.Polish);
            Console.WriteLine("write english translation below: ", _loading);
            Console.Write("     ", Color.LightGray);
            
            anwser = Console.ReadLine();
            Console.WriteLine();
            if (string.Equals(anwser, q.English, StringComparison.CurrentCultureIgnoreCase))
            { 
                //Ta linia przywraca domyślny kolor liter, inaczej wszystkie
                //zostaną przekolorowane na następny wymieniony
                Console.Write("", Color.LightGray);
                Console.WriteLine("Well done! This is the correnct anwser!", _success);
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
                return true;
            }

            Console.Write("", Color.LightGray);
            Console.WriteLine("Wrong! The correct anwser was: ", _fail);
            MarkBadPart(anwser, q.English);
            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
            return false;
        }

        private static bool Draw()
        {
            var rnd = new Random();
            return rnd.Next(1, 100) % 2 == 0;
        }

        private static Question Draw(IEnumerable<Question> q)
        {
            var pool = q.Count() > 5 ? Enumerable.Range(0, 5) : Enumerable.Range(0, q.Count());
            pool = MyExtensions.Shuffle(pool.ToList());
            var id = pool.First();
            return q.ElementAt(id);
        }

        private static List<Question> ChoiceCategory(string category)
        {
            var listOfCategories = _engine.ListAllCategories();
            if (!listOfCategories.Contains(category))
                return null;
            
            _engine.Find(category);
            return _engine.Questions;
        }

        private static void List()
        {
            Console.Clear();
            Console.WriteLine(_engine.ListAllCategories().ToStringTable(new []{"Category"}, z => z));
            Console.ReadLine();
        }

        private static void Delete()
        {
            Console.Clear();
            var errors = new List<string>();
            Console.WriteLine("Type the id's of questions You want remove from database. Separate each number with \",\" symbol, then press ENTER.");
            foreach (var z in Console.ReadLine().Split(','))
            {
                Console.Write($"Attemt to remove question with ID: {z}", _loading);
                if (!int.TryParse(z, out var id))
                {
                    Console.Write("FAIL".PadRight(40), _fail);
                    errors.Add(z);
                    continue;
                }
                _engine.Remove(id);
                if (!_engine.IsLastMethodExecutedCorrectly)
                {
                    Console.Write("FAIL".PadRight(40), _fail);
                    errors.Add(z);
                }
                Console.Write("SUCCESS".PadRight(40), _success);
            }
            Console.WriteLine("To return to main menu press ENTER.");
            Console.ReadLine();
        }

        private static void Edit()
        {
            var key = "";
            do
            {
                Console.Clear();
                PrintListOfQuestions(_engine.Questions);
                Console.WriteLine("Type id of question You want change or type pattern to print all records that matches it: ");
                key = Console.ReadLine();
                if (int.TryParse(key, out var _unused_variable))
                    _engine.Find(int.Parse(key));
                else
                    _engine.Find(key);

            } while (_engine.Questions.Count != 1);

            if (!_engine.IsLastMethodExecutedCorrectly)
            {
                Console.Clear();
                Console.WriteLine("Can not find the question. Check last catched exception\nPress ENTER to continue.");
                Console.ReadLine();
                return;
            }

            Console.Clear();
            var question = _engine.Questions.First();
            Console.WriteLine($"Old polish pharse: {question.Polish}");
            var anwser = Console.ReadLine();
            question.Polish = string.IsNullOrEmpty(anwser) ? question.Polish : anwser;
            Console.WriteLine($"\nOld english pharse: {question.English}");
            anwser = Console.ReadLine();
            question.English = string.IsNullOrEmpty(anwser) ? question.English : anwser;
            Console.WriteLine($"\nOld category: {question.Category}");
            anwser = Console.ReadLine();
            question.Category = string.IsNullOrEmpty(anwser) ? question.Category : anwser;

            _engine.Overwrite(question);
            if (!_engine.IsLastMethodExecutedCorrectly)
            {
                Console.Clear();
                Console.WriteLine("Can not overwrite the question. Check last catched exception\nPress ENTER to continue.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Overwriting successfull.\n\n");

            _engine.Find(question.ID);
            //TODO Nie drukuje na ekranie zmienionego rekordu
            PrintListOfQuestions(_engine.Questions);
            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();

        }

        private static void Add()
        {
            Console.Clear();
            Console.WriteLine("Type english sentence:");
            var engSentence = Console.ReadLine();
            Console.WriteLine("Type polish sentence:");
            var plSentence = Console.ReadLine();
            Console.WriteLine("Type category associated with your qestion.");
            var category = Console.ReadLine();

            Console.WriteLine("\nAdding new record to the database...", _loading);

            if (AddNewQuestion(engSentence, plSentence, category))
            {
                _engine.Find(engSentence);

                Console.WriteLine();
                Console.WriteLine("New question added to the database correctly:\n", _success);
                PrintListOfQuestions();
                Console.WriteLine("\n\nClick ENTER to continue...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Can not add new record to the database. Reason:", _fail);
                Console.WriteLine(_engine.ShowLastException());
                Console.WriteLine("\n\nClick ENTER to continue...");
                Console.ReadLine();
            }
        }

        private static void PrintDetails()
        {
            Console.Clear();
            Console.WriteLine("VOCABULARY TRAINER", _titleColor);
            Console.WriteLine("List of commands: ");
            for (var i = 0; i < _commands.Length; i++)
            {
                Console.Write($"{i+1}. ");
                Console.Write(_commands[i][0], _commandAccentuation);
                Console.WriteLine($" - {_commands[i][1]}");
            }
        }

        private static bool AddNewQuestion(string eng, string pl, string category)
        {
            var question = new Question
            {
                Category = category,
                English = eng,
                Polish = pl
            };
            _engine.Add(question);
            return _engine.IsLastMethodExecutedCorrectly;
        }

        private static void AddNewQuestion()
        {
            Console.Clear();
            Console.WriteLine("Reading file...", _loading);
            var records = XmlHandler.Deserialize();
            var message = records == null ? "Could not read file." : "Records readed properly.";
            Console.WriteLine(message, records == null ? _fail : _success);
            if (records == null)
            {
                Console.WriteLine("Operation is aborted. Make sure that everything is ok within records.xml file.");
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Adding records to database", _loading);
            _engine.AddMany(records);
            if (_engine.IsLastMethodExecutedCorrectly)
            {
                Console.WriteLine("Records added successfully", _success);
                Console.WriteLine();
                PrintListOfQuestions(records);
                Console.WriteLine("\n\nClick ENTER to continue...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Can not add new records to the database. Reason:", _fail);
                Console.WriteLine(_engine.ShowLastException());
                Console.WriteLine("\n\nClick ENTER to continue...");
                Console.ReadLine();
            }
            
        }

        private static void PrintListOfQuestions()
        {
            Console.WriteLine(_engine.Questions.ToStringTable(
                new[] {"ID", "English sentence", "Polish sentence", "Category", "Level of difficulty"},
                z => z.ID,
                z => z.English,
                z => z.Polish,
                z => z.Category,
                z => z.LevelOfRecognition));
        }

        private static void PrintListOfQuestions(IEnumerable<Question> q)
        {
            Console.WriteLine(q.ToStringTable(
                new[] { "ID", "English sentence", "Polish sentence", "Category", "Level of difficulty" },
                z => z.ID,
                z => z.English,
                z => z.Polish,
                z => z.Category,
                z => z.LevelOfRecognition));
        }

        private static void CreateNewTemplate()
        {
            XmlHandler.CreateNewTemplate();
            Console.WriteLine("Template created properly. Press ENTER to continue...");
            Console.ReadLine();
        }

        private static void MarkBadPart(string given, string correct)
        {
            var splittedGiven = given.Split(' ');
            var splittedCorrect = correct.Split(' ');
            Console.Write("     ");
            for (var i = 0; i < splittedCorrect.Length; i++)
            {
                var splittedGivenChar = i >= splittedGiven.Length ? " '" : splittedGiven[i];
                Console.Write(splittedCorrect[i] + " ", splittedCorrect[i] == splittedGivenChar ? _success : _fail);
            }
        }
    }

    public static class MyExtensions
    {
        //Source: https://stackoverflow.com/questions/273313/randomize-a-listt

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
