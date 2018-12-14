///////////////////////////////////////////////////////////////////////
// SCCanalyzer.cs - Process to produce SCC result                    //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains SCCanlyzer class just used to produce strong connected component for
 *   a collection of files. this package is used by Server.
 *
 * Required files:
 * - DependencyExecutive.cs
 * - SCCanalyzer.cs
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
    //////////////////////////////////////////////////////////////////
    // SCCanalyzer class, provides functions to produce SCC result
    // used by Server.
    //
    class SCCanalyzer
    {
        private string resultPath_ = "../../../ServerResult/";
        private string dirPath_;
        private string req_;
        private DependencyExecutive depExe;

        //---------------------<Constructor>--------------------
        public SCCanalyzer(string cmd)
        {
            depExe = new DependencyExecutive();
            parseCMD(cmd);

        }

        //------------------<Parse the command line>-------------
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
            Console.WriteLine(dirPath_);
            depExe.setPath(dirPath_);

        }

        //-----------------<Get the SCC analysis result>----------
        public void getSCC()
        {
            depExe.CollectTypeInfo();
            depExe.CollectDependencyInfo();
            depExe.ConstructSCC();
            depExe.SCCToText(resultPath_, req_);
            depExe.printSCC();
        }
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0) return;
                SCCanalyzer SccAn = new SCCanalyzer(args[0]);
                SccAn.getSCC();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
        }
    }
}
