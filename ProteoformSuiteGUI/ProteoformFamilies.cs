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

        public void initialize_settings()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
        }

        public void construct_families()
        {
            if (Lollipop.proteoform_community.families.Count == 0) run_the_gamut();
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
            DisplayUtility.FillDataGridView(dgv_proteoform_families, Lollipop.proteoform_community.families);
            format_families_dgv();
        }

        private void update_figures_of_merit()
        {
            this.tb_TotalFamilies.Text = Lollipop.proteoform_community.families.Count().ToString();
            this.tb_IdentifiedFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.theoretical_count > 0).ToString();
        }

        private void format_families_dgv()
        {
            //set column header
            //dgv_proteoform_families.Columns["family_id"].HeaderText = "Light Monoisotopic Mass";
            dgv_proteoform_families.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_proteoform_families.Columns["experimental_count"].HeaderText = "Experimental Proteoforms";
            dgv_proteoform_families.Columns["theoretical_count"].HeaderText = "Theoretical Proteoforms";
            dgv_proteoform_families.Columns["relation_count"].HeaderText = "Relation Count";
            dgv_proteoform_families.Columns["accession_list"].HeaderText = "Accessions";
            dgv_proteoform_families.Columns["relations"].Visible = false;
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
            if (dgv_proteoform_families.Columns[column_index].Name == "theoretical_count")
            {
                if (selected_family.theoretical_count > 0) 
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms);
                    DisplayUtility.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (dgv_proteoform_families.Columns[column_index].Name == "experimental_count")
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
            string time_stamp = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
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
    }
}
