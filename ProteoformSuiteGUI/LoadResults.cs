using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class LoadResults : Form, ISweetForm
    {
        #region Public Constructor

        public LoadResults()
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
            populate_file_lists();
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public void InitializeParameterSet()
        {
            //tb_resultsFolder.Text = Sweet.lollipop.results_folder;
            rb_neucode.Checked = Sweet.lollipop.neucode_labeled;
            rb_unlabeled.Checked = !rb_neucode.Checked;
            cb_calibrate_td_files.Checked = Sweet.lollipop.calibrate_td_files;
            cb_calibrate_raw_files.Checked = Sweet.lollipop.calibrate_raw_files;
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(Sweet.lollipop.neucode_labeled);
            ((ProteoformSweet)MdiParent).enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
            ((ProteoformSweet)MdiParent).enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>();
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(x => new DisplayInputFile(x)).ToList(), "IdentificationFiles", new List<Purpose> { Purpose.Identification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationIdentification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationIdentification).Select(x => new DisplayInputFile(x)).ToList(), "CalibrationIdentificationFiles", new List<Purpose> { Purpose.CalibrationIdentification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationTopDown).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationTopDown).Select(x => new DisplayInputFile(x)).ToList(), "CalibrationTopDownFiles", new List<Purpose> { Purpose.CalibrationTopDown }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.TopDown).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.TopDown).Select(x => new DisplayInputFile(x)).ToList(), "TopDown", new List<Purpose> { Purpose.TopDown }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.BottomUp).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.BottomUp).Select(x => new DisplayInputFile(x)).ToList(), "BottomUp", new List<Purpose> { Purpose.BottomUp }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Select(x => new DisplayInputFile(x)).ToList(), "QuantificationFiles", new List<Purpose> { Purpose.Quantification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Select(x => new DisplayInputFile(x)).ToList(), "ProteinDatabases", new List<Purpose> { Purpose.ProteinDatabase, Purpose.PtmList }));
            return DataTables;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.input_files.Clear();
            Sweet.save_actions.Clear();
            Sweet.loaded_actions.Clear();
            Sweet.lollipop.results_folder = "";
            //tb_resultsFolder.Text = "";
            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    sweet.ClearListsTablesFigures(false);
                    sweet.FillTablesAndCharts();
                }
            }
            FillTablesAndCharts();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            populate_file_lists();
        }

        public void FillTablesAndCharts()
        {
            populate_file_lists();
        }

        #endregion Public Methods

        #region GENERAL TABLE OPTIONS Private Methods

        private void rb_neucode_CheckedChanged(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(rb_neucode.Checked);
            Sweet.lollipop.neucode_labeled = rb_neucode.Checked;
            Sweet.lollipop.neucode_light_lysine = rb_neucode.Checked;
            Sweet.lollipop.natural_lysine_isotope_abundance = !rb_neucode.Checked;

            foreach (InputFile f in Sweet.lollipop.input_files)
            {
                if (rb_neucode.Checked) f.label = Labeling.NeuCode;
                if (rb_unlabeled.Checked) f.label = Labeling.Unlabeled;
            }
            populate_file_lists();
        }

        private void rb_unlabeled_CheckedChanged(object sender, EventArgs e)
        { }

        private void rb_standardOptions_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_chemicalCalibration_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_advanced_user_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_deconvolution_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();

            if (rb_standardOptions.Checked)
            {
                for (int i = 0; i < 4; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = true;

                bt_calibrate.Visible = false;
                cb_calibrate_raw_files.Visible = false;
                cb_calibrate_td_files.Visible = false;
                bt_deconvolute.Visible = false;
                bt_stepthru.Visible = true;
                bt_fullrun.Visible = true;
                bt_calibrate.Visible = false;
                panel_deconv_calib.Visible = false;
                panel_step.Visible = true;
                nud_maxcharge.Visible = false;
                nud_mincharge.Visible = false;
                label_maxcharge.Visible = false;
                label_mincharge.Visible = false;
                label_maxRT.Visible = false;
                label_minRT.Visible = false;
                rb_neucode.Visible = true;
                rb_unlabeled.Visible = true;
                calib_stand_splitContainer.Visible = true;
                fullrun_groupbox.Visible = true;
            }
            else if (rb_chemicalCalibration.Checked)
            {
                for (int i = 4; i < 7; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                bt_calibrate.Visible = true;
                cb_calibrate_td_files.Visible = true;
                cb_calibrate_raw_files.Visible = true;
                bt_deconvolute.Visible = false;
                bt_stepthru.Visible = false;
                bt_fullrun.Visible = false;
                bt_calibrate.Visible = true;
                panel_deconv_calib.Visible = true;
                panel_step.Visible = false;
                nud_maxcharge.Visible = false;
                nud_mincharge.Visible = false;
                label_maxcharge.Visible = false;
                label_mincharge.Visible = false;
                label_maxRT.Visible = false;
                label_minRT.Visible = false;
                rb_neucode.Visible = true;
                rb_unlabeled.Visible = true;
                calib_stand_splitContainer.Visible = true;
                fullrun_groupbox.Visible = false;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = true;
            }
            else if (rb_deconvolution.Checked)
            {
                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = false;

                bt_calibrate.Visible = false;
                cb_calibrate_raw_files.Visible = false;
                cb_calibrate_td_files.Visible = false;
                bt_stepthru.Visible = false;
                bt_fullrun.Visible = false;
                bt_calibrate.Visible = false;
                bt_deconvolute.Visible = true;
                panel_deconv_calib.Visible = true;
                panel_step.Visible = false;
                nud_maxcharge.Visible = true;
                nud_mincharge.Visible = true;
                label_maxcharge.Visible = true;
                label_mincharge.Visible = true;
                label_maxRT.Visible = true;
                label_minRT.Visible = true;
                rb_neucode.Visible = false;
                rb_unlabeled.Visible = false;
                calib_stand_splitContainer.Visible = false;
                fullrun_groupbox.Visible = false;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = false;
            }

            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();

            reload_dgvs();
            refresh_dgvs();
        }

        #endregion GENERAL TABLE OPTIONS Private Methods

        #region DGV DRAG AND DROP Private Methods

        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_quantResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_calibrationResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
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

        private void drag_drop(DragEventArgs e, ComboBox cmb, DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DisplayUtility.CheckForProteinFastas(cmb, files)) return; // todo: implement protein fasta usage
            Sweet.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[selected_index], Lollipop.file_types[selected_index], Sweet.lollipop.input_files, true);
            refresh_dgvs();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        private void refresh_dgvs()
        {
            foreach (DataGridView dgv in new List<DataGridView> { dgv_loadFiles1 })
            {
                dgv.Refresh();
            }

            if (MdiParent != null) //doesn't work first time
            {
                ((ProteoformSweet)MdiParent).enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
                ((ProteoformSweet)MdiParent).enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]);
        }

        #endregion DGV DRAG AND DROP Private Methods

        #region CELL FORMATTING Private Methods

        private void dgv_loadFiles1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem != null) && e.ColumnIndex >= 0 && (dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName);
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

        #endregion CELL FORMATTING Private Methods

        #region ADD BUTTONS Private Methods

        private void btn_addFiles1_Click(object sender, EventArgs e)
        {
            add_files(cmb_loadTable1, dgv_loadFiles1);
        }

        private void add_files(ComboBox cmb, DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = cmb.SelectedItem.ToString();
            openFileDialog.Filter = Lollipop.file_filters[selected_index];
            openFileDialog.Multiselect = true;

            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (DisplayUtility.CheckForProteinFastas(cmb, openFileDialog.FileNames)) return; // todo: implement protein fasta usage
                Sweet.lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[selected_index], Lollipop.file_types[selected_index], Sweet.lollipop.input_files, true);
                refresh_dgvs();
                if (openFileDialog.FileNames.Any(f => Path.GetExtension(f) == ".raw")) ValidateThermoMsFileReaderVersion();
            }

            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        private const string AssumedThermoMsFileReaderDllPath = @"C:\Program Files\Thermo\MSFileReader";
        private const string DesiredFileIoVersion = "3.0";
        private const string DesiredFregistryVersion = "3.0";
        private const string DesiredXRawFileVersion = "3.0.29.0";

        public static void ValidateThermoMsFileReaderVersion()
        {
            string fileIoAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "Fileio_x64.dll");
            string fregistryAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "fregistry_x64.dll");
            string xRawFileAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "XRawfile2_x64.dll");

            if (File.Exists(fileIoAssumedPath) && File.Exists(fregistryAssumedPath) && File.Exists(xRawFileAssumedPath))
            {
                string fileIoVersion = FileVersionInfo.GetVersionInfo(fileIoAssumedPath).FileVersion;
                string fregistryVersion = FileVersionInfo.GetVersionInfo(fregistryAssumedPath).FileVersion;
                string xRawFileVersion = FileVersionInfo.GetVersionInfo(xRawFileAssumedPath).FileVersion;

                if (fileIoVersion.Equals(DesiredFileIoVersion) && fregistryVersion.Equals(DesiredFregistryVersion) && xRawFileVersion.Equals(DesiredXRawFileVersion))
                {
                    return;
                }
                else
                {
                    MessageBox.Show("Warning!Thermo MSFileReader is not version 3.0 SP2; a crash may result from searching this .raw file.");
                    return;
                }
            }
            MessageBox.Show("Warning! Cannot find Thermo MSFileReader (v3.0 SP2 is preferred); a crash may result from searching this .raw file");
        }

        #endregion ADD BUTTONS Private Methods

        #region CLEAR BUTTONS Private Methods

        private void btn_clearFiles1_Click(object sender, EventArgs e)
        {
            clear_files(cmb_loadTable1, dgv_loadFiles1);
        }

        private void clear_files(ComboBox cmb, DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);
            List<InputFile> files_to_remove = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).ToList();
            Sweet.save_actions.RemoveAll(a => files_to_remove.Any(f => a.Contains(f.complete_path)));
            Sweet.lollipop.input_files = Sweet.lollipop.input_files.Except(files_to_remove).ToList();
            refresh_dgvs();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        #endregion CLEAR BUTTONS Private Methods

        #region FULL RUN & STEP THROUGH Private Methods

        private void btn_fullRun_Click(object sender, EventArgs e)
        {
            Stopwatch successful_run = ((ProteoformSweet)MdiParent).full_run();
            if (successful_run != null) MessageBox.Show("Successfully ran method in "
                + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", successful_run.Elapsed.Hours, successful_run.Elapsed.Minutes, successful_run.Elapsed.Seconds, successful_run.Elapsed.Milliseconds / 10)
                + ". Feel free to explore using the Results menu.", "Full Run");
            else MessageBox.Show("Method did not successfully run.", "Full Run");
        }

        private void bt_clearResults_Click(object sender, EventArgs e)
        {
            Sweet.lollipop = new Lollipop();
            ClearListsTablesFigures(true);
        }

        private void bt_stepthru_Click(object sender, EventArgs e)
        {
            (MdiParent as ProteoformSweet).resultsToolStripMenuItem.ShowDropDown();
            MessageBox.Show("Use the Results menu to step through processing results.\n\n" +
                "Load results and databases in this panel, and then proceed to Raw Experimental Components.", "Step Through Introduction.");
        }

        private FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

        private void btn_browseSummarySaveFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_resultsFolder.Text = temp_folder_path;
                Sweet.lollipop.results_folder = temp_folder_path;
            }
        }

        #endregion FULL RUN & STEP THROUGH Private Methods

        #region FILTERS Private Methods

        private void tb_filter1_TextChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[selected_index]);
        }

        #endregion FILTERS Private Methods

        #region CHANGED TABLE SELECTION Private Methods

        private void bt_calibrate_Click(object sender, EventArgs e)
        {
            //if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            //{
            //    MessageBox.Show("Please enter raw files to calibrate."); return;
            //}
            if (Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
            {
                MessageBox.Show("First create a theoretical proteoform database. On the Results tab, select Theoretical Proteoform Database.");
                return;
            }
            Sweet.lollipop.read_in_calibration_td_hits();
            MessageBox.Show(Sweet.lollipop.calibrate_files());
        }

        private void bt_deconvolute_Click(object sender, EventArgs e)
        {
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            {
                MessageBox.Show("Please enter raw files to deconvolute."); return;
            }
            string deconv_results = Sweet.lollipop.promex_deconvolute(Convert.ToInt32(nud_maxcharge.Value), Convert.ToInt32(nud_mincharge.Value), Environment.CurrentDirectory);
            MessageBox.Show(deconv_results);
        }

        #endregion CHANGED TABLE SELECTION Private Methods

        #region CHANGE ALL CELLS private methods

        private void dgv_loadFiles1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                change_all_selected_cells(dgv_loadFiles1);
        }

        private class InputBox : Form
        {
            private Label lb = new Label();
            public TextBox tb = new TextBox();
            private Button okay = new Button();
            private Button cancel = new Button();

            public InputBox()
            {
                this.Text = "Change Selected";
                lb.Text = "Replace with:";
                okay.Text = "Okay";
                cancel.Text = "Cancel";
                this.Size = new Size(300, 150);
                okay.Size = new Size(150, 50);
                cancel.Size = new Size(150, 50);
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Controls.Add(cancel);
                this.Controls.Add(okay);
                this.Controls.Add(tb);
                this.Controls.Add(lb);
                lb.Dock = DockStyle.Top;
                tb.Dock = DockStyle.Top;
                cancel.Dock = DockStyle.Left;
                okay.Dock = DockStyle.Left;
                okay.Click += new EventHandler(okay_click);
                cancel.Click += new EventHandler(cancel_click);
                tb.Enter += new EventHandler(tb_enter);
                ActiveControl = tb;
            }

            private void tb_enter(object sender, EventArgs e)
            {
                this.AcceptButton = okay;
            }

            private void okay_click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private void cancel_click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void change_all_selected_cells(DataGridView dgv)
        {
            InputBox testdialog = new InputBox();
            if (testdialog.ShowDialog(this) == DialogResult.OK)
            {
                foreach (DataGridViewTextBoxCell cell in dgv.SelectedCells.OfType<DataGridViewTextBoxCell>())
                {
                    cell.Value = testdialog.tb.Text;
                }
            }
            testdialog.Dispose();
        }

        #endregion CHANGE ALL CELLS private methods

        #region Cell Validation Methods

        private void dgv_loadFiles1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            validate(dgv_loadFiles1, e);
        }

        private void validate(DataGridView dgv, DataGridViewCellValidatingEventArgs e)
        {
            if (dgv.Rows[e.RowIndex].IsNewRow)
                return;
            if (e.FormattedValue.ToString() == "" && dgv.IsCurrentCellInEditMode)
            {
                e.Cancel = true;
                MessageBox.Show("Please enter text for each label.");
            }
            if (dgv[e.ColumnIndex, e.RowIndex].ValueType == typeof(int) && (!int.TryParse(e.FormattedValue.ToString(), out int x) || x < 1))
            {
                e.Cancel = true;
                MessageBox.Show("Please use positive integers for biological replicate labels.");
            }
        }

        #endregion Cell Validation Methods

        private void cb_calibrate_raw_files_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibrate_raw_files = cb_calibrate_raw_files.Checked;
        }

        private void cb_calibrate_td_files_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibrate_td_files = cb_calibrate_td_files.Checked;
        }

        private void topbar_splitcontainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }
    }
}