using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProteoformSuiteInternal;

namespace ProteoformSuiteGUI
{
    public partial class IdentifiedProteoforms : Form , ISweetForm
    {
        public IdentifiedProteoforms()
        {
            InitializeComponent();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            FillTablesAndCharts();
        }


        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_bottom_up_peptides.DataSource = null;
            dgv_bottom_up_peptides.Rows.Clear();
            dgv_identified_experimentals.DataSource = null;
            dgv_identified_experimentals.Rows.Clear();
            dgv_td_proteoforms.DataSource = null;
            dgv_td_proteoforms.Rows.Clear();
        }

        public void InitializeParameterSet()
        {
            //not applicable
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_identified_experimentals, Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatIdentifiedProteoformTable(dgv_identified_experimentals);
            DisplayUtility.FillDataGridView(dgv_td_proteoforms, Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms.Where(e => e.topdown_id && e.linked_proteoform_references != null)).Select(e => new DisplayTopDownProteoform(e as TopDownProteoform)));
            DisplayTopDownProteoform.FormatIdentifiedProteoformTable(dgv_td_proteoforms);
            tb_bottom_up.Text = "Bottom-Up Peptides";
            tb_not_td.Text = "Identified Experimental Proteoforms Not in Top-Down";
            tb_topdown.Text = "Top-Down Proteoforms";
        }


        private void dgv_identified_proteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_bu_peptides(e.RowIndex);
        }

        private void dgv_topdown_proteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_bu_peptides(e.RowIndex);
        }


        private void display_bu_peptides(int row_index)
        {
            ExperimentalProteoform selected_experimental = (ExperimentalProteoform)((DisplayObject)this.dgv_identified_experimentals.Rows[row_index].DataBoundItem).display_object;
            DisplayUtility.FillDataGridView(dgv_bottom_up_peptides, selected_experimental.family.theoretical_proteoforms.Where(t => t.gene_name.get_prefered_name(Lollipop.preferred_gene_label) == selected_experimental.gene_name.get_prefered_name(Lollipop.preferred_gene_label)).SelectMany(t => t.psm_list).Select(t => new DisplayBottomUpPSM(t)));
            DisplayBottomUpPSM.FormatTopDownProteoformTable(dgv_bottom_up_peptides);
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView> { dgv_identified_experimentals };
        }
    }
}
