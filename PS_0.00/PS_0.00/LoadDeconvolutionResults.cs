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
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblis/Extension/Microsoft.Office.Interop.Excel

namespace PS_0._00
{
    public partial class LoadDeconvolutionResults : Form
    {
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        public static bool repeat = false;
        //private BindingList<string> deconResultsFiles = new BindingList<string>(); 

        public LoadDeconvolutionResults()
        {
            InitializeComponent();
        }
        public void LoadDeconvolutionResults_Load(object sender, EventArgs e)
        {            
            if (GlobalData.repeat == true)
            {
                repeat = true;
                GlobalData.repeat = false;
                LoadDeconvolutionResults_Load(sender, e);
                return;
            }
            InitializeOpenFileDialog();
            lbDeconResults.Sorted = true;
            lbDeconResults.SelectionMode = SelectionMode.MultiExtended;
            lbDeconResults.DataSource = GlobalData.deconResultsFileNames;
            if (repeat==true)
            {
                repeat = false;
                GlobalData.repeat = true;
            }
        }

        private void btnDeconResultsAdd_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {

                foreach (String file in openFileDialog1.FileNames)
                {
                    try
                    {
                        if (!GlobalData.deconResultsFileNames.Contains(file) && FirstLineOK(file))
                        {
                            GlobalData.deconResultsFileNames.Add(file);
                            //MessageBox.Show("Added File: " + file +" Count: " + GlobalData.deconResultsFileNames.Count);
                            
                        }
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

        private void btnDeconResultsRemove_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbDeconResults.SelectedItems.Count; i++)
            {
                GlobalData.deconResultsFileNames.Remove(lbDeconResults.SelectedItems[i].ToString());
            }

        }

        private void btnDeconResultsClear_Click(object sender, EventArgs e)
        {
            GlobalData.deconResultsFileNames.Clear();
        }

        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog1.Filter =
                "Excel (*.xlsx)|*.xlsx";

            // Allow the user to select multiple images.
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "My Deconvolution 4.0 Results Files";
        }

        private bool FirstLineOK (string fileName)
        {
            bool fileOK = true;

            return fileOK;
        }

        private void lbDeconResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}
