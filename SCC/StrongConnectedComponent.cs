/////////////////////////////////////////////////////////////////////
// StrongConnectedComponent.cs                                     //
// - Extract the SCC from the repository                           //
// Author: Yilin Cui, ycui21@syr.edu                               //
// Environment: VS2017, Win10, Surface Pro M3                      //                                                                //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains a SCCConstructor class that construct 
 *   strong connected component ot of the information collected in 
 *   repository, and stores SCC in its' data member
 * 
 * 
 * Required Files:
 * ---------------
 * RulesAndActions.cs, parser.cs, TypeTable,cs, DependencyAnalysis.cs, StrongConnectedComponent.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 29 Oct 2018
 * - first release
 * 
 * Note:
 * - Demonstration in DependencyExecutive
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalysis;

namespace SCC
{
   
    
    /////////////////////////////////////////////////////////////////
    // SCCConstructor class: construct the SCC from repository
    //
    
    public class SCCConstructor
    {
        private Dictionary<string, List<string>> depTable_ = null;
        //Below is the helper data structure for Tarjan Algorishm
        private Stack<string> low_ = null;
        private Dictionary<string, int> lowLink_ = null;
        private Dictionary<string, bool> inStack_ = null;
        private Dictionary<string, int> index_ = null;
        private List<List<string>> SCC_;
        private List<string> visited_;
        private int CurrIndex_ = 0;

        //----------------------<Constructor>------------------
        public SCCConstructor()
        {
            depTable_ = new Dictionary<string, List<string>>();
            low_ = new Stack<string>();
            lowLink_ = new Dictionary<string, int>();
            inStack_ = new Dictionary<string, bool>();
            SCC_ = new List<List<string>>();
            visited_ = new List<string>();
            index_ = new Dictionary<string, int>();
        }

        //-------------------<Construct the dependency table out of the graph in repository>----------------
        public void ConstructDep()
        {
            Repository repo = Repository.getInstance();
            repo.Setoperation(ref depTable_);
            repo.WalkTheGraph();
        }

        

        //----------------<Produce the SCC based on a file>----------
        public void StrongConnect(string file)
        {
            if (visited_.Contains(file)) return;
            low_.Push(file);
            lowLink_.Add(file, CurrIndex_);
            index_.Add(file, CurrIndex_);
            ++CurrIndex_;
            inStack_.Add(file, true);
            visited_.Add(file);
            foreach(string child in depTable_[file])
            {
                if (!visited_.Contains(child))
                {
                    StrongConnect(child);
                    lowLink_[file] = Math.Min(lowLink_[file], lowLink_[child]);
                }
                else if (inStack_[child])
                    lowLink_[file] = Math.Min(lowLink_[file], index_[child]);
            }
            if(lowLink_[file]==index_[file])
            {
                List<string> currentScc = new List<string>();
                string temp = low_.Peek();
                do
                {
                    temp = low_.Pop();
                    inStack_[temp] = false;
                    currentScc.Add(temp);
                }
                while (temp != file);
                SCC_.Add(currentScc);
            }

        }

        //------------------------<Create entire SCC>-------------------
        public void CreateSCC()
        {
            SCC_.Clear();
            Dictionary<string, List<string>> tab = depTable();
            foreach (KeyValuePair<string,List<string>>pair in tab)
            {
                StrongConnect(pair.Key);
            }
            lowLink_.Clear();
            inStack_.Clear();
            index_.Clear();
            visited_.Clear();
            CurrIndex_ = 0;

        }

        //-----------------<Return the dependency table to console>---------------
        public Dictionary<string,List<string>> depTable()
        {
            return depTable_;
        }
        
        //---------------<Print the dependency table to console>------------------
        public void printTable()
        {
            foreach(KeyValuePair<string,List<string>> pair in depTable_)
            {
                Console.WriteLine("File: {0}\nChildren:", pair.Key);
                if (pair.Value.Count > 0)
                {
                    foreach (string child in pair.Value)
                        Console.Write("{0}, ", child);
                    Console.Write("\n==============================\n");
                }
                else
                    Console.WriteLine("This file has no children\n===========================");
                
            }
        }

       

        //-------------------<Print the SCC>------------------
        public void printScc()
        {
            foreach (List<string> component in SCC_)
            {
                Console.Write("Strong Connected Component :");
                foreach (string file in component)
                    Console.Write(" {0},", file);
                Console.Write("\n");
            }
            
        }

        //----------------<Return the SCC>-------------------------
        public List<List<string>> getSCC()
        {
            return SCC_;
        }
#if TEST_SCC
        static void Main()
        {
            Console.WriteLine("Demonstration in DependencyExecutive");
            Console.ReadLine();

        }
#endif
        


    }
}

