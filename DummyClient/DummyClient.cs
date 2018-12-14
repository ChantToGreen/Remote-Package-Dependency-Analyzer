using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWTools;
using System.Threading;
using MessagePassingComm;

namespace Analyzer
{
    using Msg = CommMessage;
    public class Client
    {
        private Comm comm_;
        private string address_;
        private int port_;
        private Thread receiver_;
        public Client(string address, int port)
        {
            address_ = address;
            port_ = port;
            comm_ = new Comm(address, port);
            receiver_ = new Thread(getMessage);
            receiver_.Start();

        }
        public void getMessage()
        {
            Msg msg;
            while(true)
            {
                msg = comm_.getMessage();
                Console.WriteLine("Received from {0}", msg.from);
                if (msg.type == Msg.MessageType.closeReceiver)
                {
                    Console.WriteLine("Reveiver thread closed!");
                    break;
                }
                if(msg.command== "Show result")
                {
                    Console.WriteLine("Showing result!");
                    foreach(string line in msg.arguments)
                    {
                        Console.WriteLine(line);
                    }
                }
                if(msg.command== "updateDirAndFile")
                {
                    Console.WriteLine("updateDirAndFile");
                    foreach (string line in msg.arguments)
                    {
                        Console.WriteLine(line);
                    }
                }

            }
        }
        public void postMessage(string address, int port)
        {

            //for(int i=0;i<25;++i)
            //{
            //    Msg msg=new Msg(Msg.MessageType.request);
            //    msg.command = "Dummy command"+i.ToString();
            //    msg.to= address + ":" + port.ToString() + "/IPluggableComm";
            //    msg.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //    Console.WriteLine("Sending msg, {0}", msg.command);
            //    comm_.postMessage(msg);
            //    Thread.Sleep(500);
            //}
            //Msg msg2 = new Msg(Msg.MessageType.closeReceiver);
            //msg2.to = address + ":" + port.ToString() + "/IPluggableComm";
            //msg2.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //comm_.postMessage(msg2);
            //Thread.Sleep(500);
            //Msg msg3 = new Msg(Msg.MessageType.closeReceiver);
            //msg3.to = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg3.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //comm_.postMessage(msg3);
            //Thread.Sleep(500);
            //Msg msg1 = new Msg(Msg.MessageType.request);
            //msg1.command = "TypeAnalysis";
            //msg1.to = address + ":" + port.ToString() + "/IPluggableComm";
            //msg1.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg1.arguments.Add("req1");
            //msg1.arguments.Add("../../../TestTypeAnaylyzer");
            //comm_.postMessage(msg1);
            //Console.WriteLine("Sending msg1, {0}", msg1.command);

            //Msg msg2 = new Msg(Msg.MessageType.request);
            //msg2.command = "DepAnalysis";
            //msg2.to = address + ":" + port.ToString() + "/IPluggableComm";
            //msg2.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg2.arguments.Add("req2");
            //msg2.arguments.Add("../../../TestTypeAnaylyzer");
            //comm_.postMessage(msg2);
            //Console.WriteLine("Sending msg2, {0}", msg2.command);

            //Msg msg3 = new Msg(Msg.MessageType.request);
            //msg3.command = "SCC";
            //msg3.to = address + ":" + port.ToString() + "/IPluggableComm";
            //msg3.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg3.arguments.Add("req3");
            //msg3.arguments.Add("../../../TestTypeAnaylyzer");
            //comm_.postMessage(msg3);
            //Console.WriteLine("Sending msg3, {0}", msg3.command);

            //Msg msg4 = new Msg(Msg.MessageType.closeReceiver);
            //msg4.to = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg4.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //comm_.postMessage(msg4);

            //Msg msg5 = new Msg(Msg.MessageType.closeSender);
            //msg5.to = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //msg5.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            //comm_.postMessage(msg4);
            Msg msg6 = new Msg(Msg.MessageType.request);
            msg6.command = "goUpperDir";
            msg6.to = address + ":" + port.ToString() + "/IPluggableComm";
            msg6.from = address_ + ":" + port_.ToString() + "/IPluggableComm";
            msg6.arguments.Add("..\\..\\..\\ServerRep\\sub1");
            comm_.postMessage(msg6);
            Console.WriteLine("Sending msg6, {0}", msg6.command);
        }
        static void Main()
        {
            Console.WriteLine("Start dummy client");
            Client cl = new Client("http://localhost", 8082);
            cl.postMessage("http://localhost", 8081);
            //Console.WriteLine("Client done!");
            Console.ReadLine();
            
        }
    }

}