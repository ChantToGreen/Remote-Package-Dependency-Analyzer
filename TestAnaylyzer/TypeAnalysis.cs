///////////////////////////////////////////////////////////////////////
// TypeAnalysis.cs - extract and store type information from files   //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Project #3                                   //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - TypeAnalysis.cs  package provides service for type information extraction and storage
 *   uses parser and RulesAnd Actions to extract type information
 *   uses TypeTable to store that information for further dependency analysis 
 *
 * Required files:
 * - Display.cs, Element.cs, Parser.cs
 * - SemiExp.cs, Toker.cs
 * - TypeTable.cs
 * 
 * Note:
 * - None
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 10/18/2018
 * - first release
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lexer;
using TypeFile;
using CodeAnalysis;

namespace TypeAnalysis
{
    ///////////////////////////////////////////////////////////////////////////////////
    // TypeAnalyzer class, collect and store type information from collection of files
    //
    
    public class TypeAnalyzer
    {
     
        private List<string> files_ = null;

        //-----------------<Constructor>-------------------
        public TypeAnalyzer()
        {
            files_ = new List<string>();
        }

        //----------------<Add files from specified path to the collection>--------------
        public void AddFileColeection(string path)// use FileMgr
        {
            string[] files = System.IO.Directory.GetFiles(path,"*.cs",SearchOption.AllDirectories);
            foreach (string file in files)
                files_.Add(file);
        }

        //----------------<clear the path in the file name>------------------
        public static string Filename(string file)
        {
            string name = "";
            for (int i = file.Length - 1; file[i] != '/' && file[i] != '\\'; --i)
            {
                name = file[i] + name;
            }
            return name;
        }

        //---------------<Exetract type information from collection of files>------------
        public void AnalyzeFiles()
        {
            foreach(string filepath in files_)
            {
                
                Semi semi = new Semi();
                if (!semi.open(filepath))
                {
                    Console.WriteLine("Cannot open file {0}", filepath);
                    continue;
                }
                string filename = Filename(filepath);
                
                BuildTypeAnalyzer typeAnalysis = new BuildTypeAnalyzer(semi, filename);
                Parser typeParser = typeAnalysis.build();
                try
                {
                    while (semi.get().Count > 0)
                        typeParser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }

            }
            Repository repo = Repository.getInstance();
            repo.MapAlias();

        }

       
#if TEST_TYPEANALYSIS
        static void Main()
        {
            Console.WriteLine("Collecting type information on a collection of files");
            TypeAnalyzer analyzer= new TypeAnalyzer();
            analyzer.AddFileColeection("../../../TestTypeAnaylyzer");
            analyzer.AnalyzeFiles();
            Repository repo = Repository.getInstance();
            Console.WriteLine("-------------------------------------------\nType table collected: ");
            Console.WriteLine("-------------------------------------------");
            repo.printTypeTable();
            Console.WriteLine("-------------------------------------------\nNamespace collected: ");
            Console.WriteLine("-------------------------------------------");
            repo.printNameSpace();
            Console.ReadLine();
        }
#endif

    }
}