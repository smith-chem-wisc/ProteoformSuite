using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.IO;
using System.Security;
using Proteomics;
using Chemistry;

namespace ProteoformSuiteGUI
{
    public partial class ProteoformFamilies : Form
    {
        //FORM OPERATION
        public ProteoformFamilies()
        {
            InitializeComponent();
            initialize_settings();
        }

        private void ProteoformFamilies_Load(object sender, EventArgs e)
        { }

        public void initialize_every_time()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
            this.nud_decimalRoundingLabels.Value = Convert.ToDecimal(Lollipop.deltaM_edge_display_rounding);
            this.cb_buildAsQuantitative.Enabled = Lollipop.qVals.Count > 0;
            this.cb_buildAsQuantitative.Checked = false;
            this.cmbx_geneLabel.SelectedIndex = Lollipop.gene_name_labels.IndexOf(ProteoformCommunity.preferred_gene_label);
            this.cb_geneCentric.Checked = ProteoformCommunity.gene_centric_families;
        }

        public void initialize_settings()
        {
            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);
            cmbx_nodeLabel.Items.AddRange(Lollipop.node_labels);
            cmbx_geneLabel.Items.AddRange(Lollipop.gene_name_labels.ToArray());
            cmbx_tableSelector.Items.AddRange(table_names);

