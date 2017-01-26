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

namespace ProteoformSuite
{
    public partial class ProteoformFamilies : Form
    {
        //FORM OPERATION
        public ProteoformFamilies()
        {
            InitializeComponent();
        }
        private void ProteoformFamilies_Load(object sender, EventArgs e)
        { }

        public void initialize_every_time()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
            this.nud_decimalRoundingLabels.Value = Convert.ToDecimal(Lollipop.deltaM_edge_display_rounding);
        }

        private void initialize_settings()
        {
            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);
            cmbx_tableSelector.Items.AddRange(table_names);

            cmbx_colorScheme.SelectedIndex = 0;
            cmbx_nodeLayout.SelectedIndex = 0;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;
            cmbx_edgeLabel.SelectedIndex = 0;

            cmbx_tableSelector.SelectedIndexChanged -= cmbx_tableSelector_SelectedIndexChanged;
            cmbx_tableSelector.SelectedIndex = 0;
            cmbx_tableSelector.SelectedIndexChanged += cmbx_tableSelector_SelectedIndexChanged;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
        }

        public void construct_families()
        {
            initialize_settings();
            if (Lollipop.proteoform_community.families.Count <= 0 && (Lollipop.proteoform_community.has_e_proteoforms || Lollipop.proteoform_community.topdown_proteoforms.Count > 0)) run_the_gamut();
            initialize_every_time();
        }

        public DataGridView GetDGV()
        {
            return dgv_main;
        }

        private void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            initialize_settings();
            Lollipop.proteoform_community.construct_families();
            fill_proteoform_families("");
            update_figures_of_merit();
            this.Cursor = Cursors.Default;
        }

        private void update_figures_of_merit()
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
            "GO Terms of Families -- " + Aspect.biologicalProcess,
            "GO Terms of Families -- " + Aspect.cellularComponent,
            "GO Terms of Families -- " + Aspect.molecularFunction,
        };

        private void cmbx_tableSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.biologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.cellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 4) fill_go(Aspect.molecularFunction, tb_tableFilter.Text);
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            if (cmbx_tableSelector.SelectedIndex == 0) fill_proteoform_families(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 1) fill_theoreticals(tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 2) fill_go(Aspect.biologicalProcess, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 3) fill_go(Aspect.cellularComponent, tb_tableFilter.Text);
            else if (cmbx_tableSelector.SelectedIndex == 4) fill_go(Aspect.molecularFunction, tb_tableFilter.Text);
        }

        private void fill_proteoform_families(string filter)
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
                Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.proteinList).SelectMany(g => g.goTerms).Where(g => g.aspect == aspect) :
                ExtensionMethods.filter(Lollipop.proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.proteinList).SelectMany(g => g.goTerms).Where(g => g.aspect == aspect), filter));
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
            bool built = build_families(Lollipop.proteoform_community.families);
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            List<ProteoformFamily> families = new List<ProteoformFamily>();
            if (cmbx_tableSelector.SelectedIndex == 0) families = (DisplayUtility.get_selected_objects(dgv_main).Select(o => (ProteoformFamily)o)).ToList();
            else if (cmbx_tableSelector.SelectedIndex == 1)
            {
                families =
                    (
                    from f in Lollipop.proteoform_community.families
                    from t in f.theoretical_proteoforms
                    where DisplayUtility.get_selected_objects(dgv_main).Select(o => (TheoreticalProteoform)o).Contains(t)
                    select f
                    ).ToList();
            }
            else
            {
                families =
                    (
                    from f in Lollipop.proteoform_community.families
                    from t in f.theoretical_proteoforms
                    from p in t.proteinList
                    from g in p.goTerms
                    where DisplayUtility.get_selected_objects(dgv_main).Select(o => (GoTerm)o).Contains(g)
                    select f
                    ).ToList();
            }

            bool built = build_families(families.Distinct().ToList());
            if (!built) return;

            string selected_family_string = "Finished building selected famil";
            if (families.Count() == 1) selected_family_string += "y :";
            else selected_family_string += "ies :";
            if (families.Count() > 3) selected_family_string = String.Join(", ", families.Select(f => f.family_id).ToList().Take(3)) + ". . .";
            else selected_family_string = String.Join(", ", families.Select(f => f.family_id));
            MessageBox.Show(selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private bool build_families(List<ProteoformFamily> families)
        {
            //Check if valid folder
            if (Lollipop.family_build_folder_path == "" || !Directory.Exists(Lollipop.family_build_folder_path))
            {
                MessageBox.Show("Please choose a folder in which the families will be built, so you can load them into Cytoscape.");
                return false;
            }
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            CytoscapeScript c = new CytoscapeScript(families, time_stamp, false, false, false, false, cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString());
            File.WriteAllText(c.edges_path, c.edge_table);
            File.WriteAllText(c.nodes_path, c.node_table);
            File.WriteAllText(c.script_path, c.script);
            c.write_styles(); //cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), "");
            return true;
        }

        private void Families_update_Click(object sender, EventArgs e)
        {
            Lollipop.proteoform_community.families.Clear();
            run_the_gamut();
        }

        private void nud_decimalRoundingLabels_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.deltaM_edge_display_rounding = Convert.ToInt32(this.nud_decimalRoundingLabels.Value);
        }

        private void bt_export_families_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string folder_path = folderBrowser.SelectedPath;
                using (var writer = new StreamWriter(folder_path + "\\identified_proteoforms.tsv"))
                {
                    int fam_id = 1;
                    writer.WriteLine("family_id\tproteoform_type\tp_id\tptm\tmass\tname");
                    foreach (ProteoformFamily family in Lollipop.proteoform_community.families.Where(f => f.theoretical_count > 0 || f.topdown_count > 0))
                    {
                        foreach (TheoreticalProteoform t in family.theoretical_proteoforms)
                        {
                            string id = t.accession;
                            if (Lollipop.use_gene_ID) id = t.gene_id.ToString();
                            writer.WriteLine(String.Join("\t", fam_id, "Theoretical", id, t.ptm_descriptions, t.modified_mass, t.name));
                        }
                        foreach (ExperimentalProteoform exp in family.experimental_proteoforms)
                        {
                            writer.WriteLine(String.Join("\t", fam_id, "Experimental", exp.accession, "", exp.modified_mass, ""));
                        }
                        foreach (TopDownProteoform td in family.topdown_proteoforms)
                        {
                            writer.WriteLine(String.Join("\t", fam_id, "Top-down", td.accession, td.ptm_descriptions, td.modified_mass, td.name));
                        }
                        fam_id++;
                    }
                }
                MessageBox.Show("Successfully exported list of proteoforms in identified families.");
            }
            else return;
            }

        private void bt_export_inclusion_list_Click(object sender, EventArgs e)
        {
            //maybe make selected families button?? 
            //exports an inclusion list of any experimental proteoforms not in an e-td pair 
            //to do... check raw file and not put anything on inclusion list that was already fragmented

            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string folder_path = folderBrowser.SelectedPath;

                List<ExperimentalProteoform> experimentals = new List<ExperimentalProteoform>();
                if (cb_inclusion_list_family.Checked)
                {
                    if (cb_inclusion_list_identified_family.Checked)
                        //family not null, ET relation in family, e proteoform not in topdown relation
                        experimentals.AddRange(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.family != null && p.family.relations.Where(r => r.relation_type == ProteoformComparison.et || r.relation_type == ProteoformComparison.etd).ToList().Count > 0 && p.relationships.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count == 0).ToList());

                    if (cb_inclusion_list_unidentified_fam.Checked)
                    {
                        experimentals.AddRange(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.family != null && p.family.relations.Where(r => r.relation_type == ProteoformComparison.et || r.relation_type == ProteoformComparison.etd).ToList().Count == 0).ToList());
                    }

                    //FOR TEST WILL DELETE THIS
                    if (cb_inclusion_list_1T.Checked)
                    {
                        experimentals.AddRange(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.family != null && p.family.relations.Where(r => r.relation_type == ProteoformComparison.et).ToList().Count == 1 && p.family.relations.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count == 0).ToList());
                    }
                }

                else
                {
                    experimentals.AddRange(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.family == null));
                }
                foreach (ExperimentalProteoform exp in experimentals)
                {
                    using (var writer = new StreamWriter(folder_path + "\\inclusion_list.txt"))
                    {
                        //max intensity charge state of the max intensity component
                        //ChargeState best_charge_state = (experimentals[experimental_num].aggregated_components.OrderBy(c => c.intensity_sum).First().charge_states.OrderBy(c => c.intensity).First());
                        if (!cb_inclusion_list_charge_state.Checked)
                        {
                            foreach (ChargeState cs in exp.aggregated_components.OrderBy(c => c.intensity_sum).ToList().First().charge_states)
                            {
                                if (cs.charge_count <= 25)
                                {
                                    writer.WriteLine(cs.mz_centroid + "\t" + cs.charge_count);
                                }
                            }
                        }
                        else
                        {
                            ChargeState cs = exp.aggregated_components.OrderBy(c => c.intensity_sum).ToList().First().charge_states.Where(c => c.charge_count <= 25).ToList().OrderBy(c => c.intensity).ToList().First();
                            if (cs.charge_count <= 25)
                            {
                                writer.WriteLine(cs.mz_centroid + "\t" + "\t" + cs.charge_count);
                            }
                        }
                    }
                }
            }
            else return;
                MessageBox.Show("Successfully exported inclusion list(s).");
            }
        

        private void cb_inclusion_list_family_CheckedChanged(object sender, EventArgs e)
        {
            if (!cb_inclusion_list_family.Checked)
            {
                cb_inclusion_list_identified_family.Checked = false;
                cb_inclusion_list_unidentified_fam.Checked = false;
            }
            if (cb_inclusion_list_family.Checked)
            {
                cb_inclusion_list_identified_family.Checked = true;
                cb_inclusion_list_unidentified_fam.Checked = true;
            }
        }

        private void cb_inclusion_list_identified_family_CheckedChanged(object sender, EventArgs e)
        {
            if (!cb_inclusion_list_identified_family.Checked)
            {
                if (!cb_inclusion_list_unidentified_fam.Checked && cb_inclusion_list_family.Checked && !cb_inclusion_list_1T.Checked)
                {
                    cb_inclusion_list_family.Checked = false;
                }
            }
        }

        private void cb_inclusion_list_unidentified_fam_CheckedChanged(object sender, EventArgs e)
        {
            if (!cb_inclusion_list_unidentified_fam.Checked)
            {
                if (!cb_inclusion_list_identified_family.Checked && cb_inclusion_list_family.Checked && !cb_inclusion_list_1T.Checked)
                {
                    cb_inclusion_list_family.Checked = false;
                }
            }
        }
        private void btn_merge_Click(object sender, EventArgs e)
        {
            
        }
    }
}
