///////////////////////////////////////////////////////////////////////////
// Display.cs  -  Manage Display properties                              //
// ver 1.0                                                               //
// Language:    C#                                                       //
// Environment: VS2017, WIN10, Surface Pro M3                            //
// Author:      Yilin Cui, ycui21@syr.edu                                //
// Origin:      Dr. Jim Fawcett's web site                               //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * - Display manages static public properties used to control what is displayed and
 * provides static helper functions to send information to MainWindow and Console.
 * - Provides display function to dispaly data structure in the DependencyExecutive
 * 
 */
/*
 * Required Files:
 *   Display.cs
 *   
 * Note:
 *  - none
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 29 Oct 2018
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CodeAnalysis
{
  ///////////////////////////////////////////////////////////////////
  // StringExt static class
  // - extension method to truncate strings

  public static class StringExt
  {
    public static string Truncate(this string value, int maxLength)
    {
      if (string.IsNullOrEmpty(value)) return value;
      return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
  }

  static public class Display
  {
    static Display()
    {
      showFiles = true;
      showDirectories = true;
      showActions = false;
      showRules = false;
      useFooter = false;
      useConsole = false;
      goSlow = false;
      width = 33;
    }
    static public bool showFiles { get; set; }
    static public bool showDirectories { get; set; }
    static public bool showActions { get; set; }
    static public bool showRules { get; set; }
    static public bool showSemi { get; set; }
    static public bool useFooter { get; set; }
    static public bool useConsole { get; set; }
    static public bool goSlow { get; set; }
    static public int width { get; set; }

    //----< display results of Code Analysis >-----------------------

    static public void showMetricsTable(List<Elem> table)
    {
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "category", "name", "bLine", "eLine", "size", "cmplx"
      );
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "--------", "----", "-----", "-----", "----", "-----"
      );
      foreach (Elem e in table)
      {
        /////////////////////////////////////////////////////////
        // Uncomment to leave a space before each defined type
        // if (e.type == "class" || e.type == "struct")
        //   Console.Write("\n");

        Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          e.type, e.name, e.beginLine, e.endLine,
          e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1
        );
      }
    }
    //----< display a semiexpression on Console >--------------------

    static public void displaySemiString(string semi)
    {
      if (showSemi && useConsole)
      {
        Console.Write("\n");
        System.Text.StringBuilder sb = new StringBuilder();
        for (int i = 0; i < semi.Length; ++i)
          if (!semi[i].Equals('\n'))
            sb.Append(semi[i]);
        Console.Write("\n  {0}", sb.ToString());
      }
    }
    //----< display, possibly truncated, string >--------------------

    static public void displayString(Action<string> act, string str)
    {
      if (goSlow) Thread.Sleep(200);  //  here only to support visualization
      if (act != null && useFooter)
        act.Invoke(str.Truncate(width));
      if (useConsole)
        Console.Write("\n  {0}", str);
    }
    //----< display string, possibly overriding client pref >--------

    static public void displayString(string str, bool force=false)
    {
      if (useConsole || force)
        Console.Write("\n  {0}", str);
    }
    //----< display rules messages >---------------------------------

    static public void displayRules(Action<string> act, string msg)
    {
      if (showRules)
      {
        displayString(act, msg);
      }
    }
    //----< display actions messages >-------------------------------

    static public void displayActions(Action<string> act, string msg)
    {
      if (showActions)
      {
        displayString(act, msg);
      }
    }
    //----< display filename >---------------------------------------

    static public void displayFiles(Action<string> act, string file)
    {
      if (showFiles)
      {
        displayString(act, file);
      }
    }
    //----< display directory >--------------------------------------

    static public void displayDirectory(Action<string> act, string file)
    {
      if (showDirectories)
      {
        displayString(act, file);
      }
    }
        //--------------<Display list of string>--------------
        static public void displayList(Action<string> act, List<string> list)
        {
           
            foreach (string item in list)
            {
                if (useFooter || act != null)
                    act.Invoke(item);
                if (useConsole)
                    Console.Write(" {0},", item);
            }
        }

        //--------------<Designed to display the dependency table in DependencyExecutive, also can be used to display similar data structure>----------
        static public void displayDepTable(Action<string> act, Dictionary<string,List<string>>dic)
        {
            foreach (KeyValuePair<string,List<string>>pair in dic)
            {

                if (useFooter || act != null)
                    act.Invoke(pair.Key);
                if (useConsole)
                    Console.Write("\nFile name:{0}\nChildren:\n", pair.Key);
                if (pair.Value.Count!=0)
                    displayList(act, pair.Value);
                else
                {
                    if (useConsole)
                        Console.Write("This file has no children");
                }
                if (useConsole)
                    Console.Write("\n-----------------------------------------");
            }
        }


        //--------------<Designed to display SCC in DependencyExecutive, also can be used to display similar data structure>----------
        static public void displaySCC(Action<string> act, List<List<string>> SCC)
        {
            int num = 1;
            foreach(var list in SCC)
            {
                if (useConsole)
                    Console.Write("\nSCC part #{0,-5} ",num.ToString()+":");
                displayList(act, list);
                ++num;
  
            }
           

        }

#if(TEST_DISPLAY)
    static void Main(string[] args)
    {
      Console.Write("\n  Tested by use in DependencyExecutive\n\n");
    }
#endif
  }
}
