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
using ProteoformSuiteInternal;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.MinHeight = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.75);
            this.MinWidth = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.50);
            this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.95);
            this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.75);
            this.Activate();
        }

        private void ProteoformSweet_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
