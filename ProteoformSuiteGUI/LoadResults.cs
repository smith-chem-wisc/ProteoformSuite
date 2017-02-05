using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblies/Extension/Microsoft.Office.Interop.Excel


namespace ProteoformSuite
{
    public partial class LoadResults : Form
    {

        public LoadResults()
        {
            InitializeComponent();
        }

        public void loadResults_Load(object sender, EventArgs e)
        { }

        private void btn_neucode_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_unlabeled.Checked)
            {
                ProteoformSweet.run_when_form_loads = false; //if unlabeled, don't run automatically. 
                cb_run_when_load.Checked = false;
            }
            else
            {
                ProteoformSweet.run_when_form_loads = true; //if unlabeled, don't run automatically. 
                cb_run_when_load.Checked = true;
            }
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(btn_neucode.Checked);
            Lollipop.neucode_labeled = btn_neucode.Checked;
            Lollipop.neucode_light_lysine = btn_neucode.Checked;
            Lollipop.natural_lysine_isotope_abundance = !btn_neucode.Checked;

            foreach (InputFile f in Lollipop.input_files)
            {
                if (btn_neucode.Checked) f.label = Labeling.NeuCode;
                if (btn_unlabeled.Checked) f.label = Labeling.Unlabeled;
            }

            match_files();
        }


        // DGV DRAG AND DROP EVENTS
        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { ".xlsx" }, Purpose.Identification);
            match_files();

            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.identification_files());
        }
        private void dgv_quantResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { ".xlsx" }, Purpose.Quantification);
            match_files();

            DisplayUtility.FillDataGridView(dgv_quantitationFiles, Lollipop.quantification_files());
        }
        private void dgv_calibrationResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { ".txt", ".tsv" }, Purpose.Calibration);
            match_files();

            DisplayUtility.FillDataGridView(dgv_calibrationFiles, Lollipop.calibration_files());
        }
        private void dgv_deconResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void dgv_calibrationResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void dgv_quantResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, Purpose purpose)
        {
            foreach (string enteredFile in files)
            {
                string path = Path.GetDirectoryName(enteredFile);
                string filename = Path.GetFileNameWithoutExtension(enteredFile);
                string extension = Path.GetExtension(enteredFile);
                Labeling label = Labeling.Unlabeled;
                if (btn_neucode.Checked) label = Labeling.NeuCode;

                if (acceptable_extensions.Contains(extension) && !Lollipop.input_files.Where(f => f.purpose == purpose).Any(f => f.filename == filename))
                {

                    // this next section is commented out for testing. allows same files to be used for identification and quantification.

                    //// Handle the conflict of loading the same deconvolution results into identification and quantitation
                    //if ((purpose == Purpose.Identification || purpose == Purpose.Quantification) &&
                    //    (Lollipop.identification_files().Any(g => g.filename == filename) || Lollipop.quantification_files().Any(g => g.filename == filename)))
                    //{
                    //    var results = MessageBox.Show("Use " + filename + extension + " for " + purpose.ToString() + "?", "Identification/Quantitation Result Conflict", MessageBoxButtons.YesNoCancel);
                    //    if (results == DialogResult.No) continue;
                    //    if (results == DialogResult.Cancel) return;
                    //    else Lollipop.input_files = Lollipop.input_files.Where(h => h.purpose == Purpose.Calibration || h.filename != filename).ToList();
                    //}

                    reload_dgvs();

                    InputFile file = new InputFile(path, filename, extension, label, purpose);
                    Lollipop.input_files.Add(file);
                }
            }
        }

        private void match_files() //for dgv
        {
            // Look for results files with the same filename as a calibration file, and show that they're matched
            if (Lollipop.td_results)
            {
                foreach (InputFile file in Lollipop.raw_files())
                    if (Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile).Select(f => f.filename).Contains(file.filename))
                    {
                        IEnumerable<InputFile> matching_files = Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile && f.filename == file.filename);
                        InputFile matching_file = matching_files.First();
                        if (matching_files.Count() != 1) MessageBox.Show("Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + ".");
                        file.matchingCalibrationFile = true;
                        matching_file.matchingCalibrationFile = true;
                    }
            }
            else
            {
                foreach (InputFile file in Lollipop.calibration_files())
                    if (Lollipop.input_files.Where(f => f.purpose != Purpose.Calibration).Select(f => f.filename).Contains(file.filename))
                    {
                        IEnumerable<InputFile> matching_files = Lollipop.input_files.Where(f => f.purpose != Purpose.Calibration  && f.filename == file.filename);
                        InputFile matching_file = matching_files.First();
                        if (matching_files.Count() != 1) MessageBox.Show("Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + ".");
                        file.matchingCalibrationFile = true;
                        matching_file.matchingCalibrationFile = true;
                    }
            }
            refresh_dgvs();

            if (Lollipop.calibration_files().Count() > 0 && !Lollipop.calibration_files().Any(f => f.matchingCalibrationFile))
                MessageBox.Show("To use calibration files, please give them the same filenames as the deconvolution results to which they correspond.", "Orphaned Calibration Files", MessageBoxButtons.OK);
        }


        private void refresh_dgvs()
        {
            foreach (DataGridView dgv in new List<DataGridView> { dgv_identificationFiles, dgv_quantitationFiles, dgv_calibrationFiles })
            {
                dgv.Refresh();
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.identification_files());
            DisplayUtility.FillDataGridView(dgv_quantitationFiles, Lollipop.quantification_files());
            DisplayUtility.FillDataGridView(dgv_calibrationFiles, Lollipop.calibration_files());
            DisplayUtility.FillDataGridView(dgv_buFiles, Lollipop.bottomup_files());
            DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.topdown_files());
        }


        // CELL FORMATTING EVENTS
        private void dgv_deconResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //MessageBox.Show("cell formatting" + dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName.ToString());
            if ((dgv_identificationFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_identificationFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_identificationFiles.Rows[e.RowIndex].DataBoundItem, dgv_identificationFiles.Columns[e.ColumnIndex].DataPropertyName);
        }
        private void dgv_quantResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //MessageBox.Show("cell formatting" + dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName.ToString());
            if ((dgv_quantitationFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_quantitationFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_quantitationFiles.Rows[e.RowIndex].DataBoundItem, dgv_quantitationFiles.Columns[e.ColumnIndex].DataPropertyName);
        }
        private void dgv_calibrationResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //MessageBox.Show("cell formatting" + dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName.ToString());
            if ((dgv_calibrationFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_calibrationFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_calibrationFiles.Rows[e.RowIndex].DataBoundItem, dgv_calibrationFiles.Columns[e.ColumnIndex].DataPropertyName);
        }

        private string BindProperty(object property, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                PropertyInfo[] arrayProperties = property.GetType().GetProperties();
                string firstPropertyName = propertyName.Substring(0, propertyName.IndexOf("."));
                PropertyInfo firstProperty = arrayProperties.Where(p => p.Name == firstPropertyName).First();
                return BindProperty(firstProperty.GetValue(property, null), propertyName.Substring(propertyName.IndexOf(".") + 1));
            }
            else
            {
                Type propertyType = property.GetType();
                PropertyInfo propertyInfo = propertyType.GetProperty(propertyName);
                return propertyInfo.GetValue(property, null).ToString();
            }
        }
        // ADD BUTTONS
        private void btn_protIdResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".xlsx" }, Purpose.Identification);

            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.identification_files());
            match_files();
        }
        private void btn_protQuantResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".xlsx" }, Purpose.Quantification);

            DisplayUtility.FillDataGridView(dgv_quantitationFiles, Lollipop.quantification_files());
            match_files();
        }
        private void btn_protCalibResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (Lollipop.td_results)
            {
                openFileDialog1.Title = "My Thermo Raw Files";
                openFileDialog1.Filter = "Raw Files (*.raw) | *.raw";
            }
            else
            {
                openFileDialog1.Title = "My Calibration Results Files";
                openFileDialog1.Filter = "Text Files (*.tsv) | *.tsv";
            }

            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                if (Lollipop.td_results)
                {
                    enter_input_files(openFileDialog1.FileNames, new List<string> { ".raw" }, Purpose.RawFile);
                    DisplayUtility.FillDataGridView(dgv_calibrationFiles, Lollipop.raw_files());

                }
                else
                {
                    enter_input_files(openFileDialog1.FileNames, new List<string> { ".tsv", ".txt" }, Purpose.Calibration);
                    DisplayUtility.FillDataGridView(dgv_calibrationFiles, Lollipop.calibration_files());

                }


            match_files();
        }
        private void bt_morpheusBUResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Morpheus Bottom-Up Results Files";
            openFileDialog1.Filter = "Text Files (*.tsv) | *.tsv";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".tsv" }, Purpose.BottomUp);

            DisplayUtility.FillDataGridView(dgv_buFiles, Lollipop.bottomup_files());
        }
        private void bt_tdResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "ProSight Top-Down Results Files";
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".xlsx" }, Purpose.TopDown);
            int i = Lollipop.input_files.Where(f => f.purpose == Purpose.TopDown).ToList().Count;
            if (i == 1) DisplayUtility.EditInputFileDGVs(dgv_tdFiles, Purpose.TopDown);  //adds drop-down column for dataresults first time td file added
            DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.topdown_files());
            for (int j = 0; j < i; j++) { InputFile input = (InputFile)dgv_tdFiles.Rows[j].DataBoundItem; dgv_tdFiles.Rows[j].Cells[0].Value = input.td_software; } //display
            if (Lollipop.ptmlist_filepath.Length == 0) get_ptm_list();
            ProteomeDatabaseReader.oldPtmlistFilePath = Lollipop.ptmlist_filepath;
            Lollipop.uniprotModificationTable = Lollipop.proteomeDatabaseReader.ReadUniprotPtmlist(); //need for reading in TD hit PTMs
        }

        // CLEAR BUTTONS
        private void btn_protIdResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.identification_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.identification_files());
            match_files();
        }
        private void btn_protQuantResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.quantification_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_quantitationFiles, Lollipop.quantification_files());
            match_files();
        }
        private void btn_protCalibResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.calibration_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_calibrationFiles, Lollipop.calibration_files());
            match_files();
        }
        private void bt_morpheusBUResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.bottomup_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_buFiles, Lollipop.bottomup_files());
            match_files();
        }
        private void bt_tdResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.topdown_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.topdown_files());
            match_files();
        }


        // FULL RUN
        private void btn_fullRun_Click(object sender, EventArgs e)
        {
            if (Lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.");
                return;
            }
            MessageBox.Show("Will start the run now.\n\nWill show as non-responsive.");
            bool successful_run = ((ProteoformSweet)MdiParent).full_run();
            if (successful_run) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.");
            else { MessageBox.Show("Method did not successfully run."); }
        }


        // INFO BUTTONS
        private void btn_nextPane_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "The Results menu is what you're looking for.\n\n" +
                "Stepping through each form will process the data one step at a time (this one is \"Load Deconvolution Results\"). " +
                "This should help give you an idea of what this program does and what settings you would like to use.\n\nOn the other hand, you have the \"Full Run with Defaults\" button, " +
                "which plows through each of those processing steps, after which you can view the results in those forms.\n\nBelow, there's another info-button for using presets for a full run.\n\n" +
                "We hope you enjoy trying Proteoform Suite! Please contact us if you have any questions. The public repository for this program is hosted on GitHub at https://github.com/smith-chem-wisc/proteoform-suite.", "How To Process Results", MessageBoxButtons.OK);
            ((ProteoformSweet)MdiParent).display_resultsMenu();
        }

        private void btn_fullRunWithPresets_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "The Method menu is what you're looking for.\n\n" +
                "You can save your current presets (or just the defaults).\n\nYou can also \"Load & Run\" presets in a method file and run through all processing steps. If you specify files both here and in the method file, we give you the choice of which to use.\n\n" +
                "Above is another info-button on how to process results without using presets.\n\n" +
                "We hope you enjoy trying Proteoform Suite! Please contact us if you have any questions. The public repository for this program is hosted on GitHub at https://github.com/smith-chem-wisc/proteoform-suite.", "How To Use Presets.", MessageBoxButtons.OK);
            ((ProteoformSweet)MdiParent).display_methodMenu();
        }

        private void cb_td_file_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.td_results = cb_td_file.Checked;
            label3.Text = "Thermo Raw Files (.raw)";
        }

        private void bt_clearResults_Click(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).clear_lists();
        }

        private void cb_run_when_load_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformSweet.run_when_form_loads = cb_run_when_load.Checked;
        }

        private void cb_calibrate_td_results_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_td_results = cb_calibrate_td_results.Checked;
            if (Lollipop.calibrate_td_results)
            {
                if (!Lollipop.input_files.Any(f => f.purpose == Purpose.TopDown))
                {
                    MessageBox.Show("Load in top-down results to calibrate deconvolution results.");
                    cb_calibrate_td_results.Checked = false;
                    Lollipop.calibrate_td_results = false;
                }
                else
                {
                    Lollipop.td_results = true;
                    Lollipop.calibrate_td_results = cb_calibrate_td_results.Checked;
                }
            }
        }

        private void get_ptm_list()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            MessageBox.Show("Please select a Uniprot ptm list.");
            DialogResult dr = ofd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string ptm_list = ofd.FileName;
                Lollipop.ptmlist_filepath = ptm_list;
            }
            else if (dr == DialogResult.Cancel) return;
            else return;
        }

        private void cb_advanced_user_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_advanced_user.Checked)
            {
                dgv_calibrationFiles.Visible = true;
                btn_protCalibResultsAdd.Visible = true;
                btn_protCalibResultsClear.Visible = true;
                dgv_buFiles.Visible = true;
                bt_morpheusBUResultsAdd.Visible = true;
                bt_morpheusBUResultsClear.Visible = true;
                label3.Visible = true;
                label5.Visible = true;
                cb_td_file.Visible = true; 
                dgv_tdFiles.Visible = true;
                bt_tdResultsAdd.Visible = true;
                bt_tdResultsClear.Visible = true;
                label4.Visible = true;
                cb_calibrate_td_results.Visible = true;

            }
            else
            {
                dgv_calibrationFiles.Visible = false;
                btn_protCalibResultsAdd.Visible = false;
                btn_protCalibResultsClear.Visible = false;
                dgv_buFiles.Visible = false;
                bt_morpheusBUResultsAdd.Visible = false;
                bt_morpheusBUResultsClear.Visible = false;
                label3.Visible = false;
                label5.Visible = false;
                cb_td_file.Visible = false;
                 dgv_tdFiles.Visible = false;
                bt_tdResultsAdd.Visible = false;
                bt_tdResultsClear.Visible = false;
                label4.Visible = false;
                cb_calibrate_td_results.Visible = false;
            }

        }

        // FILTERS
        private void tb_identificationFilter_TextChanged_1(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_identificationFiles, ExtensionMethods.filter(Lollipop.identification_files(), tb_identificationFilter.Text));
        }

        private void tb_quantFilter_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_quantitationFiles, ExtensionMethods.filter(Lollipop.identification_files(), tb_identificationFilter.Text));
        }

    }
}
