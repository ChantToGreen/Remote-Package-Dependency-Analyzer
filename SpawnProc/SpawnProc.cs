/////////////////////////////////////////////////////////////////////
// SpawnProc - demonstrate creation of multiple .net processes     //
// ver 2.0                                                         //
// Environment: WIN10, VS2017, Surface Pro M3                      //
// Author: Yilin Cui, ycui21@syr.edu                               //
// Origin: Dr. Jim Fawcett's website                               //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Instances of class SpawnProc start process by loading and executing
 * a specified application.  It provides a createProcess method that
 * accepts a fileSpecification, and an exit handler called childExited.
 * 
 * Required files:
 * ---------------
 * SpawnProc.cs
 * Application(s) to start
 * 
 * Maintenance History:
 * --------------------
 * ver 2.0 : 06 Nov 2018
 * - added exit event handler
 * ver 1.0 : 19 Oct 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SpawnProc
{
    class SpawnProc
    {
        List<Process> childProcs = new List<Process>();


        void childExited(object sender, System.EventArgs e)
        {
            Console.Write("\n  child process exited");
        }

        bool createProcess(string fileName, string commandline)
        {
            Process proc = new Process();
            childProcs.Add(proc);

            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = commandline;
            proc.EnableRaisingEvents = true;
            //proc.Exited += new EventHandler(childExited);
            proc.Exited += childExited;

            Console.Write("\n  attempting to start {0}", fileName);
            try
            {
                proc.Start();
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }
#if (TEST_PROC)
        static void Main(string[] args)
    {
      Console.Title = "SpawnProc";
      Console.BackgroundColor = ConsoleColor.Black;
      Console.ForegroundColor = ConsoleColor.White;

      Console.Write("\n  Demo Parent Process");
      Console.Write("\n =====================");

      SpawnProc sp = new SpawnProc();

      string fileName = "..\\..\\..\\DependencyExecutive\\bin\\debug\\DependencyExecutive.exe";
      string absFileSpec = Path.GetFullPath(fileName);

      if (args.Count() == 0)
      {
        Console.Write("\n  please enter number of processes to create on command line");
        return;
      }
      else
      {
        
          if(sp.createProcess(absFileSpec, "../../../TestTypeAnaylyzer"))
          {
            Console.Write(" - succeeded");
          }
          else
          {
           Console.Write(" - failed");
          }
        
      }
      Console.Write("\n  Press key to exit");
      Console.ReadKey();
      Console.Write("\n  ");
    }
#endif
    }

}
