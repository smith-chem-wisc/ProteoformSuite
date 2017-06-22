using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class TheoreticalDatabase : Form, ISweetForm
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
            InitializeParameterSet();
            initial_load = false;
        }

        private void set_Make_Database_Button()
        {
            bool ready_to_run = ReadyToRunTheGamut();
            btn_downloadUniProtPtmList.Enabled = !ready_to_run && SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.PtmList).Count() == 0;
            btn_Make_Databases.Enabled = ready_to_run;
        }

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            RunTheGamut();
            Cursor = Cursors.Default;
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                string table = cmbx_DisplayWhichDB.SelectedItem.ToString();
                if (table == "Target")
                    DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
                else
                    DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.decoy_proteoform_communities[table].theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            }
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        private void cmb_empty_TextChanged(object sender, EventArgs e) { }

        #endregion Private Methods

        #region Public Methods

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_Database, dgv_unlocalizedModifications };
        }

        public void FillDataBaseTable(string table)
        {
            if (table == "Target")
                DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            else if (SaveState.lollipop.decoy_proteoform_communities.ContainsKey(table))
                DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.decoy_proteoform_communities[table].theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public void initialize_table_bindinglist()
        {
            List<string> databases = new List<string> { "Target" };
            if (SaveState.lollipop.decoy_proteoform_communities.Keys.Count > 0)
            {
                foreach (string name in SaveState.lollipop.decoy_proteoform_communities.Keys)
                {
                    databases.Add(name);
                }
            }
            cmbx_DisplayWhichDB.DataSource = new BindingList<string>(databases);
        }

        public void InitializeParameterSet()
        {
            btn_NeuCode_Lt.Checked = SaveState.lollipop.neucode_labeled;
            btn_NaturalIsotopes.Checked = !SaveState.lollipop.neucode_labeled;

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

            tb_modTableFilter.TextChanged -= tb_modTableFilter_TextChanged;
            tb_modTableFilter.Text = "";
            tb_modTableFilter.TextChanged += tb_modTableFilter_TextChanged;

            tb_totalTheoreticalProteoforms.Text = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString();
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Environment.CurrentDirectory);
            tb_totalTheoreticalProteoforms.Text = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString();
            FillTablesAndCharts();
        }

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.theoretical_database.ready_to_make_database(Environment.CurrentDirectory);
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_Database.DataSource = null;
            dgv_Database.Rows.Clear();
            dgv_loadFiles.DataSource = null;
            dgv_loadFiles.Rows.Clear();
            dgv_unlocalizedModifications.DataSource = null;
            dgv_unlocalizedModifications.Rows.Clear();

            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    sweet.ClearListsTablesFigures(false);
                }
            }
        }
        
        public void FillTablesAndCharts()
        {
            reload_database_list();
            DisplayUtility.FillDataGridView(dgv_Database, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, SaveState.lollipop.theoretical_database.unlocalized_lookup.Values.Select(m => new DisplayUnlocalizedModification(m)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
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

        private void cb_limitLargePtmSets_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.theoretical_database.limit_triples_and_greater = cb_limitLargePtmSets.Checked;
        }

        #endregion CHECKBOXES Private Methods

        #region LOAD DATABASES GRID VIEW Private Methods

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
            initialize_table_bindinglist();
            set_Make_Database_Button();
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_theoreticals = tb_tableFilter.Text == "" ?
                SaveState.lollipop.target_proteoform_community.theoretical_proteoforms :
                ExtensionMethods.filter(SaveState.lollipop.target_proteoform_community.theoretical_proteoforms, tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_Database, selected_theoreticals.OfType<TheoreticalProteoform>().Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        Regex substituteWhitespace = new Regex(@"\s+");
        private void tb_modTypesToExclude_TextChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.mod_types_to_exclude = substituteWhitespace.Replace(tb_modTypesToExclude.Text, "").Split(',');
        }

        private void btn_downloadUniProtPtmList_Click(object sender, EventArgs e)
        {
            SaveState.lollipop.enter_uniprot_ptmlist();
            DisplayUtility.FillDataGridView(dgv_loadFiles, SaveState.lollipop.get_files(SaveState.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
            btn_downloadUniProtPtmList.Enabled = false;
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

        #region Modification Names Private Methods

        private void btn_saveModNames_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Title = "Modification Names";
            d.Filter = "Modification Names (*.modnames) | *.modnames";

            if (d.ShowDialog() != DialogResult.OK)
                return;

            SaveState.lollipop.theoretical_database.save_unlocalized_names(d.FileName);
        }

        private void btn_amendModNames_Click(object sender, EventArgs e)
        {
            SaveState.lollipop.theoretical_database.amend_unlocalized_names(Path.Combine(Environment.CurrentDirectory, "Mods", "stored_mods.modnames"));
            MessageBox.Show("Successfully amended stored modification names.", "Amend Stored Mod Names");
        }

        private void btn_loadModNames_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Title = "Modification Names";
            d.Filter = "Modification Names (*.modnames) | *.modnames";
            d.Multiselect = false;

            if (d.ShowDialog() != DialogResult.OK)
                return;

            SaveState.lollipop.theoretical_database.load_unlocalized_names(d.FileName);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, SaveState.lollipop.theoretical_database.unlocalized_lookup.Values.Select(m => new DisplayUnlocalizedModification(m)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
        }

        private void tb_modTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_unmods = tb_modTableFilter.Text == "" ?
                SaveState.lollipop.theoretical_database.unlocalized_lookup.Values :
                ExtensionMethods.filter(SaveState.lollipop.theoretical_database.unlocalized_lookup.Values, tb_modTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, selected_unmods.OfType<UnlocalizedModification>().Select(u => new DisplayUnlocalizedModification(u)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
        }

        #endregion Modification Names Private Methods
    }
}
