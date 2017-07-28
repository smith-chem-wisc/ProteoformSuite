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
            return Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null) > 0;
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            FillTablesAndCharts();
        }


        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_other_topdown_ids.DataSource = null;
            dgv_other_topdown_ids.Rows.Clear();
            dgv_identified_experimentals.DataSource = null;
            dgv_identified_experimentals.Rows.Clear();
            dgv_same_topdown_id.DataSource = null;
            dgv_same_topdown_id.Rows.Clear();
            dgv_other_topdown_ids.DataSource = null;
            dgv_other_topdown_ids.Rows.Clear();
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
        }


        private void identified_experimentals_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ( e.RowIndex >= 0) display_td_bu_proteoforms(e.RowIndex);
        }

        private void dgv_identified_proteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_td_bu_proteoforms(e.RowIndex);
        }


        private void display_td_bu_proteoforms(int row_index)
        {
            ExperimentalProteoform selected_experimental = (ExperimentalProteoform)((DisplayObject)this.dgv_identified_experimentals.Rows[row_index].DataBoundItem).display_object;
            DisplayUtility.FillDataGridView(dgv_same_topdown_id, selected_experimental.relationships.Where(r => r.RelationType == ProteoformComparison.TopdownExperimental).Select(r => r.connected_proteoforms[0]).Select(t => new DisplayTopDownProteoform(t as TopDownProteoform)));
            DisplayUtility.FillDataGridView(dgv_other_topdown_ids, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Where(t =>  t.gene_name == selected_experimental.gene_name && !t.relationships.SelectMany(r => r.connected_proteoforms).Contains(selected_experimental) &&
                Math.Abs(t.modified_mass - selected_experimental.modified_mass) < (double)Sweet.lollipop.mass_tolerance).Select(t => new DisplayTopDownProteoform(t)));
            DisplayUtility.FillDataGridView(dgv_bottom_up_peptides, selected_experimental.family.theoretical_proteoforms.Where(t => t.gene_name == selected_experimental.gene_name).SelectMany(t => t.psm_list).Select(t => new DisplayBottomUpPSM(t)));
            DisplayTopDownProteoform.FormatTopDownProteoformTable(dgv_other_topdown_ids);
            DisplayTopDownProteoform.FormatTopDownProteoformTable(dgv_same_topdown_id);
            DisplayBottomUpPSM.FormatTopDownProteoformTable(dgv_bottom_up_peptides);
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView> { dgv_identified_experimentals };
        }
    }
}
