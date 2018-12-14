///////////////////////////////////////////////////////////////////////
// DependencyExecutive.cs - Executive for depedency analysis         //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Dependency Analyzer                   //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - DependencyExecutive.cs  use TypeAnalyzer, DepAnalyzer, SCCConstructor to perform depedency analysis
 *   it contains one classes:
 *      DependencyExecutive tp produce  dependency information based on the information collected by worker classes
 *      uses Display.cs to display dependency information
 *
 * Required files:
 * - RulesAndActions.cs
 * - TypeTable.cs 
 * - DependencyAnalysis.cs
 * - StrongConnectedComponent.cs
 * - Display.cs, TypeAnalysis.cs
 * - Parser.cs
 * 
 * Note:
 * - None
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 10/29/2018
 * - First release
 * ver 1.1 : 11/23/2018
 * - Added function to print the result to file
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAnalysis;
using DependencyAnalysis;
using TypeFile;
using CodeAnalysis;
using SCC;
namespace DepExe
{
    ///////////////////////////////////////////////////////////////////
    // DependencyExecutive class produce the file depedency information
    //
    public class DependencyExecutive
    {
        private List<List<string>> SCC_=null;
        private TypeTable typetable_ = null;
        private Repository repo_ = null;
        private Dictionary<string, List<string>> depTable_;
        static public Action<string> DisplayActions;
        string path_ = "../../../TestTypeAnaylyzer";
       
        //---------------<Get Repository>------------
        public Repository repository()
        {
            return repo_;
        }

        //---------------<Get dependency table>-----------
        public Dictionary<string, List<string>> depTable()
        {
            return depTable_;
        }

        //---------------<Constructor>---------------
        public DependencyExecutive()
        {
            SCC_ = new List<List<string>>();
            typetable_ = new TypeTable();
            repo_ = Repository.getInstance();
            depTable_ = new Dictionary<string, List<string>>();
        }

        //-------------<Set the path for analysis>----------
        public bool setPath(string path)
        {
            if (!System.IO.Directory.Exists(path))
                return false;
            path_ = path;
            return true;
        }
       
        //-------------<Collect type information>-----------
        public void CollectTypeInfo()
        {
            TypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            typeAnalyzer.AddFileCollection(path_);
            typeAnalyzer.AnalyzeFiles();
            repo_ = Repository.getInstance();
            typetable_ = repo_.typeTable;
        }

        //--------------<Collect dependency information>-----
        public void CollectDependencyInfo()
        {
            DepAnalyzer depdencyAnalyzer = new DepAnalyzer();
            depdencyAnalyzer.ConstructDependency(path_);
            repo_ = Repository.getInstance();
            
            
        }

        //------------<Create SCC based on the inforamtion collected>----------
        public void ConstructSCC()
        {
            SCCConstructor worker = new SCCConstructor();
            worker.ConstructDep();
            worker.CreateSCC();
            depTable_ = worker.depTable();
            SCC_ = worker.getSCC();
        }

        //-----------<Pass SCC to outer applications>--------------
        public List<List<string>> SCC()
        {
            return SCC_;
        }

        //---------<Print the type table to console, for demonstration purpose, note: not use the service of Display class>----------
        public void printTypeTable()
        {
            typetable_.print();
        }

        //-------------<Use Display class to print the dependency table>------------------
        public void printDepTable()
        {
            Display.useConsole = true;
            if (depTable_.Count == 0)
            {
                Display.displayString("No Dpendency information has been produced yet");
                return;
            }
            Display.displayDepTable(DisplayActions, depTable_);
            Display.useConsole = false;
        }

        //-------------<Use Display to print SCC>---------------
        public void printSCC()
        {
            Display.useConsole = true;
            if (SCC_.Count == 0)
            {
                Display.displayString("No SCC has been produced yet");
                return;
            }
            Display.displaySCC(DisplayActions, SCC_);
            Display.useConsole = false;
        }

        //------------------------<Print type table to text file>-------------------
        public void typeTableToTxt(string specPath,string req)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(specPath);
            sw.WriteLine("Type information result for {0}\n", req);
            sw.WriteLine("Directory root: {0}\n", path_);
            Dictionary<string, List<TypeInfo>> table = typetable_.table();
            foreach(KeyValuePair<string ,List<TypeInfo>> pair in table)
            {
                sw.WriteLine("\nType name: {0}", pair.Key);
                foreach (TypeInfo ti in pair.Value)
                {
                    sw.Write("Name space: {0}", ti.nameSpace);
                    sw.Write("   File: {0}\n", ti.file);
                        
                }
                sw.WriteLine("===============================================================================\n");


            }
            sw.Close();
        }

        //------------------<Print dependency table to test file>-------------
        public void depTableToText(string specPath, string req)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(specPath);
            sw.WriteLine("Dependency information result for {0}", req);
            sw.WriteLine("Directory root: {0}\n", path_);
            foreach (KeyValuePair<string,List<string>>pair in depTable_)
            {
                sw.WriteLine("File: {0}, Children: ", pair.Key);
                if (pair.Value.Count != 0)
                {
                    foreach (string file in pair.Value)
                    {
                        sw.Write(" {0}, ", file);
                    }
                    sw.Write("\n");
                }
                else
                {
                    sw.WriteLine("This package has no children");
                }
                sw.WriteLine("==================================================================================\n");
            }
            sw.Close();

        }

        //------------------<Print SCC to text file>---------------
        public void SCCToText(string specPath, string req)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(specPath);
            sw.WriteLine("Strong connected component result for {0}\n", req);
            sw.WriteLine("Directory root: {0}\n",path_);
            sw.WriteLine("\n");
            int num = 1;
            foreach(List<string> lt in SCC_)
            {
                sw.Write("SCC #{0}:", num);
                ++num;
                foreach(string file in lt)
                {
                    sw.Write(" {0},", file);
                }
                sw.WriteLine("\n===============================================================================\n");
            }
            sw.Close();
        }
#if Test_EXECUTIVE
        static void Main(string[] args)
        {
            DependencyExecutive dep = new DependencyExecutive();
            dep.CollectTypeInfo();
            dep.CollectDependencyInfo();
            dep.ConstructSCC();
            Console.WriteLine("\n============================\nPrinting type table collected\n============================\n");
            dep.typeTableToTxt("typeTest.txt", "req1");
            dep.printTypeTable();
            Console.WriteLine("\n============================\nPrinting dependency table collected\n============================\n");
            dep.printDepTable();
            dep.depTableToText("depTest.txt", "req2");
            Console.WriteLine("\n============================\nPrinting SCC collected\n============================\n");
            dep.printSCC();
            dep.SCCToText("SCCtest.txt", "req3");
            Console.ReadLine();
        }
#endif
    }

}