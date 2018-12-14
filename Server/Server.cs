///////////////////////////////////////////////////////////////////////
// Server.cs - Server for  Package Dependency Analysis               //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains Server class to provide function for Remote Package Dependency Analysis server side.
 *   It will accept the request message from client, handle the request by spawn an analyze process  and send back result or send back the
 *   file and directory inforamtion.
 *
 * Required files:
 * - MPCommService.cs
 * - SpawnProcs.cs
 * - Server.cs
 * - BlockingQueue.cs
 * - IMPCommService.cs
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SWTools;
using System.Threading;
using MessagePassingComm;
using SpawnProc;

namespace Analyzer
{
    using Msg = CommMessage;

    /////////////////////////////////////////////////////////////////////////////////
    // Server class, provides server functions for Remote Package Dependency Analysis
    // use Message passing communication to accept request and send back reply
    //
    public class Server
    {
        private BlockingQueue<Msg> msgQueue_ = null;
        private Comm comm_;
        private string address_;
        private int port_;
        private Thread sender_;
        private Thread receiver_;
        private Dictionary<string, Action<Msg>> messageDispatcher_;
        private string resultStore_ = "..\\..\\..\\ServerResult\\";
        private string rootPath = "..\\..\\..\\ServerRep";
        internal SpawnProc.SpawnProc sp_;

        //----------------<Constructor, initialize the server>-------------------
        public Server(string address,int port)
        {
            address_ = address;
            port_ = port;
            msgQueue_ = new BlockingQueue<Msg>();
            comm_ = new Comm(address, port);
            receiver_ = new Thread(getMessage);
            sender_ = new Thread(processMessage);
            messageDispatcher_ = new Dictionary<string, Action<Msg>>();
            receiver_.Start();
            sender_.Start();
            registerDepAn();
            registerTypeAn();
            registerSCC();
            registerSubdir();
            registerUpperDir();
            registerFiles();
            sp_ = new SpawnProc.SpawnProc();
        }

        //----------------<Create the command line argument for anlysis process>-------------------
        public string createCmd(Msg msg)
        {
            string req = msg.arguments[0];
            string path = msg.arguments[1];
            string cmd = req + '-' + path;
            return cmd;
        }

