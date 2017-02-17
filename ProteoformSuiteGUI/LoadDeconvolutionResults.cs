﻿using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;


namespace ProteoformSuite
{
    public partial class LoadDeconvolutionResults : Form
    {
        
        public LoadDeconvolutionResults()
        {
            InitializeComponent();
            populate_file_lists();
        }

        public void LoadDeconvolutionResults_Load(object sender, EventArgs e)
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

        private void match_files()
        {
            string message = Lollipop.match_calibration_files();
            refresh_dgvs();
            if (message != "")
                MessageBox.Show(message);
        }


        // CHANGED GENERAL OPTIONS FOR TABLES
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
            for (int i = 0; i < 3; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 3; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
            for (int i = 0; i < 3; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);
            cmb_loadTable1.SelectedIndex = 0;
            cmb_loadTable2.SelectedIndex = 1;
            cmb_loadTable3.SelectedIndex = 2;

            if (rb_chemicalCalibration.Checked)
            {
                cmb_loadTable1.Items.Add(Lollipop.file_lists[3]);
                cmb_loadTable2.Items.Add(Lollipop.file_lists[3]);
                cmb_loadTable3.Items.Add(Lollipop.file_lists[3]);
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = 1;
                cmb_loadTable3.SelectedIndex = cmb_loadTable3.Items.Count - 1;
            }

            else if (rb_advanced_user.Checked)
            {
                for (int i = 3; i < Lollipop.file_lists.Length; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                for (int i = 3; i < Lollipop.file_lists.Length; i++) cmb_loadTable2.Items.Add(Lollipop.file_lists[i]);
                for (int i = 3; i < Lollipop.file_lists.Length; i++) cmb_loadTable3.Items.Add(Lollipop.file_lists[i]);
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable2.SelectedIndex = cmb_loadTable2.Items.Count - 2;
                cmb_loadTable3.SelectedIndex = cmb_loadTable2.Items.Count - 1;
            }

            lb_filter1.Text = Lollipop.file_lists[cmb_loadTable1.SelectedIndex];
            lb_filter2.Text = Lollipop.file_lists[cmb_loadTable2.SelectedIndex];
            lb_filter3.Text = Lollipop.file_lists[cmb_loadTable3.SelectedIndex];
        }


        // DGV DRAG AND DROP EVENTS
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
            Lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex]);
            match_files();
            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.file_types[cmb.SelectedIndex]));
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
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Lollipop.get_files(Lollipop.file_types[cmb_loadTable1.SelectedIndex]));
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Lollipop.get_files(Lollipop.file_types[cmb_loadTable2.SelectedIndex]));
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Lollipop.get_files(Lollipop.file_types[cmb_loadTable3.SelectedIndex]));
        }


        // CELL FORMATTING EVENTS
        private void dgv_loadFiles1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem != null) && (dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName);
        }

        private void dgv_loadFiles2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles2.Rows[e.RowIndex].DataBoundItem != null) && (dgv_loadFiles2.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles2.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles2.Columns[e.ColumnIndex].DataPropertyName);
        }

        private void dgv_loadFiles3_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles3.Rows[e.RowIndex].DataBoundItem != null) && (dgv_loadFiles3.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
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


        // ADD BUTTONS
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
                Lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex]);
                match_files();
            }

            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.file_types[cmb.SelectedIndex]));
        }


        // CLEAR BUTTONS
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
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.get_files(Lollipop.file_types[cmb.SelectedIndex])).ToList();
            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.file_types[cmb.SelectedIndex]));
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
                "The Results menu is what you're looking for.\n\n"+
                "Stepping through each form will process the data one step at a time (this one is \"Load Deconvolution Results\"). "+
                "This should help give you an idea of what this program does and what settings you would like to use.\n\nOn the other hand, you have the \"Full Run with Defaults\" button, "+
                "which plows through each of those processing steps, after which you can view the results in those forms.\n\nBelow, there's another info-button for using presets for a full run.\n\n"+
                "We hope you enjoy trying Proteoform Suite! Please contact us if you have any questions. The public repository for this program is hosted on GitHub at https://github.com/smith-chem-wisc/proteoform-suite.", "How To Process Results", MessageBoxButtons.OK);
            ((ProteoformSweet)MdiParent).display_resultsMenu();
        }

        private void btn_fullRunWithPresets_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "The Method menu is what you're looking for.\n\n"+
                "You can save your current presets (or just the defaults).\n\nYou can also \"Load & Run\" presets in a method file and run through all processing steps. If you specify files both here and in the method file, we give you the choice of which to use.\n\n"+
                "Above is another info-button on how to process results without using presets.\n\n"+
                "We hope you enjoy trying Proteoform Suite! Please contact us if you have any questions. The public repository for this program is hosted on GitHub at https://github.com/smith-chem-wisc/proteoform-suite.", "How To Use Presets.", MessageBoxButtons.OK);
            ((ProteoformSweet)MdiParent).display_methodMenu();
        }

        private void cb_td_file_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.td_results = cb_td_file.Checked;
        }

        private void bt_clearResults_Click(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).clear_lists();
        }

        private void cb_run_when_load_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformSweet.run_when_form_loads = cb_run_when_load.Checked;
        }


        // FILTERS
        private void tb_filter1_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Lollipop.get_files(Lollipop.file_types[cmb_loadTable1.SelectedIndex]), tb_filter1.Text));
        }

        private void tb_filter2_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, ExtensionMethods.filter(Lollipop.get_files(Lollipop.file_types[cmb_loadTable2.SelectedIndex]), tb_filter2.Text));
        }

        private void tb_filter3_TextChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, ExtensionMethods.filter(Lollipop.get_files(Lollipop.file_types[cmb_loadTable3.SelectedIndex]), tb_filter3.Text));

        }


        // CHANGED TABLE SELECTION
        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Lollipop.get_files(Lollipop.file_types[cmb_loadTable1.SelectedIndex]));
            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_loadTable2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles2, Lollipop.get_files(Lollipop.file_types[cmb_loadTable2.SelectedIndex]));
            lb_filter2.Text = cmb_loadTable1.SelectedItem.ToString();
        }

        private void cmb_LoadTable3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles3, Lollipop.get_files(Lollipop.file_types[cmb_loadTable3.SelectedIndex]));
            lb_filter3.Text = cmb_loadTable1.SelectedItem.ToString();
        }
    }
}
