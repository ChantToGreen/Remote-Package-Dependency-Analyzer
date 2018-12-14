///////////////////////////////////////////////////////////////////////
// DependencyAnalyzer.cs - Executive package for DepAnalysis         //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains TypeAnalyzer class just used to produce dependency information for
 *   a collection of files. this package is used by Server.
 *
 * Required files:
 * ------------------
 * - DependencyExecutive.cs
 * - DependencyAnalyzer.cs
 * 
 * Note:
 * - None
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 11/23/2018
 * - First release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepExe
{

    ///////////////////////////////////////////////////////////////////////
    //DepAnalyzer class, provides functions to produce dependency analysis result
    //used by server

    class DepAnalyzer
    {
        private string resultPath_ = "../../../ServerResult/";
        private string dirPath_;
        private string req_;
        private DependencyExecutive depExe;

        //---------------<Constructor>----------------
        public DepAnalyzer(string cmd)
        {
            depExe = new DependencyExecutive();
            parseCMD(cmd);
        }

        //---------------<Parse the command line>-----------
        public void parseCMD(string cmd)
        {
            int index = 0;
            while (index < cmd.Length)
            {
                if (cmd[index] == '-')
                    break;
                index++;
            }
            req_ = cmd.Substring(0, index);
            resultPath_ = resultPath_ + req_ + ".txt";
            dirPath_ = cmd.Substring(index + 1);
            depExe.setPath(dirPath_);

        }

        //--------------<Print the dependency information to table>-------------
        public void getDepTable()
        {
            depExe.CollectTypeInfo();
            depExe.CollectDependencyInfo();
            depExe.ConstructSCC();
            depExe.depTableToText(resultPath_, req_);
            depExe.printDepTable();
        }
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0) return;
                DepAnalyzer depAn = new DepAnalyzer(args[0]);             
                depAn.getDepTable();
               
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
        }
    }
}
