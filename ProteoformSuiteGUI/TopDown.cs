using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProteoformSuiteInternal;
using System.IO;

namespace ProteoformSuite
{
    public partial class TopDown : Form
    {
        public TopDown()
        {
            InitializeComponent();
        }

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Lollipop.proteoform_community.topdown_proteoforms);
        }
    }
}
