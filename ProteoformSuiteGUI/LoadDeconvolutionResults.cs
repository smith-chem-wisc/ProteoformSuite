using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class LoadDeconvolutionResults : Form, ISweetForm
    {

        #region Public Constructor

        public LoadDeconvolutionResults()
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
            tb_resultsFolder.Text = Sweet.lollipop.results_folder;
            rb_neucode.Checked = Sweet.lollipop.neucode_labeled;
            rb_unlabeled.Checked = !rb_neucode.Checked;
            nud_randomSeed.Value = Sweet.lollipop.calibration_random_seed;
            cb_useRandomSeed.Checked = Sweet.lollipop.calibration_use_random_seed;
            cb_calibrate_td_files.Checked = Sweet.lollipop.calibrate_td_files;
            cb_calibrate_raw_files.Checked = Sweet.lollipop.calibrate_raw_files;
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(Sweet.lollipop.neucode_labeled);
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
            tb_resultsFolder.Text = "";
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

        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();
            cmb_loadTable2.Items.Clear();
            cmb_loadTable3.Items.Clear();

            if (rb_standardOptions.Checked)
            {
                cmb_loadTable1.Items.Add(Lollipop.file_lists[0]);
                cmb_loadTable2.Items.Add(Lollipop.file_lists[0]);
                cmb_loadTable3.Items.Add(Lollipop.file_lists[0]);

                //if unlabeled add quant
             //   if (Sweet.lollipop.neucode_labeled)
                {
                    cmb_loadTable1.Items.Add(Lollipop.file_lists[1]);
                    cmb_loadTable2.Items.Add(Lollipop.file_lists[1]);
                    cmb_loadTable3.Items.Add(Lollipop.file_lists[1]);
                }

                for (int i = 2; i < 5; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                for (int i = 2; i < 5; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
                for (int i = 2; i < 5; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);

                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 1;
                cmb_loadTable3.SelectedIndex = 2;

                cmb_loadTable1.Enabled = true;
                cmb_loadTable2.Enabled = true;
                cmb_loadTable3.Enabled = true;


                bt_calibrate.Visible = false;
                cb_useRandomSeed.Visible = false;
                cb_calibrate_raw_files.Visible = false;
                cb_calibrate_td_files.Visible = false;
                nud_randomSeed.Visible = false;
                bt_deconvolute.Visible = false;
                nud_maxcharge.Visible = false;
                nud_maxmass.Visible = false;
                nud_mincharge.Visible = false;
                nud_minmass.Visible = false;
                nud_maxRT.Visible = false;
                nud_minRT.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }

            else if (rb_chemicalCalibration.Checked)
            {
                for (int i = 4; i < 7; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                for (int i = 4; i < 7; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
                for (int i = 4; i < 7; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);
                bt_calibrate.Visible = true;
                cb_useRandomSeed.Visible = true;
                cb_calibrate_td_files.Visible = true;
                cb_calibrate_raw_files.Visible = true;
                nud_randomSeed.Visible = true;
                bt_deconvolute.Visible = false;
                nud_maxcharge.Visible = false;
                nud_maxmass.Visible = false;
                nud_mincharge.Visible = false;
                nud_minmass.Visible = false;
                nud_maxRT.Visible = false;
                nud_minRT.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;

                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 1;
                cmb_loadTable3.SelectedIndex = 2;

                cmb_loadTable1.Enabled = false;
                cmb_loadTable2.Enabled = false;
                cmb_loadTable3.Enabled = false;
            }

            else if (rb_deconvolution.Checked)
            {

                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);
                cmb_loadTable2.Items.Add(Lollipop.file_lists[4]);
                cmb_loadTable3.Items.Add(Lollipop.file_lists[4]);

                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 0;
                cmb_loadTable3.SelectedIndex = 0;

                cmb_loadTable1.Enabled = false;
                cmb_loadTable2.Enabled = false;
                cmb_loadTable3.Enabled = false;

                bt_calibrate.Visible = false;
                cb_useRandomSeed.Visible = false;
                cb_calibrate_raw_files.Visible = false;
                cb_calibrate_td_files.Visible = false;
                nud_randomSeed.Visible = false;
                bt_deconvolute.Visible = true;
                nud_maxcharge.Visible = true;
                nud_maxmass.Visible = true;
                nud_mincharge.Visible = true;
                nud_minmass.Visible = true;
                nud_maxRT.Visible = true;
                nud_minRT.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
            }

            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
            lb_filter2.Text = cmb_loadTable2.SelectedItem.ToString();
            lb_filter3.Text = cmb_loadTable3.SelectedItem.ToString();

            reload_dgvs();
            refresh_dgvs();
        }

        #endregion General Table Option Private Methods

        #region DGV DRAG AND DROP Private Methods

        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_quantResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable2, dgv_loadFiles2);
        }

        private void dgv_calibrationResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable3, dgv_loadFiles3);
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
            foreach (DataGridView dgv in new List<DataGridView> { dgv_loadFiles1, dgv_loadFiles2, dgv_loadFiles3 })
            {
                dgv.Refresh();
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable2.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable3.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]);
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable2.Text)]);
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable3.Text)]);
        }
        #endregion DGV DRAG AND DROP Private Methods

        #region CELL FORMATTING Private Methods

        private void dgv_loadFiles1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem != null) && e.ColumnIndex >= 0 && (dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName);
        }

        private void dgv_loadFiles2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles2.Rows[e.RowIndex].DataBoundItem != null) && e.ColumnIndex >= 0 && (dgv_loadFiles2.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles2.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles2.Columns[e.ColumnIndex].DataPropertyName);
        }

        private void dgv_loadFiles3_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles3.Rows[e.RowIndex].DataBoundItem != null) && e.ColumnIndex >= 0 && (dgv_loadFiles3.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles3.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles3.Columns[e.ColumnIndex].DataPropertyName);
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

        private void btn_addFiles2_Click(object sender, EventArgs e)
        {
            add_files(cmb_loadTable2, dgv_loadFiles2);
        }

        private void btn_addFiles3_Click(object sender, EventArgs e)
        {
            add_files(cmb_loadTable3, dgv_loadFiles3);
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
            }

            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        #endregion ADD BUTTONS Private Methods

        #region CLEAR BUTTONS Private Methods
        private void btn_clearFiles1_Click(object sender, EventArgs e)
        {
            clear_files(cmb_loadTable1, dgv_loadFiles1);
        }

        private void btn_clearFiles2_Click(object sender, EventArgs e)
        {
            clear_files(cmb_loadTable2, dgv_loadFiles2);
        }

        private void btn_clearFiles3_Click(object sender, EventArgs e)
        {
            clear_files(cmb_loadTable3, dgv_loadFiles3);
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

        private void btn_stepThrough_Click(object sender, EventArgs e)
        {
            (MdiParent as ProteoformSweet).resultsToolStripMenuItem.ShowDropDown();
            MessageBox.Show("Use the Results menu to step through processing results.\n\n" + 
                "Load results and databases in this panel, and then proceed to Raw Experimental Components.", "Step Through Introduction.");
        }

        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
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

        private void tb_filter2_TextChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable2.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles2, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter2.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[selected_index]);
        }

        private void tb_filter3_TextChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable3.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles3, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter3.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[selected_index]);
        }
        #endregion FILTERS Private Methods

        #region CHANGED TABLE SELECTION Private Methods

        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[selected_index]);
            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_loadTable2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable2.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[selected_index]);
            lb_filter2.Text = cmb_loadTable2.SelectedItem.ToString();
        }

        private void cmb_loadTable3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable3.Text);
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[selected_index]);
            lb_filter3.Text = cmb_loadTable3.SelectedItem.ToString();
        }

        private void bt_calibrate_Click(object sender, EventArgs e)
        {
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).Count() == 0)
            {
                MessageBox.Show("Please enter raw files to calibrate."); return;
            }
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
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).Count() == 0)
            {
                MessageBox.Show("Please enter raw files to deconvolute."); return;
            }
            if(Convert.ToInt32(nud_maxcharge.Value) > 60 || Convert.ToInt32(nud_mincharge.Value) < 1)
            {
                MessageBox.Show("Charge range must be within 1-60."); return;
            }
            if(Convert.ToInt32(nud_maxmass.Value) > 100000 || Convert.ToInt32(nud_minmass.Value) < 2000)
            {
                MessageBox.Show("Mass range must be within 2000-100000."); return;
            }
            if (Convert.ToInt32(nud_maxRT.Value) > 100 || Convert.ToInt32(nud_minRT.Value) < 0)
            {
                MessageBox.Show("Retention time must be within 0-100."); return;
            }

            string deconv_results = Sweet.lollipop.promex_deconvolute(Convert.ToInt32(nud_maxcharge.Value), Convert.ToInt32(nud_mincharge.Value), Convert.ToInt32(nud_maxmass.Value), Convert.ToInt32(nud_minmass.Value), Convert.ToInt32(nud_maxRT.Value), Convert.ToInt32(nud_minRT.Value), Environment.CurrentDirectory);
            MessageBox.Show(deconv_results);
        }

        #endregion CHANGED TABLE SELECTION Private Methods

        //Do nothing when text changes
        private void cmb_loadTable1_TextChanged(object sender, EventArgs e) { }
        private void cmb_loadTable2_TextChanged(object sender, EventArgs e) { }
        private void cmb_loadTable3_TextChanged(object sender, EventArgs e) { }

        #region CHANGE ALL CELLS private methods

        private void dgv_loadFiles1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                change_all_selected_cells(dgv_loadFiles1);
        }

        private void dgv_loadFiles2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                change_all_selected_cells(dgv_loadFiles2);
        }

        private void dgv_loadFiles3_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                change_all_selected_cells(dgv_loadFiles3);
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

            void tb_enter(object sender, EventArgs e)
            {
                this.AcceptButton = okay;
            }

            void okay_click(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            void cancel_click(object sender, EventArgs e)
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

        #endregion

        #region Cell Validation Methods

        private void dgv_loadFiles1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            validate(dgv_loadFiles1, e);
        }

        private void dgv_loadFiles2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            validate(dgv_loadFiles2, e);
        }

        private void dgv_loadFiles3_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            validate(dgv_loadFiles3, e);
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

        private void nud_randomSeed_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibration_random_seed = (int)nud_randomSeed.Value;
        }

        private void cb_useRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibration_use_random_seed = cb_useRandomSeed.Checked;
        }

        private void cb_calibrate_raw_files_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibrate_raw_files = cb_calibrate_raw_files.Checked;
        }

        private void cb_calibrate_td_files_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.calibrate_td_files = cb_calibrate_td_files.Checked;
        }
    }
}
