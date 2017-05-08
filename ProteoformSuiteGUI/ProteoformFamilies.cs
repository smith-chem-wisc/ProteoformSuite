using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.IO;
using Chemistry;

namespace ProteoformSuiteGUI
{
    public partial class ProteoformFamilies : Form, ISweetForm
    {

        #region Public Constructor

        public ProteoformFamilies()
        {
            InitializeComponent();
            InitializeParameterSet();
        }

        #endregion

        #region Public Methods

        public void initialize_every_time()
        {
            tb_familyBuildFolder.Text = SaveState.lollipop.family_build_folder_path;
            nud_decimalRoundingLabels.Value = Convert.ToDecimal(SaveState.lollipop.deltaM_edge_display_rounding);
            cb_buildAsQuantitative.Enabled = SaveState.lollipop.qVals.Count > 0;
            cb_buildAsQuantitative.Checked = false;
            cmbx_geneLabel.SelectedIndex = Lollipop.gene_name_labels.IndexOf(ProteoformCommunity.preferred_gene_label);
            cb_geneCentric.Checked = ProteoformCommunity.gene_centric_families;
        }

        public void InitializeParameterSet()
        {
            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);
            cmbx_nodeLabel.Items.AddRange(Lollipop.node_labels);
            cmbx_geneLabel.Items.AddRange(Lollipop.gene_name_labels.ToArray());
            cmbx_tableSelector.Items.AddRange(table_names);

            cmbx_colorScheme.SelectedIndex = 1;
            cmbx_nodeLayout.SelectedIndex = 1;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;
            cmbx_edgeLabel.SelectedIndex = 1;
            cmbx_nodeLabel.SelectedIndex = 1;
            cmbx_geneLabel.SelectedIndex = 1;
            ProteoformCommunity.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
            ProteoformCommunity.gene_centric_families = cb_geneCentric.Checked;

            cmbx_tableSelector.SelectedIndexChanged -= cmbx_tableSelector_SelectedIndexChanged;
            cmbx_tableSelector.SelectedIndex = 0;
            cmbx_tableSelector.SelectedIndexChanged += cmbx_tableSelector_SelectedIndexChanged;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;

            tb_likelyCleavages.TextChanged -= tb_likelyCleavages_TextChanged;
            tb_likelyCleavages.Text = String.Join(",", SaveState.lollipop.likely_cleavages);
            tb_likelyCleavages.TextChanged += tb_likelyCleavages_TextChanged;

