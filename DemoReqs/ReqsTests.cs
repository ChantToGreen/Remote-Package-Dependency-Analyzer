/////////////////////////////////////////////////////////////////////
// ReqsTests.cs - Test classes for Project4 SMA                    //
// ver 1.0                                                         //
// Author: Yilin Cui, ycui21@syr.edu                               //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines several classes to demonstrate the requirement of project4
 */
/* Required Files:
 * -------------------
 * - Display.cs, FileMgr.cs,
 * - ReqsTests.cs, AutomatedTestClient.cs, Semi.cs, TestHarness.cs, Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 30 Nov 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MessagePassingComm;

namespace CodeAnalysis
{
    using Token = String;


    /////////////////////////////////////////////////////////////////
    // FileUtils class: provides facility to use files in code
    //

    class FileUtils
    {
        public static bool openFile(string fileSpec, out StreamReader sr)
        {
            sr = File.OpenText(fileSpec);
            return sr != null;
        }

        public static bool fileLines(string fileSpec, int start = 0, int end = 10000)
        {
            fileSpec = Path.GetFullPath(fileSpec);
            Console.Write("\n  file: \"{0}\"", fileSpec);
            StreamReader sr = null;
            try
            {
                sr = File.OpenText(fileSpec);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            int count = 0;
            string line;
            while (count < end)
            {
                line = sr.ReadLine();
                if (line == null)
                    return count > 0;
                if (++count > start)
                    Console.Write("\n  {0}", line);
            }
            return true;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // ReqDisplay class
    // - display methods for Requirements testing

    class ReqDisplay
    {
        public static void title(string tle)
        {
            Console.Write("\n  {0}", tle);
            Console.Write("\n {0}", new string('-', tle.Length + 2));
        }
        public static void message(string msg)
        {
            Console.Write("\n  {0}\n", msg);
        }
        public static void showSet(HashSet<string> set, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  Set:\n  ");
            foreach (var tok in set)
            {
                Console.Write("\"{0}\" ", tok);
            }
            Console.Write("\n");
        }

        public static void showList(List<string> lst, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  List:\n  ");
            int count = 0;
            foreach (var tok in lst)
            {
                Console.Write("\"{0}\" ", tok);
                if (++count == 10)
                {
                    count = 0;
                    Console.Write("\n  ");
                }
            }
            Console.Write("\n");
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Finder class
    // - finds semiExp with specified sequence of tokens in specified file

    class Finder
    {
        public static string file { get; set; } = "";

        public static bool findSequence(bool findAll, params string[] toks)
        {
            bool found = false;
            if (!File.Exists(file))
                return false;
            Lexer.Semi semi = new Lexer.Semi();
            Lexer.Toker toker = new Lexer.Toker();
            toker.open(file);
            semi.toker = toker;
            while (!semi.isDone())
            {
                semi.get();
                if (semi.hasSequence(toks))
                {
                    semi.show();
                    found = true;
                    if (findAll == false)
                        return true;
                }
            }
            return found;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestReq1 class
    class TestReq1 : ITest
    {
        public string name { get; set; } = "Test Req1";
        public string path { get; set; } = "..\\..\\..\\Parser";
        public bool result { get; set; } = true;
        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #1: Shall use Visual Studio 2017 and C# ");
            ReqDisplay.message("Show files in Parser directory");
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string fileName in files)
            {
                if (fileName.Substring(fileName.Length - 2, 2) == "cs")
                    Console.WriteLine("        {0}", fileName);
            }

            return result;
        }


    }

    ///////////////////////////////////////////////////////////////////
    // TestReq2 class
    class TestReq2 : ITest
    {
        public string name { get; set; } = "Test Req2";
        public string path { get; set; } = "..\\..\\..\\SemiExp\\Semi.cs";
        public bool result { get; set; } = true;
        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #2: Shall use the .Net System.IO and System.Text for all I/O");
            ReqDisplay.message("Showing using System.IO and System.Text in Semi.cs");
            StreamReader reader = new StreamReader(path);
            string line;
            int count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                count++;
                if (count >= 40 && count <= 45)
                {
                    Console.WriteLine("        {0}", line);
                }
                if (count > 45) break;

            }


            return result;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // TestReq3 class

    class TestReq3 : ITest
    {
        public string name { get; set; } = "Test Req3";
        public string path { get; set; } = "..\\..\\..\\";
        public bool result { get; set; } = true;

        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #3: Shall provide C# packages as described in the Purpose section");
            ReqDisplay.message("Showing code files used inside packages");
            string[] files2 = System.IO.Directory.GetFiles(path + "SemiExp", "*.cs");
            foreach (string file in files2)
            {
                Console.WriteLine(file);
            }
            string[] files3 = System.IO.Directory.GetFiles(path + "TypeAnalysis", "*.cs");
            foreach (string file in files3)
            {
                Console.WriteLine(file);
            }
            string[] files4 = System.IO.Directory.GetFiles(path + "DependencyAnalysis", "*.cs");
            foreach (string file in files4)
            {
                Console.WriteLine(file);
            }
            string[] files5 = System.IO.Directory.GetFiles(path + "SCC", "*.cs");
            foreach (string file in files5)
            {
                Console.WriteLine(file);
            }
            string[] files8 = System.IO.Directory.GetFiles(path + "MessagePassingCommService", "*.cs");
            foreach (string file in files8)
            {
                Console.WriteLine(file);
            }
            string[] files9 = System.IO.Directory.GetFiles(path + "Gui", "*.cs");
            foreach (string file in files9)
            {
                Console.WriteLine(file);
            }
            string[] files10 = System.IO.Directory.GetFiles(path + "Server", "*.cs");
            foreach (string file in files10)
            {
                Console.WriteLine(file);
            }
            return result;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestReq4 class

    class TestReq45 : ITest
    {
        public string name { get; set; } = "Test Req4 and 5";
        public bool result { get; set; } = true;
        public string port { get; set; }
        public TestReq45(string cmdPort)
        {
            port = cmdPort;
        }
        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #4 & #5:The Server packages shall conduct Dependency and Strong Connected Component analysis");
            ReqDisplay.message("Send request for dependency analysis from auto client and display result on GUI client");
            ReqDisplay.message("-------------------------------------------------------------");
            AutoClient autoClient = new AutoClient(port);
            autoClient.sendDep();
            ReqDisplay.message("Send request for SCC analysis from auto client and display result on GUI client");
            ReqDisplay.message("-------------------------------------------------------------");
            autoClient.sendScc();
            autoClient.shutDown();
            
            return result;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestReq6 class

    class TestReq6 : ITest
    {
        public string name { get; set; } = "Test Req6";
        public bool result { get; set; } = true;
        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #6: The Client packages shall display requested results in a well formated GUI display");
            ReqDisplay.message("The result will be displayed on GUI client");
            return result;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestReq7 class

    class TestReq7 : ITest
    {
        public string name { get; set; } = "Test Req7";
        public bool result { get; set; } = true;
        public bool doTest()
        {
            ReqDisplay.message("=============================================================");
            ReqDisplay.title("Requirement #7: Shall include an automated unit test suite");
            ReqDisplay.message("All above are automated unit test suites\n\n");
            ReqDisplay.message("Notice: If you click the Shut Down Server button,\n  please wait 10 seconds for server to close,\n  I have no clue why it takes this long");
            return result;
        }
    }


}
