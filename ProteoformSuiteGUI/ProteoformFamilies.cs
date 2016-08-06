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

namespace ProteoformSuite
{
    public partial class ProteoformFamilies : Form
    {
        public ProteoformFamilies()
        {
            InitializeComponent();
        }

        private void ProteoformFamilies_Load(object sender, EventArgs e)
        {
            if (Lollipop.proteoform_community.families.Count == 0) Lollipop.proteoform_community.construct_families();
            fill_proteoform_families();
        }

        public void fill_proteoform_families()
        {
            DisplayUtility.FillDataGridView(dgv_proteoform_families, Lollipop.proteoform_community.families);
            format_families_dgv();
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
    }
}
