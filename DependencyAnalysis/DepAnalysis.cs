///////////////////////////////////////////////////////////////////////
// DependencyAnalysis.cs - construct depedency information           //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: Remote Depdency Analyzer                             //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - DependencyAnalysis.cs  package provides service for depedency inforamtion construction
 *   uses parser and RulesAnd Actions to establish dependency
 *   deposit dependency information in the Repository's graph 
 *   it contains one classes:
 *      DepAnalysis use the rules defined to construct depedency 
 *
 * Required files:
 * - Parser.cs, Display.cs, Toker.cs
 * - TypeTable.cs, Element.cs TypeAnalysis.cs
 * - RulesAndActions.cs, SemiExp.cs, TypeTable.cs
 * - DepAnalysis.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalysis;
using Lexer;


namespace DependencyAnalysis
{

    /////////////////////////////////////////////////////////
    //  DepAnalysis analyze the dependency in set of files
    //
    public class DepAnalyzer 
    {
        private List<string> files_ = null;
        public DepAnalyzer()
        {
            files_ = new List<string>();
        }

        //----------------------<Add files from specified directory>----------------
        public void AddFile(string path)
        {
            string[] files = System.IO.Directory.GetFiles(path, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach(string file in files)
                files_.Add(file);
        }

        //----------------------<Extract file name from a relative path string>--------
        public string Filename(string file)
        {
            int j= file.Length-1;
            while(file[j]!='\\'&&file[j]!='/')
            {
                --j;
            }
            return file.Substring(j+1);
        }

        //-----------------------<Construct dependency information and deposit in repository>-----------
        public void ConstructDependency(string path)
        {
            AddFile(path);
            foreach(string filepath in files_)
            {
                ITokenCollection semi = Factory.create();
                if (!semi.open(filepath))
                {
                    Console.WriteLine("Cannot open file {0}", filepath);
                    continue;
                }
                string filename = Filename(filepath);
                Console.WriteLine("Constructed filename: {0}", filename);
                BuildDependencyAnalyzer depAnalyzer = new BuildDependencyAnalyzer(semi,filename);
                Parser depParser = depAnalyzer.build();
                try
                {
                    while (semi.get().Count > 0)
                        depParser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
            }

        }
#if TEST_DEPENDENCY
        static void Main()
        {
            Console.WriteLine("Demonstration in DependencyExecutive");
            Console.ReadKey();
        }
#endif
    }
}
