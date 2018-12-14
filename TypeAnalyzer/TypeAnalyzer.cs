///////////////////////////////////////////////////////////////////////
// TypeAnalyzer.cs - Executive package for TypeAnalysis              //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains TypeAnalyzer class just used to produce type information for
 *   a collection of files. this package is used by Server.
 *
 * Required files:
 * - DependencyExecutive.cs
 * - TypeAnalyzer.cs
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
    ///////////////////////////////////////////////////////////////////////////
    // TypeAnalyzer class, provides functions to produce type analysis result
    // used by Server.
    //
    class TypeAnalyzer
    {
        private string resultPath_="../../../ServerResult/";
        private string dirPath_;
        private string req_;
        private DependencyExecutive depExe;

        //-----------------<Constructor>---------------
        public TypeAnalyzer(string cmd)
        {
            depExe = new DependencyExecutive();
            parseCMD(cmd);
        }

        //-----------------<Parse comandline into useful arguments>------------
        public void parseCMD(string cmd)
        {
            int index = 0;
            while(index<cmd.Length)
            {
                if (cmd[index] == '-')
                    break;
                index++;
            }
            req_ = cmd.Substring(0, index);
            resultPath_ = resultPath_+req_+".txt";
            dirPath_ = cmd.Substring(index + 1);
            depExe.setPath(dirPath_);

        }

        //-----------------<Print type table to file>--------------------
        public void getTypeTable()
        {
            depExe.CollectTypeInfo();
            int index = resultPath_.IndexOf('.');
            depExe.typeTableToTxt(resultPath_, req_);
            depExe.printTypeTable();
        }

        static void Main(string[] args)
        {

            try
            {
                if (args.Length == 0) return;
                TypeAnalyzer typeAn = new TypeAnalyzer(args[0]);
                typeAn.getTypeTable();


            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }


}
    }
}
