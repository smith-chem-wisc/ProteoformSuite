using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteGUI
{
    public partial class TheoreticalDatabase : Form
    {

        #region Public Constructor

        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        #endregion Public Constructor

        #region Private Fields

        OpenFileDialog openAccessionListDialog = new OpenFileDialog();
        bool initial_load = true;

        #endregion Private Fields

        #region Private Methods

        private void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            InitializeSettings();
            initial_load = false;
        }

        private void InitializeSettings()
        {
            if (SaveState.lollipop.neucode_labeled)
                btn_NeuCode_Lt.Checked = true;
            else
            {
                btn_NeuCode_Lt.Checked = false;
                btn_NaturalIsotopes.Checked = true;
            }

            nUD_MaxPTMs.Minimum = 0;
            nUD_MaxPTMs.Maximum = 5;
            nUD_MaxPTMs.Value = SaveState.lollipop.max_ptms;

            nUD_NumDecoyDBs.Minimum = 0;
            nUD_NumDecoyDBs.Maximum = 50;
            nUD_NumDecoyDBs.Value = SaveState.lollipop.decoy_databases;

            nUD_MinPeptideLength.Minimum = 0;
            nUD_MinPeptideLength.Maximum = 20;
            nUD_MinPeptideLength.Value = SaveState.lollipop.min_peptide_length;

            ckbx_combineIdenticalSequences.Checked = SaveState.lollipop.combine_identical_sequences;
            ckbx_combineTheoreticalsByMass.Checked = SaveState.lollipop.combine_theoretical_proteoforms_byMass;

            tb_modTypesToExclude.Text = String.Join(",", SaveState.lollipop.mod_types_to_exclude);

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
        }

        private void set_Make_Database_Button()
        {
            btn_Make_Databases.Enabled = SaveState.lollipop.theoretical_database.ready_to_make_database(Environment.CurrentDirectory);
        }

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            make_databases();
            DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            this.initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
            this.Cursor = Cursors.Default;
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                string table = cmbx_DisplayWhichDB.SelectedItem.ToString();
                if (table == "Target")
                    DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
                else
                    DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.decoy_proteoforms[table].Select(t => new DisplayTheoreticalProteoform(t)));
            }
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        #endregion Private Methods

        #region Public Methods

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            this.initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public DataGridView GetDGV()
        {
            return dgv_Database;
        }

        public void FillDataBaseTable(string table)
        {
            if (table == "Target")
                DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            else if (SaveState.lollipop.proteoform_community.decoy_proteoforms.ContainsKey(table))
                DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.proteoform_community.decoy_proteoforms[table].Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);

        }

        public void make_databases()
        {
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Environment.CurrentDirectory);
            ((ProteoformSweet)MdiParent).experimentalTheoreticalComparison.ClearListsAndTables();
            tb_totalTheoreticalProteoforms.Text = SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length.ToString();
        }

        public void initialize_table_bindinglist()
        {
            List<string> databases = new List<string> { "Target" };
            if (SaveState.lollipop.proteoform_community.decoy_proteoforms.Keys.Count > 0)
                foreach (string name in SaveState.lollipop.proteoform_community.decoy_proteoforms.Keys)
                    databases.Add(name);
            cmbx_DisplayWhichDB.DataSource = new BindingList<string>(databases.ToList());
        }

        #endregion Public Methods

        #region CHECKBOXES Private Methods

        private void ckbx_combineIdenticalSequences_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.combine_identical_sequences = ckbx_combineIdenticalSequences.Checked;
        }

        private void ckbx_combineTheoreticalsByMass_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.combine_theoretical_proteoforms_byMass = ckbx_combineTheoreticalsByMass.Checked;
        }

        private void ckbx_OxidMeth_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.methionine_oxidation = ckbx_OxidMeth.Checked;
        }

        private void ckbx_Carbam_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.carbamidomethylation = ckbx_Carbam.Checked;
        }

        private void ckbx_Meth_Cleaved_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.methionine_cleavage = ckbx_Meth_Cleaved.Checked;
        }

        private void btn_NaturalIsotopes_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.natural_lysine_isotope_abundance = btn_NaturalIsotopes.Checked;
        }

        private void btn_NeuCode_Lt_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.neucode_light_lysine = btn_NeuCode_Lt.Checked;
        }

        private void btn_NeuCode_Hv_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.neucode_heavy_lysine = btn_NeuCode_Hv.Checked;
        }

        private void nUD_MaxPTMs_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.max_ptms = Convert.ToInt32(nUD_MaxPTMs.Value);
        }

        private void nUD_NumDecoyDBs_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.decoy_databases = Convert.ToInt32(nUD_NumDecoyDBs.Value);
        }

        private void nUD_MinPeptideLength_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_peptide_length = Convert.ToInt32(nUD_MinPeptideLength.Value);
        }

        #endregion CHECKBOXES Private Methods

        #region LOAD DATABASES GRID VIEW Private Methods

        private void cmb_loadTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_loadTable.SelectedIndex != 2) MessageBox.Show("Use the Load Deconvolution Results page to load data.");
            cmb_loadTable.SelectedIndex = 2;
        }

        private void dgv_loadFiles_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable, dgv_loadFiles);
            set_Make_Database_Button();
        }

        private void dgv_loadFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void drag_drop(DragEventArgs e, ComboBox cmb, DataGridView dgv)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            SaveState.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], SaveState.lollipop.input_files);
            DisplayUtility.FillDataGridView(dgv, SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
        }

        public void reload_database_list()
        {
            cmb_loadTable.Items.Clear();
            cmb_loadTable.Items.AddRange(Lollipop.file_lists);
            cmb_loadTable.SelectedIndex = 2;
            DisplayUtility.FillDataGridView(dgv_loadFiles, SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
            set_Make_Database_Button();
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_theoreticals = tb_tableFilter.Text == "" ?
                SaveState.lollipop.proteoform_community.theoretical_proteoforms :
                ExtensionMethods.filter(SaveState.lollipop.proteoform_community.theoretical_proteoforms, tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_Database, selected_theoreticals.OfType<TheoreticalProteoform>().Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        Regex substituteWhitespace = new Regex(@"\s+");
        private void tb_modTypesToExclude_TextChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.mod_types_to_exclude = substituteWhitespace.Replace(tb_modTypesToExclude.Text, "").Split(',');
        }

        #endregion LOAD DATABASES GRID VIEW Private Methods

        #region ADD/CLEAR Private Methods

        private void btn_addFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = cmb_loadTable.SelectedItem.ToString();
            openFileDialog.Filter = Lollipop.file_filters[cmb_loadTable.SelectedIndex];
            openFileDialog.Multiselect = true;

            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
                SaveState.lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb_loadTable.SelectedIndex], Lollipop.file_types[cmb_loadTable.SelectedIndex], SaveState.lollipop.input_files);

            DisplayUtility.FillDataGridView(dgv_loadFiles, SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
            set_Make_Database_Button();
        }

        private void btn_clearFiles_Click(object sender, EventArgs e)
        {
            SaveState.lollipop.input_files = SaveState.lollipop.input_files.Except(SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex])).ToList();
            DisplayUtility.FillDataGridView(dgv_loadFiles, SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
            set_Make_Database_Button();
        }

        #endregion ADD/CLEAR Private Methods

    }
}
