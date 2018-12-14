///////////////////////////////////////////////////////////////////////
// DeleteConfirm.xaml.cs - Client with GUI                           //
// ver 1.0                                                           //
// Language:    C#                                                   //
// Platform:    Win10,VS2017, Surface Pro M3                         //
// Application: CSE681, Remote Package Dependency Analysis           //
// Author:      Yilin Cui, ycui21@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package provides a DeleteConfirm class which allows user to think twice before
 *   deleting the history analysis result
 * Required files:
 * - MainWindow.xaml.cs
 * - DeleteConfirm.xaml.cs
 * - DeleteConfirm.xaml
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
    ////////////////////////////////////////////////////////////////////////////
    /// DeleteConfirm class, give user a chance to think twice
    ///
    public partial class DeleteConfirm : Window
    {
        private MainWindow mw_ = null;
        public DeleteConfirm(MainWindow mw)
        {
            mw_ = mw;
            InitializeComponent();
        }


        //------------------------<Users do want to delete>---------------------
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            mw_.del = true;
            Close();
        }

        //------------------------<User do not want to delete>--------------------
        private void No_Click(object sender, RoutedEventArgs e)
        {
            mw_.del = false;
            Close();
        }
    }
}
