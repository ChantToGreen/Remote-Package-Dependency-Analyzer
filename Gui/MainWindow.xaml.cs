///////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client with GUI                              //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package provides MainWindow class which provides WPF GUI function for 
 *   Remote Package Dependency Analysis client. This class supports users to navigate
 *   the remote directories on the server side, send type, depedency and strong
 *   connected component analysis request to client, diplay new and old analysis result;
 *
 * Required files:
 * - TestUtilities
 * - MPCommService.cs
 * - MainWindow.xaml.cs
 * - MainWindow.xaml
 * - IMessagePassingComm.cs
 * - App.xaml.cs, App.xaml
 * - AnalysisResult.xaml.cs, AnalysisResult.xaml
 * - FileWarning.xaml.cs, FileWarning.xaml
 * - DeleteConfirm.xaml.cs, DeleteConfirm.xaml
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessagePassingComm;
using System.Threading;

namespace Client
{
    using Msg = CommMessage;
    //////////////////////////////////////////////////////////////////////////////
    // MainWindow class, provides functions for Remote Dependency Analysis client
    //
    public partial class MainWindow : Window
    {
        private int reqNum = 1;
        public string currentPath { get; set; }
        public string resultStore { get; set; } = "../../../History/";
        public AnalysisResult anResult { get; set; } = null;
        public WarningRes wr { get; set; } = null;
        public DeleteConfirm dw { get; set; } = null;
        public bool del { get; set; } = false;
        private Comm comm_;
        private string clientFullAddress_ ;
        private string serverFullAddress_ = "http://localhost:8081/IPluggableComm";
        private Dictionary<string, Action<Msg>> messageDispatcher_;
        private Thread receiver_;
        private bool serverRunning=true;

        //--------------------<Initialize the window for client>------------
        public MainWindow()
        {
            InitializeComponent();
            int port = Int32.Parse(App.mArgs[0]);
            clientFullAddress_ = "http://localhost:" + App.mArgs[0] + "/IPluggableComm";
            comm_ = new Comm("http://localhost", port);
            messageDispatcher_ = new Dictionary<string, Action<Msg>>();
            registerUpdate();
            registerStoreResult();
            registerFiles();
            receiver_ = new Thread(procMsg);
            receiver_.IsBackground = true;
            receiver_.Start();
            loadResultTab();
            postFirstMessage();

        }


        //---------------<Navigate deeper into the remote directory tree>---------
        private void goDeeper(string selecetedPath)
        {
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "goSubDir";
            msg.arguments.Add(selecetedPath);
            comm_.postMessage(msg);

        }

        //-------------<Navigate back to parent directory>----------
        private void goUpper()
        {
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "goUpperDir";
            msg.arguments.Add(currentPath);
            comm_.postMessage(msg);
        }

        //------------<Send out the first message request for remote directory information>--------
        private void postFirstMessage()
        {
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "goSubDir";
            msg.arguments.Add("..\\..\\..\\ServerRep");
            comm_.postMessage(msg);
          
        }
        
       
        //------------<Updating the information in Set Directory tab>--------
        private void registerUpdate()
        {
            if (messageDispatcher_.ContainsKey("updateDirAndFile"))
                return;
            messageDispatcher_["updateDirAndFile"] = (Msg msg) =>
              {
                  Dirs.Items.Clear();
                  currentPath = msg.arguments[0];
                  TargetPath.Text = currentPath;
                  Dirs.Items.Add("..");
                  int dirNum = Int32.Parse(msg.arguments[1]);
                  int i = 2;
                  while (i <= dirNum+1)
                  {
                      Dirs.Items.Add(msg.arguments[i]);
                      i++;
                  }
                  StringBuilder files=new StringBuilder() ;
                  while (i < msg.arguments.Count)
                  {
                      files.Append(msg.arguments[i]).Append('\n');
                      i++;
                  }
                  Files.Text = files.ToString();
              };
        }