            cmbx_colorScheme.SelectedIndex = 0;
            cmbx_nodeLayout.SelectedIndex = 0;
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
            tb_likelyCleavages.Text = String.Join(",", Lollipop.likely_cleavages);
            tb_likelyCleavages.TextChanged += tb_likelyCleavages_TextChanged;
        }

        public void construct_families()
        {
            initialize_every_time();
            if (Lollipop.proteoform_community.families.Count <= 0 && Lollipop.proteoform_community.has_e_proteoforms) run_the_gamut();
        }


        public DataGridView GetDGV()
        {
            return dgv_main;
        }

        public void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            Lollipop.proteoform_community.families.Clear();
            Lollipop.proteoform_community.construct_families();
            fill_proteoform_families("");
            update_figures_of_merit();
            this.Cursor = Cursors.Default;
        }

        public void ClearListsAndTables()
        {
            Lollipop.proteoform_community.families.Clear();
            foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.family = null;
            foreach (Proteoform p in Lollipop.proteoform_community.theoretical_proteoforms) p.family = null;
            foreach (Proteoform p in Lollipop.proteoform_community.decoy_proteoforms.Values.SelectMany(d => d)) p.family = null;
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();
        }

        public void update_figures_of_merit()
        {
            this.tb_TotalFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString();
            this.tb_IdentifiedFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.theoretical_count > 0).ToString();
            this.tb_singleton_count.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString();
        }


        // MAIN TABLE CONTROL
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
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.BiologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.CellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 4) fill_go(Aspect.MolecularFunction, tb_tableFilter.Text);
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.BiologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.CellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 4) fill_go(Aspect.MolecularFunction, tb_tableFilter.Text);
        }

        public void fill_proteoform_families(string filter)
        {
            IEnumerable<object> families = filter == "" ?
                Lollipop.proteoform_community.families.OrderByDescending(f => f.relation_count) :
                ExtensionMethods.filter(Lollipop.proteoform_community.families.OrderByDescending(f => f.relation_count), filter);
            DisplayUtility.FillDataGridView(dgv_main, families);
            if (families.Count() > 0) DisplayUtility.format_families_dgv(dgv_main);
        }

        private void fill_theoreticals(string filter)
        {
            IEnumerable<object> theoreticals = filter == "" ?
                Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms) :
                ExtensionMethods.filter(Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms), filter);
            DisplayUtility.FillDataGridView(dgv_main, theoreticals);
            if (theoreticals.Count() > 0) DisplayUtility.FormatTheoreticalProteoformTable(dgv_main);
        }

        private void fill_go(Aspect aspect, string filter)
        {
            DisplayUtility.FillDataGridView(dgv_main, filter == "" ?
                Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect) :
                ExtensionMethods.filter(Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect), filter));
        }

        private void dgv_proteoform_families_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0 && e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void dgv_proteoform_families_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0 && e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void display_family_members(int row_index, int column_index)
        {
            ProteoformFamily selected_family = (ProteoformFamily)this.dgv_main.Rows[row_index].DataBoundItem;
            if (column_index < 0) return;
            if (new List<string> { "theoretical_count", "accession_list","name_list" }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.theoretical_count > 0) 
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms);
                    DisplayUtility.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (new List<string> { "experimental_count", "experimentals_list", "agg_mass_list" }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.experimental_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms);
                    DisplayUtility.FormatAggregatesTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (dgv_main.Columns[column_index].Name == "relation_count")
            {
                if (selected_family.relation_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.relations);
                    DisplayUtility.FormatRelationsGridView(dgv_proteoform_family_members, false, false);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
        }


        // CYTOSCAPE VISUALIZATION
        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;

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
            Lollipop.family_build_folder_path = path;
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
            string message = CytoscapeScript.write_cytoscape_script(Lollipop.proteoform_community.families, Lollipop.proteoform_community.families, 
                Lollipop.family_build_folder_path, "", time_stamp, 
                cb_buildAsQuantitative.Checked, cb_redBorder.Checked, cb_boldLabel.Checked, cb_moreOpacity.Checked, 
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), Lollipop.deltaM_edge_display_rounding, 
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            object[] selected = DisplayUtility.get_selected_objects(dgv_main);
            string message = CytoscapeScript.write_cytoscape_script(selected, Lollipop.proteoform_community.families,
                Lollipop.family_build_folder_path, "", time_stamp,
                cb_buildAsQuantitative.Checked, cb_redBorder.Checked, cb_boldLabel.Checked, cb_moreOpacity.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), Lollipop.deltaM_edge_display_rounding, 
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void Families_update_Click(object sender, EventArgs e)
        {
            ClearListsAndTables();
            run_the_gamut();
        }

        private void nud_decimalRoundingLabels_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.deltaM_edge_display_rounding = Convert.ToInt32(this.nud_decimalRoundingLabels.Value);
        }

        private void btn_merge_Click(object sender, EventArgs e)
        {
            
        }

        private void cb_buildAsQuantitative_CheckedChanged(object sender, EventArgs e)
        {
            cb_redBorder.Enabled = cb_buildAsQuantitative.Checked;
            cb_boldLabel.Enabled = cb_buildAsQuantitative.Checked;
            cb_moreOpacity.Enabled = false; //not fully implemented
        }

        private void btn_inclusion_list_all_families_Click(object sender, EventArgs e)
        {
            List<ExperimentalProteoform> proteoforms = new List<ExperimentalProteoform>();
            if (cb_identified_families.Checked) proteoforms.AddRange(Lollipop.proteoform_community.families.Where(f => f.relation_count > 0 && f.theoretical_count > 0).SelectMany(f => f.experimental_proteoforms).ToList());
            if (cb_unidentified_families.Checked) proteoforms.AddRange(Lollipop.proteoform_community.families.Where(f => f.relation_count > 0 && f.theoretical_count == 0).SelectMany(f => f.experimental_proteoforms).ToList());
            if (cb_orphans.Checked) proteoforms.AddRange(Lollipop.proteoform_community.families.Where(f => f.relation_count == 0).SelectMany(f => f.experimental_proteoforms).ToList());
            write_inclusion_list(proteoforms);
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
                        if (Lollipop.neucode_labeled) mz = mz - (136.109162 * proteoform.lysine_count).ToMz(max.charge_count) + (128.094963 * proteoform.lysine_count).ToMz(max.charge_count);
                        writer.WriteLine(mz + "\t" + max.charge_count + "\t" + proteoform.agg_rt);
                    }
                }
                MessageBox.Show("Successfully exported inclusion list.");
            }
            else return;
        }

        private void cmbx_geneLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
        }

        private void cb_geneCentric_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.gene_centric_families = cb_geneCentric.Checked;
        }

        private void tb_likelyCleavages_TextChanged(object sender, EventArgs e)
        {
            Lollipop.likely_cleavages = tb_likelyCleavages.Text.Split(',');
        }
    }
}
