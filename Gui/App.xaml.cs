///////////////////////////////////////////////////////////////////////
// App.xaml.cs - Configures the environemtn for WPF application      //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package provides a App class to configure the startup environment for WPF application
 *   
 *   
 * Required files:
 * - App.xaml
 * - App.xaml.cs
 * 
 * Note:
 * - None
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 12/4/2018
 * - First release
 *
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    //////////////////////////////////////////////////////////////
    /// App class: used for passing commandline argument to GUI
    /// 
    public partial class App : Application
    {
        public static String[] mArgs;

        //---------------<Configure the startup for application>-----------
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(e.Args.Length>0)
            {
                mArgs = e.Args;
            }
        }
    }
}
