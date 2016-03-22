using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_0._00
{
    public partial class Form1 : Form
    {
        //  Initialize Forms START
        LoadDeconvolutionResults lDR;
        RawExperimentalProteoforms rEP;
        NeuCodePairs nCP;
        AggregatedProteoforms aGP;
        TheoreticalDatabase tDB;
        ExperimentTheoreticalComparison eTC;
        //  Initialize Forms END

        // Data associated with lDR Form START

        // Data associated with lDR Form END

        public Form1()
        {
            InitializeComponent();
        }

        private void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

            if (lDR == null)
            {
                lDR = new LoadDeconvolutionResults();
           
                //lDR.PassString = "This string from form 1";// this sends data to the lDR form.

                lDR.MdiParent = this;
                lDR.Show();
                lDR.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
            else
            {
                lDR.Show();
                lDR.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
        }

        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rEP == null)
            {
                rEP = new RawExperimentalProteoforms();
                rEP.MdiParent = this;
                rEP.Show();
                rEP.WindowState = FormWindowState.Maximized;
            }
            else
            {
                rEP.Show();
                rEP.WindowState = FormWindowState.Maximized;
            }
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (nCP == null)
            {
                nCP = new NeuCodePairs();

                //lDR.PassString = "This string from form 1";// this sends data to the lDR form.

                nCP.MdiParent = this;
                nCP.Show();
                nCP.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
            else
            {
                nCP.Show();
                nCP.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aGP == null)
            {
                aGP = new AggregatedProteoforms();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                aGP.MdiParent = this;
                aGP.Show();
                aGP.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                aGP.Show();
                aGP.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tDB == null)
            {
                tDB = new TheoreticalDatabase();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                tDB.MdiParent = this;
                tDB.Show();
                tDB.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                tDB.Show();
                tDB.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (eTC == null)
            {
                eTC = new ExperimentTheoreticalComparison();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                eTC.MdiParent = this;
                eTC.Show();
                eTC.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                eTC.Show();
                eTC.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("experimentExperimentComparisonToolStripMenuItem_Click");
        }

        private void proteoformFamilyGraphsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("proteoformFamilyGraphsToolStripMenuItem_Click");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("openToolStripMenuItem_Click");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveToolStripMenuItem_Click");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveAsToolStripMenuItem_Click");
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void loadMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("loadMethodToolStripMenuItem_Click");
        }

        private void saveMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveMethodToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
