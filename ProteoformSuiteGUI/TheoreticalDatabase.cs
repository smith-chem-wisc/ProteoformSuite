using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
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
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
        }

        #endregion Public Constructor

        #region Private Fields

        private OpenFileDialog openAccessionListDialog = new OpenFileDialog();
        private bool initial_load = true;

        #endregion Private Fields

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Private Methods

        private void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            InitializeParameterSet();
            initial_load = false;
        }

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            RunTheGamut(false);
            Cursor = Cursors.Default;
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                string table = cmbx_DisplayWhichDB.SelectedItem.ToString();
                if (table == "Target")
                    DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
                else
                    DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.decoy_proteoform_communities[table].theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            }
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        private void cmb_empty_TextChanged(object sender, EventArgs e)
        {
        }

        #endregion Private Methods

        #region Public Methods

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)).ToList(), "TargetDatabase"),
                DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(Sweet.lollipop.theoretical_database.unlocalized_lookup.Values.Select(m => new DisplayUnlocalizedModification(m)).ToList(), "UnlocalizedModifications")
            };
            foreach (KeyValuePair<string, ProteoformCommunity> decoy_community in Sweet.lollipop.decoy_proteoform_communities)
            {
                DataTables.Add(DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(decoy_community.Value.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)).ToList(), decoy_community.Key));
            }
            return DataTables;
        }

        public void FillDataBaseTable(string table)
        {
            if (table == "Target")
                DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            else if (Sweet.lollipop.decoy_proteoform_communities.ContainsKey(table))
                DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.decoy_proteoform_communities[table].theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public void initialize_table_bindinglist()
        {
            List<string> databases = new List<string> { "Target" };
            if (Sweet.lollipop.decoy_proteoform_communities.Keys.Count > 0)
            {
                foreach (string name in Sweet.lollipop.decoy_proteoform_communities.Keys)
                {
                    databases.Add(name);
                }
            }
            cmbx_DisplayWhichDB.DataSource = new BindingList<string>(databases);
            cb_useRandomSeed.Checked = Sweet.lollipop.useRandomSeed_decoys;
            nud_randomSeed.Value = Sweet.lollipop.randomSeed_decoys;
        }

        public void InitializeParameterSet()
        {
            cb_average_mass.Checked = Sweet.lollipop.use_average_mass;

            nUD_MaxPTMs.Minimum = 0;
            nUD_MaxPTMs.Maximum = 5;
            nUD_MaxPTMs.Value = Sweet.lollipop.max_ptms;

            nUD_NumDecoyDBs.Minimum = 0;
            nUD_NumDecoyDBs.Maximum = 50;
            nUD_NumDecoyDBs.Value = Sweet.lollipop.decoy_databases;

            nUD_MinPeptideLength.Minimum = 0;
            nUD_MinPeptideLength.Maximum = 20;
            nUD_MinPeptideLength.Value = Sweet.lollipop.min_peptide_length;

            ckbx_combineIdenticalSequences.Checked = Sweet.lollipop.combine_identical_sequences;
            ckbx_combineTheoreticalsByMass.Checked = Sweet.lollipop.combine_theoretical_proteoforms_byMass;
            cb_limitLargePtmSets.Checked = Sweet.lollipop.theoretical_database.limit_triples_and_greater;
            cb_useRandomSeed.Checked = Sweet.lollipop.useRandomSeed_decoys;
            nud_randomSeed.Value = Sweet.lollipop.randomSeed_decoys;
            ckbx_OxidMeth.Checked = Sweet.lollipop.methionine_oxidation;
            ckbx_Meth_Cleaved.Checked = Sweet.lollipop.methionine_cleavage;
            ckbx_Carbam.Checked = Sweet.lollipop.carbamidomethylation;
            ckbx_combineIdenticalSequences.Checked = Sweet.lollipop.combine_identical_sequences;
            ckbx_combineTheoreticalsByMass.Checked = Sweet.lollipop.combine_theoretical_proteoforms_byMass;

            tb_modTypesToExclude.Text = string.Join(",", Sweet.lollipop.mod_types_to_exclude);

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;

            tb_modTableFilter.TextChanged -= tb_modTableFilter_TextChanged;
            tb_modTableFilter.Text = "";
            tb_modTableFilter.TextChanged += tb_modTableFilter_TextChanged;

            tb_totalTheoreticalProteoforms.Text = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString();
        }

        public void RunTheGamut(bool full_run)
        {
            if (ReadyToRunTheGamut())
            {
                ClearListsTablesFigures(true);
                Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Environment.CurrentDirectory);
                FillTablesAndCharts();
                if (!full_run && BottomUpReader.bottom_up_PTMs_not_in_dictionary.Count() > 0)
                {
                    MessageBox.Show(
                        "Warning: the following PTMs in the .mzid file were not matched with any PTMs in the theoretical database: " +
                        string.Join(", ", BottomUpReader.bottom_up_PTMs_not_in_dictionary.Distinct()));
                }
            }
            else
            {
                MessageBox.Show("Please list at least one protein database.");
            }
        }

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).Count() > 0;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_Database.DataSource = null;
            dgv_Database.Rows.Clear();
            dgv_loadFiles.DataSource = null;
            dgv_loadFiles.Rows.Clear();
            dgv_unlocalizedModifications.DataSource = null;
            dgv_unlocalizedModifications.Rows.Clear();
            tb_modTableFilter.Clear();
            tb_tableFilter.Clear();
            tb_totalTheoreticalProteoforms.Clear();
            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    if (sweet as RawExperimentalComponents == null && (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Any(e => e.topdown_id) || sweet as AggregatedProteoforms == null))
                    {
                        sweet.ClearListsTablesFigures(false);
                    }
                }
            }
        }

        public void FillTablesAndCharts()
        {
            reload_database_list();
            DisplayUtility.FillDataGridView(dgv_Database, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
            initialize_table_bindinglist();
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, Sweet.lollipop.theoretical_database.unlocalized_lookup.Values.Select(m => new DisplayUnlocalizedModification(m)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
            tb_totalTheoreticalProteoforms.Text = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString();
        }

        #endregion Public Methods

        #region CHECKBOXES Private Methods

        private void ckbx_combineIdenticalSequences_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.combine_identical_sequences = ckbx_combineIdenticalSequences.Checked;
        }

        private void ckbx_combineTheoreticalsByMass_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.combine_theoretical_proteoforms_byMass = ckbx_combineTheoreticalsByMass.Checked;
        }

        private void ckbx_OxidMeth_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.methionine_oxidation = ckbx_OxidMeth.Checked;
        }

        private void ckbx_Carbam_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.carbamidomethylation = ckbx_Carbam.Checked;
        }

        private void ckbx_Meth_Cleaved_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.methionine_cleavage = ckbx_Meth_Cleaved.Checked;
        }

        private void nUD_MaxPTMs_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_ptms = Convert.ToInt32(nUD_MaxPTMs.Value);
        }

        private void nUD_NumDecoyDBs_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.decoy_databases = Convert.ToInt32(nUD_NumDecoyDBs.Value);
        }

        private void nUD_MinPeptideLength_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_peptide_length = Convert.ToInt32(nUD_MinPeptideLength.Value);
        }

        private void cb_limitLargePtmSets_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.theoretical_database.limit_triples_and_greater = cb_limitLargePtmSets.Checked;
        }

        #endregion CHECKBOXES Private Methods

        #region LOAD DATABASES GRID VIEW Private Methods

        private void dgv_loadFiles_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable, dgv_loadFiles);
        }

        private void dgv_loadFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void drag_drop(DragEventArgs e, ComboBox cmb, DataGridView dgv)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Sweet.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Sweet.lollipop.input_files, true);
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[cmb.SelectedIndex]);
        }

        public void reload_database_list()
        {
            cmb_loadTable.Items.Clear();
            cmb_loadTable.Items.AddRange(Lollipop.file_lists);
            cmb_loadTable.SelectedIndex = 2;
            DisplayUtility.FillDataGridView(dgv_loadFiles, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
            initialize_table_bindinglist();
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            List<TheoreticalProteoform> theoreticals_to_display = cmbx_DisplayWhichDB.SelectedItem.ToString() == "Target" ?
                 Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.ToList()
                 : Sweet.lollipop.decoy_proteoform_communities[cmbx_DisplayWhichDB.SelectedItem.ToString()].theoretical_proteoforms.ToList();
            IEnumerable<object> selected_theoreticals = tb_tableFilter.Text == "" ?
                theoreticals_to_display :
                ExtensionMethods.filter(theoreticals_to_display, tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_Database, selected_theoreticals.OfType<TheoreticalProteoform>().Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_Database);
        }

        private Regex substituteWhitespace = new Regex(@"\s+");

        private void tb_modTypesToExclude_TextChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.mod_types_to_exclude = substituteWhitespace.Replace(tb_modTypesToExclude.Text, "").Split(',');
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
            {
                Sweet.lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb_loadTable.SelectedIndex], Lollipop.file_types[cmb_loadTable.SelectedIndex], Sweet.lollipop.input_files, true);
            }

            DisplayUtility.FillDataGridView(dgv_loadFiles, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
        }

        private void btn_clearFiles_Click(object sender, EventArgs e)
        {
            List<InputFile> files_to_remove = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).ToList();
            Sweet.save_actions.RemoveAll(a => files_to_remove.Any(f => a.Contains(f.complete_path)));
            Sweet.lollipop.input_files = Sweet.lollipop.input_files.Except(files_to_remove).ToList();
            DisplayUtility.FillDataGridView(dgv_loadFiles, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles, Lollipop.file_types[cmb_loadTable.SelectedIndex]);
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

            Sweet.lollipop.theoretical_database.save_unlocalized_names(d.FileName);
        }

        private void btn_amendModNames_Click(object sender, EventArgs e)
        {
            Sweet.lollipop.theoretical_database.amend_unlocalized_names(Path.Combine(Environment.CurrentDirectory, "Mods", "stored_mods.modnames"));
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

            Sweet.lollipop.theoretical_database.load_unlocalized_names(d.FileName);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, Sweet.lollipop.theoretical_database.unlocalized_lookup.Values.Select(m => new DisplayUnlocalizedModification(m)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
        }

        private void tb_modTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_unmods = tb_modTableFilter.Text == "" ?
                Sweet.lollipop.theoretical_database.unlocalized_lookup.Values :
                ExtensionMethods.filter(Sweet.lollipop.theoretical_database.unlocalized_lookup.Values, tb_modTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_unlocalizedModifications, selected_unmods.OfType<UnlocalizedModification>().Select(u => new DisplayUnlocalizedModification(u)));
            DisplayUnlocalizedModification.FormatUnlocalizedModificationTable(dgv_unlocalizedModifications);
        }

        #endregion Modification Names Private Methods

        private void cb_useRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.useRandomSeed_decoys = cb_useRandomSeed.Checked;
        }

        private void nud_randomSeed_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.randomSeed_decoys = Convert.ToInt32(nud_randomSeed.Value);
        }
        private void cb_mostAbundantMass_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.use_average_mass = cb_average_mass.Checked;

        }
    }
}