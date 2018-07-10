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
    /// Interaction logic for InputBox.xaml
    /// </summary>
    /// <remarks>
    /// - Dialogueresults changed to bool?
    /// - The dialogue return valur should be bool
    /// </remarks>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
            tb.Focusable = true;
            tb.Focus();
        }
        void tb_enter(object sender, KeyEventArgs e)
        {
            this.okay.IsDefault = true;
        }
        void okay_click(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        void cancel_click(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        
    }
}