        //----------------<Send back the analysis result to client>--------------------------------
        public void sendResult(Msg msg)
        {
            Thread.Sleep(10);
            Msg returnMsg=new Msg(Msg.MessageType.reply);
            returnMsg.to = msg.from;
            returnMsg.from = address_ + ':' + port_.ToString() + "/IPluggableComm";
            string path = resultStore_ + msg.arguments[0] + ".txt";
            returnMsg.arguments.Add(msg.arguments[0]+"_"+msg.command+".txt");
            string line;
            System.IO.FileStream freader = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamReader strReader = new System.IO.StreamReader(freader);
            while ((line = strReader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                returnMsg.arguments.Add(line);
            }
            returnMsg.command = "Show result";
            comm_.postMessage(returnMsg);
        }
        
        //---------------------------<Do type analysis and send back result>---------------
        public void registerTypeAn()
        {
            Action<Msg> act = new Action<Msg>((Msg msg) => { sendResult(msg); });
            if(!messageDispatcher_.ContainsKey("TypeAnalysis"))
                messageDispatcher_["TypeAnalysis"] = (Msg msg)=> 
            {
                string path = System.IO.Path.GetFullPath("../../../TypeAnalyzer/bin/Debug/TypeAnalyzer.exe");
                Console.WriteLine("\nDoing type analysis {0}", createCmd(msg));
                sp_.createProcess(path, createCmd(msg),act,msg);
            };
        }
       

        //------------------------<Do dependency analysis and send back result>----------
        public void registerDepAn()
        {
            Action<Msg> act = new Action<Msg>((Msg msg) => { sendResult(msg); });
            if (!messageDispatcher_.ContainsKey("DepAnalysis"))
                messageDispatcher_["DepAnalysis"] = (Msg msg) =>
            {
                string path = System.IO.Path.GetFullPath("../../../DepAnalyzer/bin/Debug/DepAnalyzer.exe");
                Console.WriteLine("\nDoing dependecy analysis");
                sp_.createProcess(path, createCmd(msg), act, msg);
            };
        }

        //----------------------<Do SCC analysis and send back result>-------------
        public void registerSCC()
        {
            Action<Msg> act = new Action<Msg>((Msg msg) => { sendResult(msg); });
            if (!messageDispatcher_.ContainsKey("SCC"))
                messageDispatcher_["SCC"] = (Msg msg) =>
            {
                string path=System.IO.Path.GetFullPath("../../../SCCanalyzer/bin/Debug/SCCanalyzer.exe");
                  Console.WriteLine("\nConstructing strong connected component");
                  sp_.createProcess(path, createCmd(msg),act, msg);
            };
        }
        //--------------<Return the sub directories and all files under this directory tree>------------
        public void registerSubdir()
        {
            if(!messageDispatcher_.ContainsKey("goSubDir"))
            {
                messageDispatcher_["goSubDir"] = (Msg msg) =>
                  {
                      string subDir = msg.arguments[0];
                      Console.WriteLine("Subdir: {0}", subDir);
                      List<string> subDirs = getSubDirs(subDir);
                      string dirNum = subDirs.Count.ToString();
                      List<string> files = getFiles(subDir);
                      Msg returnMsg = new Msg(Msg.MessageType.reply);
                      returnMsg.command = "updateDirAndFile";
                      returnMsg.arguments.Add(subDir);
                      returnMsg.arguments.Add(dirNum);
                      foreach(string dir in subDirs)
                      {
                          Console.WriteLine(dir);
                          returnMsg.arguments.Add(dir);
                      }
                      foreach(string file in files)
                      {
                          Console.WriteLine(file);
                          returnMsg.arguments.Add(file);
                      }
                      returnMsg.to = msg.from;
                      returnMsg.from = address_ + ':' + port_.ToString() + "/IPluggableComm";
                      comm_.postMessage(returnMsg);

                  };
            }
        }

        //--------------------<Update the parent directory and all files under parent tree>-----------
        public void registerUpperDir()
        {
            if (!messageDispatcher_.ContainsKey("goUpperDir"))
            {
                messageDispatcher_["goUpperDir"] = (Msg msg) =>
                  {
                      string curDir = msg.arguments[0];
                      string upperDir = getParent(curDir);
                      List<string> dirs = getSubDirs(upperDir);
                      List<string> files = getFiles(upperDir);
                      string dirNum = dirs.Count.ToString();
                      Msg returnMsg = new Msg(Msg.MessageType.reply);
                      returnMsg.command = "updateDirAndFile";
                      returnMsg.arguments.Add(upperDir);
                      returnMsg.arguments.Add(dirNum);
                      foreach(string dir in dirs)
                      {
                          returnMsg.arguments.Add(dir);
                      }
                      foreach(string file in files)
                      {
                          returnMsg.arguments.Add(file);
                      }
                      returnMsg.to = msg.from;
                      returnMsg.from = address_ + ':' + port_.ToString() + "/IPluggableComm";
                      comm_.postMessage(returnMsg);

                  };
            }
        }

        //----------------<Update the files under certain directory tree>--------------
        public void registerFiles()
        {
            if (!messageDispatcher_.ContainsKey("UpdateFiles"))
            {
                messageDispatcher_["UpdateFiles"] = (Msg msg) =>
                {
                    string dir = msg.arguments[0];
                    List<string> files = getFiles(dir);
                    Msg returnMsg = new Msg(Msg.MessageType.reply);
                    returnMsg.command = "UpdateFiles";
                    returnMsg.to = msg.from;
                    returnMsg.from = address_ + ':' + port_.ToString() + "/IPluggableComm";
                    returnMsg.arguments = files;
                    comm_.postMessage(returnMsg);

                };
            }
                
        }

        //----------------<Get the sub directory under specified directory>-----------
        public List<string> getSubDirs(string path)
        {
            List<string> dirs = new List<string>();
            if(System.IO.Directory.Exists(path))
            {
                dirs = System.IO.Directory.GetDirectories(path).ToList<string>();
            }
            return dirs;
        }

        //---------------<Get parent directory>------------------------------------
        public string getParent(string path)
        {
            string fullparent = System.IO.Directory.GetParent(path).FullName;
            int index = fullparent.IndexOf("ServerRep");
            if (index == -1) return rootPath;
            return rootPath + fullparent.Substring(index + 9);

        }

        //---------------------<Get all the files under specified directory tree>-----------
        public List<string>getFiles(string path)
        {
            List<string> Files = new List<string>();
            if (System.IO.Directory.Exists(path))
            {
                Files = System.IO.Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories).ToList<string>();
            }
            return Files;
        }

        //--------------<process income message>---------------
        public void getMessage()
        {
          
            while(true)
            {
                Msg msg = comm_.getMessage();
                if (msg.type == Msg.MessageType.closeReceiver)
                {
                    Console.WriteLine("Server shutting down");
                    msg.type = Msg.MessageType.closeSender;
                    msgQueue_.enQ(msg);
                    break;
                }
                if(msg.type==Msg.MessageType.request)
                {
                    Console.WriteLine("Received request from {0}", msg.from);
                    Console.WriteLine("Command: {0},",msg.command);
                    msgQueue_.enQ(msg);
                }
            }
        }

        //--------------<Read the request, do the action and send back reply>-----------
        public void processMessage()
        {
           while(true)
            {

                Msg msg = msgQueue_.deQ();
                if (msg.type == Msg.MessageType.closeSender)
                {
                    Console.WriteLine("Sending thread closed");
                    comm_.postMessage(msg);
                    break;
                }
                if(messageDispatcher_.ContainsKey(msg.command))
                    messageDispatcher_[msg.command](msg);

            }
        }

        static void Main()
        {
            try
            {
                Console.WriteLine("Start test server");
                Server testserver = new Server("http://localhost", 8081);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}