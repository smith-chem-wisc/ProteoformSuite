using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblies/Extension/Microsoft.Office.Interop.Excel

namespace ProteoformSuite
{
    public partial class LoadDeconvolutionResults : Form
    {

        public LoadDeconvolutionResults()
        {
            InitializeComponent();
        }

        public void LoadDeconvolutionResults_Load(object sender, EventArgs e)
        {            
            lbDeconResults.Sorted = true;
            lbDeconResults.SelectionMode = SelectionMode.MultiExtended;
            lbDeconResults.DataSource = Lollipop.deconResultsFileNames;

            lbCorrectionFiles.Sorted = true;
            lbCorrectionFiles.SelectionMode = SelectionMode.MultiExtended;
            lbCorrectionFiles.DataSource = Lollipop.correctionFactorFilenames;
        }

        private bool FirstLineOK (string fileName)
        {
            bool fileOK = true;

            return fileOK;
        }

        private void lbDeconResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

//        private void cb_neuCodeLabeled_CheckedChanged(object sender, EventArgs e)
//        {
//=======
//        }

        //private void lbDeconResults_SelectedIndexChanged(object sender, EventArgs e)
        //{
            
        //}

        //private void cb_neuCodeLabeled_CheckedChanged(object sender, EventArgs e)
        //{

        //}

        private void btnDeconResultsAdd_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";

            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    try
                    {
                        if (!Lollipop.deconResultsFileNames.Contains(file))
                            Lollipop.deconResultsFileNames.Add(file);
                    }
                    catch (SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the result fiel - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the file: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }
        }

        private void btnDeconResultsRemove_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < lbDeconResults.SelectedItems.Count; i++)
            {
                Lollipop.deconResultsFileNames.Remove(lbDeconResults.SelectedItems[i].ToString());
            }
        }

        private void btn_AddCorrectionFactors_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.Filter = "txt files (*.txt)|*.txt";


            openFileDialog2.Multiselect = true;
            openFileDialog2.Title = "My Correction Factor Files";

            DialogResult dr = openFileDialog2.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in openFileDialog2.FileNames)
                {
                    try
                    {
                        if (!Lollipop.correctionFactorFilenames.Contains(file))
                            Lollipop.correctionFactorFilenames.Add(file);
                    }
                    catch (SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the result fiel - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the file: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }
        }

        private void btnDeconResultsClear_Click_1(object sender, EventArgs e)
        {
            Lollipop.deconResultsFileNames.Clear();
            Lollipop.correctionFactorFilenames.Clear();
        }

        private void cb_neuCodeLabeled_CheckedChanged_1(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(cb_neuCodeLabeled.Checked);
            Lollipop.neucode_labeled = cb_neuCodeLabeled.Checked;
            Lollipop.neucode_light_lysine = cb_neuCodeLabeled.Checked;
            Lollipop.natural_lysine_isotope_abundance = !cb_neuCodeLabeled.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