            initialize_every_time();
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_main };
        }

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.target_proteoform_community.families.Count <= 0 && SaveState.lollipop.target_proteoform_community.has_e_proteoforms;
        }

        public void RunTheGamut()
        {
            Cursor = Cursors.WaitCursor;
            SaveState.lollipop.clear_all_families();
            SaveState.lollipop.construct_target_and_decoy_families();
            cmbx_tableSelector.SelectedIndex = 0;
            tb_tableFilter.Text = "";
            fill_proteoform_families("", -1);
            update_figures_of_merit();
            Cursor = Cursors.Default;
        }

        public void FillTablesAndCharts()
        {
            fill_proteoform_families("", -1);
            update_figures_of_merit();
        }

        public void ClearListsTablesFigures()
        {
            SaveState.lollipop.clear_all_families();
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();
            dgv_proteoform_family_members.DataSource = null;
            dgv_proteoform_family_members.Rows.Clear();
        }

        public void update_figures_of_merit()
        {
            tb_TotalFamilies.Text = SaveState.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString();
            tb_IdentifiedFamilies.Text = SaveState.lollipop.target_proteoform_community.families.Count(f => f.theoretical_proteoforms.Count > 0).ToString();
            tb_singleton_count.Text = SaveState.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString();
            tb_identified_experimentals.Text = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null).ToString();
            if (SaveState.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.experimental_proteoforms).Count() > 0)
            tb_identified_decoys.Text = Math.Round(SaveState.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null)), 0).ToString();

            //change selected table names based on # decoy communities
            int decoy_communities = SaveState.lollipop.decoy_proteoform_communities.Count;
            for(int i = 0; i < decoy_communities; i++)
            {
               if (!cmbx_tableSelector.Items.Contains("Decoy Community " + i)) cmbx_tableSelector.Items.Add("Decoy Community " + i);
            }
            //if more items than decoy databases, remove later ones //FIX   
            while(cmbx_tableSelector.Items.Count - 5 - decoy_communities > 0)
            {
                cmbx_tableSelector.Items.RemoveAt(5 + decoy_communities);
            }
        }

        #endregion Public Methods

        #region Main Table Private Methods

        private static string[] table_names = new string[5]
        {
            "Proteoform Families",
            "Theoretical Proteoforms in Families",
            "GO Terms of Families -- " + Aspect.BiologicalProcess,
            "GO Terms of Families -- " + Aspect.CellularComponent,
            "GO Terms of Families -- " + Aspect.MolecularFunction,
        };

        private static Type[] table_types = new Type[5]
        {
            typeof(ProteoformFamily),
            typeof(TheoreticalProteoform),
            typeof(GoTerm),
            typeof(GoTerm),
            typeof(GoTerm)
        };

        private void cmbx_tableSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text, -1);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.BiologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.CellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 4) fill_go(Aspect.MolecularFunction, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex > 4) fill_proteoform_families(tb_tableFilter.Text, cmbx_tableSelector.SelectedIndex - 5);
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text, -1);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.BiologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.CellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex > 4) fill_proteoform_families(tb_tableFilter.Text, cmbx_tableSelector.SelectedIndex - 5);
        }

        public void fill_proteoform_families(string filter, int decoyCommunityMinusOneIsTarget)
        {
            IEnumerable<object> families = filter == "" ?
                 ( decoyCommunityMinusOneIsTarget < 0 ? 
                SaveState.lollipop.target_proteoform_community.families.OrderByDescending(f => f.relations.Count) :
                SaveState.lollipop.decoy_proteoform_communities[SaveState.lollipop.decoy_community_name_prefix + decoyCommunityMinusOneIsTarget].families.OrderByDescending(f => f.relations.Count)) :
                ( decoyCommunityMinusOneIsTarget < 0 ?
                ExtensionMethods.filter(SaveState.lollipop.target_proteoform_community.families.OrderByDescending(f => f.relations.Count), filter)
                : ExtensionMethods.filter(SaveState.lollipop.decoy_proteoform_communities[SaveState.lollipop.decoy_community_name_prefix + decoyCommunityMinusOneIsTarget].families.OrderByDescending(f => f.relations.Count), filter));
            DisplayUtility.FillDataGridView(dgv_main, families.OfType<ProteoformFamily>().Select(f => new DisplayProteoformFamily(f)));
            DisplayProteoformFamily.format_families_dgv(dgv_main);
        }

        private void fill_theoreticals(string filter)
        {
            IEnumerable<object> theoreticals = filter == "" ?
                SaveState.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms) :
                ExtensionMethods.filter(SaveState.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms), filter);
            DisplayUtility.FillDataGridView(dgv_main, theoreticals.OfType<TheoreticalProteoform>().Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_main);
        }

        private void fill_go(Aspect aspect, string filter)
        {
            DisplayUtility.FillDataGridView(dgv_main, filter == "" ?
                SaveState.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ExpandedProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect) :
                ExtensionMethods.filter(SaveState.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ExpandedProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect), filter));
        }

        private void dgv_proteoform_families_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((cmbx_tableSelector.SelectedIndex == 0 || cmbx_tableSelector.SelectedIndex > 4) && e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }

        private void dgv_proteoform_families_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((cmbx_tableSelector.SelectedIndex == 0 || cmbx_tableSelector.SelectedIndex > 4) && e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }

        private void display_family_members(int row_index, int column_index)
        {
            ProteoformFamily selected_family = (ProteoformFamily)((DisplayObject)this.dgv_main.Rows[row_index].DataBoundItem).display_object;

            if (column_index < 0) return;

            if (new List<string> { nameof(DisplayProteoformFamily.theoretical_count), nameof(DisplayProteoformFamily.accession_list), nameof(DisplayProteoformFamily.name_list) }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.theoretical_proteoforms.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
                    DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }

            else if (new List<string> { nameof(DisplayProteoformFamily.experimental_count), nameof(DisplayProteoformFamily.experimentals_list), nameof(DisplayProteoformFamily.agg_mass_list) }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.experimental_proteoforms.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)));
                    DisplayExperimentalProteoform.FormatAggregatesTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }

            else if (dgv_main.Columns[column_index].Name == nameof(DisplayProteoformFamily.relation_count))
            {
                if (selected_family.relations.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.relations.Select(r => new DisplayProteoformRelation(r)));
                    DisplayProteoformRelation.FormatRelationsGridView(dgv_proteoform_family_members, false, false);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
        }

        #endregion Main Table Private Methods

        #region Cytoscape Visualization Private Fields

        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;

        #endregion Cytoscape Visualization Private Fields

        #region Cytoscape Visualization Private Methods

        private void btn_browseTempFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_familyBuildFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void tb_tempFileFolderPath_TextChanged(object sender, EventArgs e)
        {
            string path = tb_familyBuildFolder.Text;
            SaveState.lollipop.family_build_folder_path = path;
            got_cyto_temp_folder = true;
            enable_buildAllFamilies_button();
            enable_buildSelectedFamilies_button();
        }

        private void enable_buildAllFamilies_button()
        {
            if (got_cyto_temp_folder) btn_buildAllFamilies.Enabled = true;
        }

        private void enable_buildSelectedFamilies_button()
        {
            if (got_cyto_temp_folder && dgv_main.SelectedRows.Count > 0) btn_buildSelectedFamilies.Enabled = true;
        }

        private void btn_buildAllFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(SaveState.lollipop.target_proteoform_community.families, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp,
                cb_buildAsQuantitative.Checked, cb_redBorder.Checked, cb_boldLabel.Checked, cb_moreOpacity.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            object[] selected = DisplayUtility.get_selected_objects(dgv_main);
            string message = CytoscapeScript.write_cytoscape_script(selected, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp,
                cb_buildAsQuantitative.Checked, cb_redBorder.Checked, cb_boldLabel.Checked, cb_moreOpacity.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void Families_update_Click(object sender, EventArgs e)
        {
            ClearListsTablesFigures();
            RunTheGamut();
        }

        private void nud_decimalRoundingLabels_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.deltaM_edge_display_rounding = Convert.ToInt32(this.nud_decimalRoundingLabels.Value);
        }

        private void cmbx_geneLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
        }

        #endregion Cytoscape Visualization Private Methods

        #region Private Methods

        private void cb_buildAsQuantitative_CheckedChanged(object sender, EventArgs e)
        {
            cb_redBorder.Enabled = cb_buildAsQuantitative.Checked;
            cb_boldLabel.Enabled = cb_buildAsQuantitative.Checked;
            cb_moreOpacity.Enabled = false; //not fully implemented
        }

        private void btn_inclusion_list_all_families_Click(object sender, EventArgs e)
        {
            List<ExperimentalProteoform> proteoforms = new List<ExperimentalProteoform>();
            if (cb_identified_families.Checked) proteoforms.AddRange(SaveState.lollipop.target_proteoform_community.families.Where(f => f.relations.Count > 0 && f.theoretical_proteoforms.Count > 0).SelectMany(f => f.experimental_proteoforms).ToList());
            if (cb_unidentified_families.Checked) proteoforms.AddRange(SaveState.lollipop.target_proteoform_community.families.Where(f => f.relations.Count > 0 && f.theoretical_proteoforms.Count == 0).SelectMany(f => f.experimental_proteoforms).ToList());
            if (cb_orphans.Checked) proteoforms.AddRange(SaveState.lollipop.target_proteoform_community.families.Where(f => f.relations.Count == 0).SelectMany(f => f.experimental_proteoforms).ToList());
            write_inclusion_list(proteoforms);
        }


        private void btn_merge_Click(object sender, EventArgs e)
        {

        }

        private void btn_inclusion_list_selected_families_Click(object sender, EventArgs e)
        {
            object[] selected = DisplayUtility.get_selected_objects(dgv_main);
            List<ProteoformFamily> families = selected.OfType<ProteoformFamily>().ToList();
            write_inclusion_list(families.SelectMany(f => f.experimental_proteoforms).ToList());
        }

        private void write_inclusion_list(List<ExperimentalProteoform> proteoforms)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Text files (*.txt)|*.txt";
            saveDialog.FileName = "inclusion_list.txt";
            DialogResult dr = saveDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                using (var writer = new StreamWriter(saveDialog.FileName))
                {
                    foreach (ExperimentalProteoform proteoform in proteoforms)
                    {
                        //get highest intensity charge state 
                        ChargeState max = proteoform.aggregated_components.SelectMany(p => p.charge_states).OrderByDescending(c => c.intensity).First();
                        double mz = max.mz_centroid;
                        if (SaveState.lollipop.neucode_labeled) mz = mz - (136.109162 * proteoform.lysine_count).ToMz(max.charge_count) + (128.094963 * proteoform.lysine_count).ToMz(max.charge_count);
                        writer.WriteLine(mz + "\t" + max.charge_count + "\t" + proteoform.agg_rt);
                    }
                }
                MessageBox.Show("Successfully exported inclusion list.");
            }
            else return;
        }

        private void cb_geneCentric_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.gene_centric_families = cb_geneCentric.Checked;
        }

        private void tb_likelyCleavages_TextChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.likely_cleavages = tb_likelyCleavages.Text.Split(',');
        }

        #endregion Private Methods

    }
}
