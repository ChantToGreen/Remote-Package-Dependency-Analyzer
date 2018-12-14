///////////////////////////////////////////////////////////////////////
// FileWarning.xaml.cs - Client with GUI                             //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package provides a FileWarning class which alert user that the file doesn't exist
 * Required files:
 * - MainWindow.xaml.cs
 * - FileWarning.xaml.cs 
 * - FileWarning.xaml
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
using System.Windows.Shapes;

namespace Client
{
    //////////////////////////////////////////////////////////////////////
    /// FileWarning : alerts user that the file doesn't exist
    public partial class WarningRes : Window
    {
        
        public WarningRes()
        {
            InitializeComponent();
        }


        //---------------<Close the warning>------------------------------
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}