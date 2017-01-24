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
        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;

        public ProteoformFamilies()
        {
            InitializeComponent();
        }

        private void ProteoformFamilies_Load(object sender, EventArgs e)
        { }

        private void initialize_settings()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
            this.nud_decimalRoundingLabels.Value = Convert.ToDecimal(Lollipop.deltaM_edge_display_rounding);
        }

        public void construct_families()
        {
            initialize_settings();
            if (Lollipop.proteoform_community.families.Count <= 0 && (Lollipop.proteoform_community.has_e_proteoforms || Lollipop.proteoform_community.topdown_proteoforms.Count > 0)) run_the_gamut();
        }

        public DataGridView GetDGV()
        {
            return dgv_proteoform_families;
        }

        private void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            Lollipop.proteoform_community.construct_families();
            fill_proteoform_families();
            update_figures_of_merit();
            this.Cursor = Cursors.Default;
        }

        private void fill_proteoform_families()
        {
            DisplayUtility.FillDataGridView(dgv_proteoform_families, Lollipop.proteoform_community.families.OrderByDescending(f => f.relation_count).ToList());
            format_families_dgv();
        }

        private void update_figures_of_merit()
        {
            this.tb_TotalFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString();
            this.tb_IdentifiedFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.theoretical_count > 0).ToString();
            this.tb_singleton_count.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString();
        }

        private void format_families_dgv()
        {
            //set column header
            //dgv_proteoform_families.Columns["family_id"].HeaderText = "Light Monoisotopic Mass";
            dgv_proteoform_families.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_proteoform_families.Columns["experimental_count"].HeaderText = "Experimental Proteoforms";
            dgv_proteoform_families.Columns["theoretical_count"].HeaderText = "Theoretical Proteoforms";
            dgv_proteoform_families.Columns["relation_count"].HeaderText = "Relation Count";
            dgv_proteoform_families.Columns["accession_list"].HeaderText = "Theoretical Accessions";
            dgv_proteoform_families.Columns["name_list"].HeaderText = "Theoretical Names";
            dgv_proteoform_families.Columns["experimentals_list"].HeaderText = "Experimental Accessions";
            dgv_proteoform_families.Columns["agg_mass_list"].HeaderText = "Experimental Aggregated Masses";
            dgv_proteoform_families.Columns["relations"].Visible = false;
            dgv_proteoform_families.Columns["proteoforms"].Visible = false;
        }

        private void dgv_proteoform_families_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void dgv_proteoform_families_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void display_family_members(int row_index, int column_index)
        {
            ProteoformFamily selected_family = (ProteoformFamily)this.dgv_proteoform_families.Rows[row_index].DataBoundItem;
            if (new List<string> { "theoretical_count", "accession_list","name_list" }.Contains(dgv_proteoform_families.Columns[column_index].Name))
            {
                if (selected_family.theoretical_count > 0) 
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms);
                    DisplayUtility.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (new List<string> { "experimental_count", "experimentals_list", "agg_mass_list" }.Contains(dgv_proteoform_families.Columns[column_index].Name))
            {
                if (selected_family.experimental_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms);
                    DisplayUtility.FormatAggregatesTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (dgv_proteoform_families.Columns[column_index].Name == "relation_count")
            {
                if (selected_family.relation_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.relations);
                    DisplayUtility.FormatRelationsGridView(dgv_proteoform_family_members, false, false);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
        }

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
            if (got_cyto_temp_folder && dgv_proteoform_families.SelectedRows.Count > 0) btn_buildSelectedFamilies.Enabled = true;
        }

        private void btn_buildAllFamilies_Click(object sender, EventArgs e)
        {
            bool built = build_families(Lollipop.proteoform_community.families);
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            //Check if there are any rows selected
            int selected_row_sum = 0;
            for (int i = 0; i < dgv_proteoform_families.SelectedCells.Count; i++) selected_row_sum += dgv_proteoform_families.SelectedCells[i].RowIndex;

            List<ProteoformFamily> families = new List<ProteoformFamily>();
            if (dgv_proteoform_families.SelectedRows.Count > 0)
                for (int i = 0; i < dgv_proteoform_families.SelectedRows.Count; i++)
                    families.Add((ProteoformFamily)dgv_proteoform_families.SelectedRows[i].DataBoundItem);
            else
                for (int i = 0; i < dgv_proteoform_families.SelectedCells.Count; i++)
                    if (dgv_proteoform_families.SelectedCells[i].RowIndex != 0)
                        families.Add((ProteoformFamily)dgv_proteoform_families.Rows[dgv_proteoform_families.SelectedCells[i].RowIndex].DataBoundItem);

            bool built = build_families(families);
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
            CytoscapeScript c = new CytoscapeScript(families, time_stamp);
            File.WriteAllText(c.edges_path, c.edge_table);
            File.WriteAllText(c.nodes_path, c.node_table);
            File.WriteAllText(c.script_path, c.script);
            c.write_styles();
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
                int inclusion_list_num = 1;
                //int list_count = 0;
                int experimental_num = 0;
                //experimentals in relations without topdown relations
                // ExperimentalProteoform[] experimentals = Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.relationships.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count == 0).ToList().Where(p => p.relationships.Count > 0).ToList().OrderBy(p => p.agg_intensity).ToArray();

                //experimentals without topdown relations
                ExperimentalProteoform[] experimentals = Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.relationships.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count == 0).ToList().OrderBy(p => p.agg_intensity).ToArray();

                while (experimental_num < experimentals.Length)
                {
                    using (var writer = new StreamWriter(folder_path + "\\inclusion_list_" + inclusion_list_num + ".txt"))
                    {
                        //while count is less than maximum export inclusion list with highest intensity items 
                        //while (list_count < 100)
                        while(experimental_num < experimentals.Length)
                        {
                            //max intensity charge state of the max intensity component
                            ChargeState best_charge_state = (experimentals[experimental_num].aggregated_components.OrderBy(c => c.intensity_sum).First().charge_states.OrderBy(c => c.intensity).First());
                            writer.WriteLine(best_charge_state.mz_centroid + "\t" + "\t" + best_charge_state.charge_count);
                            experimental_num++;
                        }
                        //list_count = 0;
                        inclusion_list_num++;
                    }
                }
                MessageBox.Show("Successfully exported inclusion list(s).");
            }
            else return;
        }
    }
}
