///////////////////////////////////////////////////////////////////////
// AutomatedTestClient.cs - Executive for depedency analysis         //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Project #4                                   //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains a class with special client used for demonstration. This client will send 
 *   analysis request for dependency and strong connected component. Only for demonstration purpose, 
 *   instead of sending the result back to automated client, the result will be sent back to GUI client 
 *   for display.
 *
 * Required files:
 * - AutomatedTestClient.cs, MPCommService.cs,IMPCommService.cs
 * 
 * 
 * Note:
 * - This client does not have any GUI function
 * - To simplyfy the demonstration, the requested directory is fixed to ../../../ServerRep
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 11/29/2018
 * - First release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace MessagePassingComm
{
    using Msg= CommMessage;
    ////////////////////////////////////////////////////////////////////
    /// AutoClient: automated test client, will send request for dependency
    ///             analysis and SCC analysis. The result will be send back 
    ///             to GUI client. for demonstration purpose only
    /// 
    
    public class AutoClient
    {
        private string autoClientAdd_= "http://localhost:8082/IPluggableComm";
        private string guiClientAdd_;
        private string serverAdd_= "http://localhost:8081/IPluggableComm";
        private Comm comm_;

        //---------------<Constructor>------------
        public AutoClient(string port)
        {
            comm_ = new Comm("http://localhost", 8082);
            guiClientAdd_ = "http://localhost:" + port + "/IPluggableComm";
        }

        //----------------<Close all relevent thread gracefully>-----------
        public void shutDown()
        {
            Msg msg = new Msg(Msg.MessageType.closeReceiver);
            msg.to = autoClientAdd_;
            msg.from = autoClientAdd_;
            comm_.postMessage(msg);
            msg.type = Msg.MessageType.closeSender;
            comm_.postMessage(msg);

        }

        //----------------<Used for demonstration on dependency analysis>-----------
        public void sendDep()
        {

            Console.WriteLine("Auto client {0} send request for depedency analysis ", autoClientAdd_);
            Console.WriteLine("Result will be displayed on GUI client {0}", guiClientAdd_);
            Console.WriteLine("Gui client has same function which requires user action");
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = guiClientAdd_;
            msg.to = serverAdd_;
            msg.command = "DepAnalysis";
            msg.arguments.Add("DepDemo");
            msg.arguments.Add( "../../../ServerRep");
            comm_.postMessage(msg);
        }

        //-----------------<Used for demonstration on SCC analysis>-------------------
        public void sendScc()
        {
            Console.WriteLine("Auto client {0} send request for Strong connected component ", autoClientAdd_);
            Console.WriteLine("Result will be displayed on GUI client {0}", guiClientAdd_);
            Console.WriteLine("Gui client has same function which requires user action");
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = guiClientAdd_;
            msg.to = serverAdd_;
            msg.command = "SCC";
            msg.arguments.Add("SCCdemo");
            msg.arguments.Add("../../../ServerRep");
            comm_.postMessage(msg);
        }
#if (TEST_AUTOCLIENT)
        static void Main(string[] args)
        {
            Thread.Sleep(3000);
            AutoClient acl = new AutoClient(args[0]);
            acl.sendDep();
            acl.sendScc();
            acl.shutDown();
           
        }
#endif
    }
}
