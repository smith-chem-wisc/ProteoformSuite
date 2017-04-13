using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class LoadDeconvolutionResults : Form
    {

        #region Public Constructor

        public LoadDeconvolutionResults()
        {
            InitializeComponent();
            populate_file_lists();
            ProteoformSweet.run_when_form_loads = cb_run_when_load.Checked;
        }

        #endregion Public Constructor

        #region GENERAL TABLE OPTIONS Private Methods

        private void btn_neucode_CheckedChanged(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(rb_neucode.Checked);
            Lollipop.neucode_labeled = rb_neucode.Checked;
            Lollipop.neucode_light_lysine = rb_neucode.Checked;
            Lollipop.natural_lysine_isotope_abundance = !rb_neucode.Checked;

            foreach (InputFile f in Lollipop.input_files)
            {
                if (rb_neucode.Checked) f.label = Labeling.NeuCode;
                if (rb_unlabeled.Checked) f.label = Labeling.Unlabeled;
            }

        }

        private void match_files()
        {
            string message = Lollipop.match_calibration_files();
            refresh_dgvs();
            if (message != "")
                MessageBox.Show(message);
        }

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

        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();
            cmb_loadTable2.Items.Clear();
            cmb_loadTable3.Items.Clear();
            for (int i = 0; i < 8; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 8; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 8; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);
            cmb_loadTable1.SelectedIndex = 0;
            cmb_loadTable2.SelectedIndex = 1;
            cmb_loadTable3.SelectedIndex = 2;
            bt_calibrate.Visible = false;
            cb_lockmass.Visible = false;
            cb_tdhits.Visible = false;
            cb_td_hits_diff_file.Visible = false;

            if (rb_chemicalCalibration.Checked)
            {
                bt_calibrate.Visible = true;
                cb_lockmass.Visible = true;
                cb_tdhits.Visible = true;
                cb_td_hits_diff_file.Visible = true;
                cmb_loadTable1.SelectedIndex = 3;
                cmb_loadTable2.SelectedIndex = 4;
                cmb_loadTable3.SelectedIndex = 5;
            }

            else if (rb_advanced_user.Checked)
            {
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 6;
                cmb_loadTable3.SelectedIndex = 2;
            }

            lb_filter1.Text = Lollipop.file_lists[cmb_loadTable1.SelectedIndex];
            lb_filter2.Text = Lollipop.file_lists[cmb_loadTable2.SelectedIndex];
            lb_filter3.Text = Lollipop.file_lists[cmb_loadTable3.SelectedIndex];
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
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Lollipop.input_files);
            match_files();
            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
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
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[cmb_loadTable1.SelectedIndex]);
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[cmb_loadTable2.SelectedIndex]);
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[cmb_loadTable3.SelectedIndex]);
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = cmb.SelectedItem.ToString();
            openFileDialog.Filter = Lollipop.file_filters[cmb.SelectedIndex];
            openFileDialog.Multiselect = true;

            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Lollipop.input_files);
                match_files();
            }

            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
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
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex])).ToList();
            match_files();
            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
        }
        #endregion CLEAR BUTTONS Private Methods

        #region FULL RUN Private Methods

        private void btn_fullRun_Click(object sender, EventArgs e)
        {
            if (Lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.", "Full Run");
                return;
            }
            bool successful_run = ((ProteoformSweet)MdiParent).full_run();
            if (successful_run) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.", "Full Run");
            else MessageBox.Show("Method did not successfully run.", "Full Run");
        }
        private void bt_clearResults_Click(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).clear_lists();
        }

        private void cb_run_when_load_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformSweet.run_when_form_loads = cb_run_when_load.Checked;
        }
        #endregion FULL RUN Private Methods

        #region FILTERS Private Methods

        private void tb_filter1_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[cmb_loadTable1.SelectedIndex]);
        }

        private void tb_filter2_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, ExtensionMethods.filter(Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]), tb_filter2.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[cmb_loadTable2.SelectedIndex]);
        }

        private void tb_filter3_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, ExtensionMethods.filter(Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]), tb_filter3.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[cmb_loadTable3.SelectedIndex]);
        }
        #endregion FILTERS Private Methods

        #region CHANGED TABLE SELECTION Private Methods

        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[cmb_loadTable1.SelectedIndex]);
            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_loadTable2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[cmb_loadTable2.SelectedIndex]);
            lb_filter2.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_LoadTable3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[cmb_loadTable3.SelectedIndex]);
            lb_filter3.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void bt_calibrate_Click(object sender, EventArgs e)
        {
            if (!cb_tdhits.Checked && !cb_lockmass.Checked && !cb_td_hits_diff_file.Checked) { MessageBox.Show("Please select at least one calibration method."); return; }
            if (cb_tdhits.Checked || cb_td_hits_diff_file.Checked)
            {
                if (Lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationTopDown).Count() > 0) Lollipop.read_in_calibration_td_hits();
                else { MessageBox.Show("Please enter top-down results files to calibrate."); return; }
            }
            if (Lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).Count() > 0)
            {
              Lollipop.calibrate_files();
              MessageBox.Show("Successfully calibrated files.");
            }
            else { MessageBox.Show("Please enter raw files to calibrate."); return; }
        }

        private void cb_tdhits_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_td_results = cb_tdhits.Checked;
            if (cb_tdhits.Checked) cb_td_hits_diff_file.Checked = false;
        }

        private void cb_lockmass_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_lock_mass = cb_lockmass.Checked;
        }

        private void cb_td_hits_diff_file_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_intact_with_td_ids = cb_td_hits_diff_file.Checked;
            if (cb_td_hits_diff_file.Checked) cb_tdhits.Checked = false;
        }
        #endregion CHANGED TABLE SELECTION Private Methods
    }
}
