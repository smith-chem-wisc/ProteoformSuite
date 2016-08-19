using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public partial class Quantification : Form
    {
        public Quantification()
        {
            InitializeComponent();
        }

        private void Quantification_Load(object sender, EventArgs e)
        {
            MessageBox.Show("hello from quantification");
        }
    }
}