        //--------------<Store and show the result of analysis>----------
        private void registerStoreResult()
        {
            if (messageDispatcher_.ContainsKey("Show result"))
                return;
            messageDispatcher_["Show result"] = (Msg msg) =>
            {
                StringBuilder time = new StringBuilder(System.DateTime.Now.ToString());
                for(int i=0;i<time.Length;++i)
                {
                    if (time[i] == ':'|| time[i] == ' ' || time[i] == '\\' || time[i] == '/') 
                        time[i] = '_';
                }
                string dateTime = time.ToString();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(resultStore+dateTime + "_" + msg.arguments[0]);
                for(int i=1;i<msg.arguments.Count;++i)
                {
                    sw.WriteLine(msg.arguments[i]);
                }
                sw.Close();
                System.IO.StreamReader sr = new System.IO.StreamReader(resultStore + dateTime + "_" + msg.arguments[0]);
                Console.WriteLine(resultStore + msg.arguments[0]);
                anResult = new AnalysisResult();
                anResult.resultView.Text = sr.ReadToEnd();
                anResult.Show();
                sr.Close();
                ResultList.Items.Add(resultStore + dateTime + "_" + msg.arguments[0]);
            };
        }
         
        //----------------<Update the files under sub directory tree>---------
        void registerFiles()
        {
            if (messageDispatcher_.ContainsKey("UpdateFiles"))
                return;
            messageDispatcher_["UpdateFiles"] = (Msg msg) =>
            {
                int i = 0;
                StringBuilder files = new StringBuilder();
                while (i < msg.arguments.Count)
                {
                    files.Append(msg.arguments[i]).Append('\n');
                    i++;
                }
                Files.Text = files.ToString();
            };
        }

        //------------------<Refresh the result list>----------
        void loadResultTab()
        {
            ResultList.Items.Clear();
            string[] files = System.IO.Directory.GetFiles(resultStore);
            foreach (string file in files)
            {
                ResultList.Items.Add(file);
            }
            

        }


