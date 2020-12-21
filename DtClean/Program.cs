using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DtClean
{
    class Program
    {
        private static Dictionary<String,String[]> conf;

        private static string EMPTY_S = "";
        private static string desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        static void Main(string[] args)
        {
            Console.Out.Write("Let\'s clean "+desk);
            initConfig();
            prepDir();
            moveFiles();
            Console.WriteLine("\n\nPress enter to exit...");
            Console.Read();
        }

        /**
         * Loads values from App.config to our Dictionary named conf
         */
        private static void initConfig()
        {
            conf = new Dictionary<string, string[]>();
            foreach (string key in ConfigurationManager.AppSettings)
            {
                string[] vals = ConfigurationManager.AppSettings[key].Split(';');
                conf.Add(key, vals);
            }
        }

        /**
         * Itereates over loaded config Keys (desired folder) and moves all desktop files
         * with corresponding file ending (value is string[] of desired endings) to folder
         */
        private static void moveFiles()
        {
            Console.WriteLine("\n\n----- Searching for and moving Files -----");
            int errs = 0;
            LinkedList<string> errFiles = new LinkedList<string>();
            foreach (string key in conf.Keys)
            {
                string[] vals;
                conf.TryGetValue(key, out vals);
                Console.Write("\nSearching for ");
                vals.ToList().ForEach(i => Console.Write(i.ToString() + " "));
                Console.WriteLine(":");
                LinkedList<string> filePaths = new LinkedList<string>();

                foreach (string val in vals)
                {
                    if(val == EMPTY_S) continue; // crucify me
                    var buffer = Directory.GetFiles(desk, "*." + val).
                        Where(s =>s.EndsWith("."+val.ToUpper()) || s.EndsWith("." + val.ToLower()));
                    if (buffer.ToArray<string>().Length > 0)
                        buffer.ToList().ForEach(i =>
                        {
                            filePaths.AddLast(i);
                            Console.Write(i.Replace(desk + "\\", "") + " ");
                        });
                }
                Console.WriteLine("\nFiles found: " + filePaths.Count);

                filePaths.ToList().ForEach(i =>
                {
                    try
                    {
                        string safeMovePath = generateSafePath(i.Replace(desk + "\\", desk + "\\" + key +"\\"));
                        File.Move(i, safeMovePath);
                    } catch (Exception e)
                    {
                        errs++;
                        errFiles.AddLast(i.Replace(desk + "\\", ""));
                    }
                });
            }

            Console.WriteLine("\n\n------------------------------------------");
            Console.WriteLine("Error count: " + errs);
            if (errs > 0)
            {
                Console.WriteLine("Couldn't move following files:");
                errFiles.ToList().ForEach(i => Console.Write(i.ToString() + " "));
            }
        }

        /**
         * From stackoverflow https://stackoverflow.com/questions/13049732/automatically-rename-a-file-if-it-already-exists-in-windows-way
         * get renamed path if file already exists in folder we want to move to
         */
        private static string generateSafePath(string s)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(s);
            string extension = Path.GetExtension(s);
            string path = Path.GetDirectoryName(s);
            string newFullPath = s;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        /**
         * Directory nonexistent? create it
         */
        private static void prepDir()
        {
            foreach(string key in conf.Keys)
            {
                string dir = desk + "\\" + key;
                bool exists = System.IO.Directory.Exists(dir);
                if (!exists)
                    System.IO.Directory.CreateDirectory(dir);
            }
        }
    }
}
