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
using FileFunctions;
using System.IO;

namespace PictHandler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
           // Dictionary<DateTime, List<FileSystemInfo>> futureDirStructure = Program.GetFutureDirStructure(@"C:\");
            //LoadFutureDirStructure(futureDirStructure);
        }

        void LoadFutureDirStructure(Dictionary<DateTime, List<FileSystemInfo>> items)
        {
            TreeViewItem root = new TreeViewItem() { Header = "root" };
            PictTree.Items.Add(root);
 
            foreach (var dir in items.ToArray())
            {
                TreeViewItem subDir = new TreeViewItem() { Header = dir.Key };
                root.Items.Add(subDir);
            }

        }
    }
}