        //------------------<Navigate to the sub directory>---------------
        private void Dirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!serverRunning) return;
            if (Dirs.SelectedIndex == -1)
                return;
            if (Dirs.SelectedValue.ToString() == ".." && currentPath != "..\\..\\..\\ServerRep")
            {
                Console.WriteLine("Current: {0}",currentPath);
                goUpper();
            }
            else if (Dirs.SelectedValue.ToString() == ".." && currentPath == "..\\..\\..\\ServerRep")
            {
                TargetPath.Text = "..\\..\\..\\ServerRep";
            }
            else if(Dirs.SelectedValue.ToString() != "..")
            {
                Console.WriteLine(Dirs.SelectedValue.ToString());
                goDeeper(Dirs.SelectedValue.ToString());
            }
           
        }


        //------------<Send request for type analysis>----------------
        private void TypeAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (!serverRunning) return;
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "TypeAnalysis";
            msg.arguments.Add("Request" + reqNum.ToString());
            reqNum++;
            msg.arguments.Add(TargetPath.Text);
            Console.WriteLine("Path: {0}", msg.arguments[1]);
            comm_.postMessage(msg);

        }

        //----------------<Send request for dependency analysis>------------------
        private void DepAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (!serverRunning) return;
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "DepAnalysis";
            msg.arguments.Add("Request" + reqNum.ToString());
            reqNum++;
            msg.arguments.Add(TargetPath.Text);
            Console.WriteLine("Path: {0}", msg.arguments[1]);
            comm_.postMessage(msg);
        }

        //------------------<Send request for strong connected component analysis>----------------
        private void StrongConnectedComponent_Click(object sender, RoutedEventArgs e)
        {
            if (!serverRunning) return;
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "SCC";
            msg.arguments.Add("Request" + reqNum.ToString());
            reqNum++;
            msg.arguments.Add(TargetPath.Text);
            Console.WriteLine("Path: {0}", msg.arguments[1]);
            comm_.postMessage(msg);
        }

        //-----------------<Show the result for previous analysis>--------------
        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedIndex == -1)
                return;
            string res = ResultList.SelectedValue.ToString();
            if(!System.IO.File.Exists(res))
            {
                ResultList.Items.Remove(ResultList.SelectedItem);
                wr = new WarningRes();
                wr.Show();
                return;
            }
            string content = System.IO.File.ReadAllText(res);
            anResult = new AnalysisResult();
            anResult.resultView.Text = content;
            anResult.Show();
        }


        //------------<Single click to choose the target directory>--------------
        private void Dirs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!serverRunning) return;
            if (Dirs.SelectedIndex == -1)
                return;
            string selectedDir = Dirs.SelectedItem.ToString();
            if (selectedDir != "..") 
                TargetPath.Text = selectedDir;
            Msg msg = new Msg(Msg.MessageType.request);
            msg.from = clientFullAddress_;
            msg.to = serverFullAddress_;
            msg.command = "UpdateFiles";
            msg.arguments.Add(TargetPath.Text);
            comm_.postMessage(msg);

        }

        //------------<Function for receiver thread to accept message and update GUI>------------
        private void procMsg()
        {
            while (true)
            {
                Msg msg = comm_.getMessage();
                Console.WriteLine("command arrived : {0}", msg.command);
                if (msg.type == Msg.MessageType.closeReceiver)
                {
                    Console.WriteLine("Shut down comm receiver");
                    break;
                }
                if (msg.type == Msg.MessageType.reply)
                {
                    Console.WriteLine("Recieved reply with command: {0}", msg.command);
                    if (messageDispatcher_.ContainsKey(msg.command))
                        Dispatcher.Invoke(messageDispatcher_[msg.command], new object[] { msg });

                }
            }

        }



        //------------------<Shut down the comm and thread when exit the client>--------------
        private void Window_Closed(object sender, EventArgs e)
        {
            Msg msg = new Msg(Msg.MessageType.closeReceiver);
            msg.to = clientFullAddress_;
            msg.from = clientFullAddress_;
            comm_.postMessage(msg);
            msg.type=Msg.MessageType.closeSender;
            comm_.postMessage(msg);
        }


        //----------<Open history request log>------------
        private void OpenHis_Click(object sender, RoutedEventArgs e)
        {
            if (ResultList.SelectedIndex == -1)
                return;
            string res = ResultList.SelectedValue.ToString();
            if (!System.IO.File.Exists(res))
            {
                ResultList.Items.Remove(ResultList.SelectedItem);
                wr = new WarningRes();
                wr.Show();
                return;
            }
            string content = System.IO.File.ReadAllText(res);
            anResult = new AnalysisResult();
            anResult.resultView.Text = content;
            anResult.Show();
        }

        //--------------<Delete the history request log>------------
        private void DeleteHis_Click(object sender, RoutedEventArgs e)
        {
            string path = ResultList.SelectedValue.ToString();
            if (!System.IO.File.Exists(path))
            {
                ResultList.Items.Remove(ResultList.SelectedItem);
                wr = new WarningRes();
                wr.Show();
                return;
            }
            else
            {
                dw = new DeleteConfirm(this);
                dw.ShowDialog();
                if (del)
                {
                    System.IO.File.Delete(path);
                    ResultList.Items.Remove(ResultList.SelectedItem);
                }
            }
        }

        //------------<Use Top button to go back to parent directory
        private void Top_Click(object sender, RoutedEventArgs e)
        {

            if (!serverRunning) return;
            goUpper();
        }

        //-----------------------<Open the folder>------------------
        private void Open_Click(object sender, RoutedEventArgs e)
        {

            if (!serverRunning) return;
            if (Dirs.SelectedIndex == -1)
                return;
            if (Dirs.SelectedValue.ToString() == ".." && currentPath != "..\\..\\..\\ServerRep")
            {
                Console.WriteLine("Current: {0}", currentPath);
                goUpper();
            }
            else if (Dirs.SelectedValue.ToString() == ".." && currentPath == "..\\..\\..\\ServerRep")
            {
                TargetPath.Text = "..\\..\\..\\ServerRep";
            }
            else if (Dirs.SelectedValue.ToString() != "..")
            {
                Console.WriteLine(Dirs.SelectedValue.ToString());
                goDeeper(Dirs.SelectedValue.ToString());
            }
        }

        //-----------------<Send order to shut down server, for unknown reason server may take 3-5 seconds to shut down>----------------
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!serverRunning) return;
            Msg msg = new Msg(Msg.MessageType.closeReceiver);
            msg.to = serverFullAddress_;
            msg.from = clientFullAddress_;
            comm_.postMessage(msg);
            serverRunning = false;
        }
    }
}
