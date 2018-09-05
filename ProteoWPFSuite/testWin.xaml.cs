using System;
using System.Windows;
using System.Windows.Controls;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            ProteoformSweet ctn = new ProteoformSweet();
            Window wdo = new Window
            {
                Title = "Proteo Display",
                Content = new ProteoformSweet(),
                MinHeight = 450,
                MinWidth = 800
            };
            wdo.Show();
            
            
        }

        private void Dissoci_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
