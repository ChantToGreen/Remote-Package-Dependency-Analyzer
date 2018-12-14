///////////////////////////////////////////////////////////////////////
// DemoReqs.cs - Demonstrate Project #4 Requirements                 //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Environment: VS2017, WIN10, Surface Pro M3                        //
// Application: Demonstration for CSE681, Project #4, Fall 2018      //
// Author:      Yilin Cui, ycui21@syr.edu                            //
// Origin:      Dr. Jim Fawcett's website                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   Executive:
 *   - used to demonstrate the requirement on Remote Package Dependency Analyzer 
 */
/* Required Files:
 * -------------------
 * - Display.cs, FileMgr.cs,
 * - ReqsTests.cs, AutomatedTestClient.cs, Semi.cs, TestHarness.cs, Toker.cs
 * - DemoReqs.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 12/4 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace CodeAnalysis
{
  class Executive
  {
    //----< process commandline to get file references >-----------------

    static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length < 2)
      {
        Console.Write("\n  Please enter path and file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      if (!Directory.Exists(path))
      {
        Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
        return files;
      }
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
    }

    bool testToker()
    {
      return false;
    }

    static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n");
    }

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Project #4 Requirements");
      Console.Write("\n =======================================\n");

      TestHarness.Tester tester = new TestHarness.Tester();

            TestReq1 tr1 = new TestReq1();
            tester.add(tr1);
            TestReq2 tr2 = new TestReq2();
            tester.add(tr2);
            TestReq3 tr3 = new TestReq3();
            tester.add(tr3);
            TestReq45 tr45 = new TestReq45(args[0]);
            tester.add(tr45);
            TestReq6 tr6 = new TestReq6();
            tester.add(tr6);
            TestReq7 tr7 = new TestReq7();
            tester.add(tr7);
            tester.execute();

           
      Console.Write("\n\n");
            Console.ReadLine();
        }
  }
}
