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

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for testWin.xaml
    /// </summary>
    public partial class testWin : Window
    {
        public testWin()
        {
            InitializeComponent();
            
        }

        private void Dissoci_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void testFunc(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            this.test.cmb_loadTable.Text = "test"+rnd.Next();
        }
    }
}
