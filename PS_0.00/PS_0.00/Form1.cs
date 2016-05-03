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
        LoadDeconvolutionResults loadDeconvolutionResults;
        RawExperimentalComponents rawExperimentalComponents;
        NeuCodePairs neuCodePairs;
        AggregatedProteoforms aggregatedProteoforms;
        TheoreticalDatabase theoreticalDatabase;
        ExperimentTheoreticalComparison experimentalTheoreticalComparison;
        ExperimentDecoyComparison experimentDecoyComparison;
        ExperimentExperimentComparison experimentExperimentComparison;
        ProteoformFamilyAssignment proteoformFamilyAssignment;
        //  Initialize Forms END

        // Data associated with lDR Form START

        // Data associated with lDR Form END

        public Form1()
        {
            InitializeComponent();
        }

        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadDeconvolutionResults == null)
            {
                loadDeconvolutionResults = new LoadDeconvolutionResults();
           
                //lDR.PassString = "This string from form 1";// this sends data to the lDR form.

                loadDeconvolutionResults.MdiParent = this;
                loadDeconvolutionResults.Show();
                loadDeconvolutionResults.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
            else
            {
                loadDeconvolutionResults.Show();
                loadDeconvolutionResults.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
        }

        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rawExperimentalComponents == null)
            {
                rawExperimentalComponents = new RawExperimentalComponents();
                rawExperimentalComponents.MdiParent = this;
                rawExperimentalComponents.Show();
                rawExperimentalComponents.WindowState = FormWindowState.Maximized;
            }
            else
            {
                if (GlobalData.repeat == true)
                {
                    GlobalData.repeat = false;
                    rawExperimentalComponents.RawExperimentalComponents_Load(GlobalData.repeatsender, GlobalData.repeate);
                }
                rawExperimentalComponents.Show();
                rawExperimentalComponents.WindowState = FormWindowState.Maximized;
            }
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (neuCodePairs == null)
            {
                neuCodePairs = new NeuCodePairs();

                //lDR.PassString = "This string from form 1";// this sends data to the lDR form.

                neuCodePairs.MdiParent = this;
                neuCodePairs.Show();
                neuCodePairs.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
            else
            {
                neuCodePairs.Show();
                neuCodePairs.WindowState = FormWindowState.Maximized;
                // This is where we get data back from lDR form
            }
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aggregatedProteoforms == null)
            {
                aggregatedProteoforms = new AggregatedProteoforms();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                aggregatedProteoforms.MdiParent = this;
                aggregatedProteoforms.Show();
                aggregatedProteoforms.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                aggregatedProteoforms.Show();
                aggregatedProteoforms.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theoreticalDatabase == null)
            {
                theoreticalDatabase = new TheoreticalDatabase();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                theoreticalDatabase.MdiParent = this;
                theoreticalDatabase.Show();
                theoreticalDatabase.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                theoreticalDatabase.Show();
                theoreticalDatabase.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (experimentalTheoreticalComparison == null)
            {
                experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                experimentalTheoreticalComparison.MdiParent = this;
                experimentalTheoreticalComparison.Show();
                experimentalTheoreticalComparison.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                experimentalTheoreticalComparison.Show();
                experimentalTheoreticalComparison.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }
        
        private void experimentDecoyComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (experimentDecoyComparison == null)
            {
                experimentDecoyComparison = new ExperimentDecoyComparison();

                experimentDecoyComparison.MdiParent = this;
                experimentDecoyComparison.Show();
                experimentDecoyComparison.WindowState = FormWindowState.Maximized;
            }
            else
            {
                experimentDecoyComparison.Show();
                experimentDecoyComparison.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }

        }


        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (experimentExperimentComparison == null)
            {
                experimentExperimentComparison = new ExperimentExperimentComparison();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                experimentExperimentComparison.MdiParent = this;
                experimentExperimentComparison.Show();
                experimentExperimentComparison.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                experimentExperimentComparison.Show();
                experimentExperimentComparison.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (proteoformFamilyAssignment == null)
            {
                proteoformFamilyAssignment = new ProteoformFamilyAssignment();

                //lDR.PassString = "This string from form 1";// this sends data to the aGP form.

                proteoformFamilyAssignment.MdiParent = this;
                proteoformFamilyAssignment.Show();
                proteoformFamilyAssignment.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
            else
            {
                proteoformFamilyAssignment.Show();
                proteoformFamilyAssignment.WindowState = FormWindowState.Maximized;
                // This is where we get data back from aGP form
            }
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
