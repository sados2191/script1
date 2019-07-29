using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace script1
{
    static class Program
    {
        private static List<String> dictionariesPath = Directory.GetFiles(@"C:\Dictionaries\").ToList();
        private static string duplicatesPath = @"C:\Dictionaries\EntryIdDuplicates\";

        static void Main(string[] args)
        {
            if (!Directory.Exists(@"C:\Dictionaries\"))
            {
                Console.WriteLine("Before using the script please copy dictionaries files to C:\\Dictionaries\\");
                Console.ReadKey();
            }
            else
            {
                //Deleting old files with duplicates
                Directory.CreateDirectory(duplicatesPath);
                DirectoryInfo directory = new DirectoryInfo(duplicatesPath);
                foreach (var file in directory.GetFiles())
                {
                    file.Delete();
                }

                Menu();
            }
        }

        private static void Menu()
        {
            List<String> list = new List<String>();

            int x = 0;
            do
            {
                Console.Clear();
                Console.WriteLine("IHS Markit - task");
                Console.WriteLine();
                Console.WriteLine("1. Return values of all <DictionaryID>");
                Console.WriteLine("2. Return unique values of all <DictionaryID>");
                Console.WriteLine("3. Return duplicates of <EntryID>");
                Console.WriteLine("4. Exit");
                int.TryParse(Console.ReadLine(), out x);
                Console.WriteLine();

                switch (x)
                {
                    case 1:
                        list = ReturnDictionaryIDs(dictionariesPath);
                        foreach (var item in list)
                        {
                            Console.WriteLine(item);
                        }
                        Console.WriteLine();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 2:
                        list = ReturnUniqueDictionaryIDs(dictionariesPath);
                        foreach (var item in list)
                        {
                            Console.WriteLine(item);
                        }
                        Console.WriteLine();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 3:
                        ReturnEntryIdDuplicates(dictionariesPath);
                        Console.WriteLine("Files with duplicates are saved in {0}", duplicatesPath);
                        Console.WriteLine();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Wrong input");
                        break;
                }
            } while (x != 4);
        }

        private static List<String> ReturnDictionaryIDs(List<String> dictionariesPath)
        {
            List<String> dictionaries = new List<String>();

            foreach (var path in dictionariesPath)
            {
                XElement file = XElement.Load(path);
                var ns = file.GetDefaultNamespace();
                var elements = file.Elements(ns + "DictionaryEvent").Elements(ns + "Dictionary");

                foreach (var element in elements)
                {
                    dictionaries.Add(element.Attribute("ID").Value);
                }
            }

            return dictionaries;
        }

        private static List<String> ReturnUniqueDictionaryIDs(List<String> dictionariesPath)
        {
            List<String> dictionaries = ReturnDictionaryIDs(dictionariesPath);

            dictionaries = dictionaries.Distinct().OrderBy(e => e).ToList();

            return dictionaries;

        }

        private static void ReturnEntryIdDuplicates(List<String> dictionariesPath)
        {
            foreach (var path in dictionariesPath)
            {
                XElement file = XElement.Load(path);
                var ns = file.GetDefaultNamespace();
                var elements = file.Elements(ns + "DictionaryEvent").Elements(ns + "Dictionary");

                foreach (var element in elements)
                {
                    string fileName = duplicatesPath + "Dictionary" + element.Attribute("ID").Value + ".txt";

                    StreamWriter sw = File.AppendText(fileName);
                  
                    var entries = element.Elements(ns + "Events").Elements(ns + "Entry");

                    foreach (var entry in entries)
                    {
                        string entryID = entry.Attribute("ID").Value;
                        string entryClassificationID = String.Empty;
                        if (entry.HasElements)
                        {
                            entryClassificationID = entry.Element(ns + "Events").Attribute("ClassificationID").Value;
                        }

                        sw.WriteLine("EntryID={0}|ClassificationID={1}", entryID, entryClassificationID);
                    }

                    sw.Close();
                }
            }

            DirectoryInfo directory = new DirectoryInfo(duplicatesPath);
            foreach (var file in directory.GetFiles())
            {
                List<string> entriesList = File.ReadAllLines(file.FullName).ToList();
                Dictionary<string, string> dict = new Dictionary<string, string>();

                StreamWriter sw = File.CreateText(file.FullName);

                foreach (var item in entriesList)
                {
                    var entry = item.Split('|');
                    if (dict.Count == 0)
                    {
                        dict.Add(entry[0], entry[1]);
                    }
                    else if (dict.ContainsKey(entry[0]))
                    {
                        sw.WriteLine("{0} | {1}", entry[0], entry[1]);
                    }
                    else
                    {
                        dict.Add(entry[0], entry[1]);
                    }
                }

                sw.Close();
            }
        }
    }
}