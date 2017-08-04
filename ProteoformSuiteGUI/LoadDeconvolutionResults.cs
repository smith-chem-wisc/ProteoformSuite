using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
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

        #region Public Methods

        public void InitializeParameterSet()
        {
            tb_resultsFolder.Text = Sweet.lollipop.results_folder;
            rb_neucode.Checked = Sweet.lollipop.neucode_labeled;
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(Sweet.lollipop.neucode_labeled);
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView> { dgv_loadFiles1, dgv_loadFiles2, dgv_loadFiles3 };
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.input_files.Clear();
            Sweet.actions.Clear();
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

        public void RunTheGamut()
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

            match_files();
            populate_file_lists();
        }

        private void rb_unlabeled_CheckedChanged(object sender, EventArgs e)
        { }

        private void match_files()
        {
            string message = Sweet.lollipop.match_calibration_files();
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
            for (int i = 0; i < 4; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 4; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 4; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);
            cmb_loadTable1.SelectedIndex = 0;
            cmb_loadTable2.SelectedIndex = 1;
            cmb_loadTable3.SelectedIndex = 2;

            if (rb_chemicalCalibration.Checked)
            {
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 1;
                cmb_loadTable3.SelectedIndex = 3;
            }

            lb_filter1.Text = Lollipop.file_lists[cmb_loadTable1.SelectedIndex];
            lb_filter2.Text = Lollipop.file_lists[cmb_loadTable2.SelectedIndex];
            lb_filter3.Text = Lollipop.file_lists[cmb_loadTable3.SelectedIndex];

            dgv_loadFiles1.Refresh();
            dgv_loadFiles2.Refresh();
            dgv_loadFiles3.Refresh();
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
            Sweet.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Sweet.lollipop.input_files, true);
            match_files();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
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
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]).Select(f => new DisplayInputFile(f)));
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
                Sweet.lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Sweet.lollipop.input_files, true);
                match_files();
            }

            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
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
            Sweet.lollipop.input_files = Sweet.lollipop.input_files.Except(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex])).ToList();
            match_files();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
        }
        #endregion CLEAR BUTTONS Private Methods

        #region FULL RUN & STEP THROUGH Private Methods

        private void btn_fullRun_Click(object sender, EventArgs e)
        {
            bool successful_run = ((ProteoformSweet)MdiParent).full_run();
            if (successful_run) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.", "Full Run");
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
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[cmb_loadTable1.SelectedIndex]);
        }

        private void tb_filter2_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]), tb_filter2.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[cmb_loadTable2.SelectedIndex]);
        }

        private void tb_filter3_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]), tb_filter3.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[cmb_loadTable3.SelectedIndex]);
        }
        #endregion FILTERS Private Methods

        #region CHANGED TABLE SELECTION Private Methods

        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable1.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[cmb_loadTable1.SelectedIndex]);
            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_loadTable2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable2.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles2, Lollipop.file_types[cmb_loadTable2.SelectedIndex]);
            lb_filter2.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_loadTable3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable3.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles3, Lollipop.file_types[cmb_loadTable3.SelectedIndex]);
            lb_filter3.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        //Do nothing when text changes
        private void cmb_loadTable1_TextChanged(object sender, EventArgs e) { }
        private void cmb_loadTable2_TextChanged(object sender, EventArgs e) { }
        private void cmb_loadTable3_TextChanged(object sender, EventArgs e) { }


        #endregion CHANGED TABLE SELECTION Private Methods

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
            if (e.FormattedValue.ToString() == "")
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

    }
}
