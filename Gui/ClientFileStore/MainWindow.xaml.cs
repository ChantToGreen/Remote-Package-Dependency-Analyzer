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

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string path { get; set; }
        public SelectionWindow swin { get; set; } = null;//For pop up window
        public MainWindow()
        {
            InitializeComponent();
        }
        string getAncester(int n,string path)
        {
            for(int i=0;i<n;++i)
            {
                path = System.IO.Directory.GetParent(path).FullName;
            }
            return path;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            path = System.IO.Directory.GetCurrentDirectory();
            path = getAncester(2, path);
            LoadNavigatorTab(path);
        }
        void LoadNavigatorTab(string path)
        {
            Dirs.Items.Clear();//Clear everything out of list box
            CurrentPath.Text = path;//Writing the path to the top page
            String[] dirs = System.IO.Directory.GetDirectories(path);
            Dirs.Items.Add("..");// Click to go back
            foreach(string dir in dirs)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                //string name = System.IO.Path.GetDirectoryName(dir);
                Dirs.Items.Add(di.Name);
            }
            Files.Items.Clear();
            string[] files = System.IO.Directory.GetFiles(path);
            foreach(string file in files)
            {
                string name = System.IO.Path.GetFileName(file);//Get rid of the path
                Files.Items.Add(name);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
          
        }
        private void Dirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Dirs.SelectedIndex == -1)//Nothing selected
                return;
            string selectedDir = Dirs.SelectedItem.ToString();
            if (selectedDir == "..")
                path = getAncester(1, path);
            else
                path = System.IO.Path.Combine(path, selectedDir);
            LoadNavigatorTab(path);
        }

        private void TypeAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (Dirs.SelectedIndex == -1)//Nothing selected
                return;
            string dir = Dirs.SelectedItem.ToString();
            CurrentPath.Text = dir;
        }

        private void DepAnalysis_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StrongConnectedComponent_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
