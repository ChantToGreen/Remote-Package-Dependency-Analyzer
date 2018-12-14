///////////////////////////////////////////////////////////////////////
// TypeAnalysis.cs - extract and store type information from files   //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Dependency Analyzer                   //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - TypeAnalysis.cs  package provides service for type information extraction and storage
 *   uses parser and RulesAnd Actions to extract type information
 *   uses TypeTable to store that information for further dependency analysis 
 *   it contains one classes:
 *      TypeAnalyzer use the rules defined to collect type information and deposit in repository
 *
 * Required files:
 * - Parser.cs, TypeAnalysis
 * - TypeTable.cs 
 * - RulesAndActions.cs, SemiExp.cs
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
        public void AddFileCollection(string path)// use FileMgr
        {
            string[] files = System.IO.Directory.GetFiles(path,"*.cs",SearchOption.AllDirectories);
            foreach (string file in files)
            {
                files_.Add(file);
            }
        }

        //----------------<clear the path in the file name,but keep some part to distinguish from file with same name>------------------
        public string Filename(string file)
        {
            int j = file.Length - 1;
            while (file[j] != '\\' && file[j] != '/')
            {
                --j;
            }
            return file.Substring(j + 1);
        }

        //---------------<Exetract type information from collection of files>------------
        public void AnalyzeFiles()
        {
            foreach(string filepath in files_)
            {
                
                ITokenCollection semi = Factory.create();
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
            Console.WriteLine("Demonstration in DependencyExecutive");
            Console.ReadLine();
        }
#endif

    }
}