using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class AggregatedProteoforms : Form, ISweetForm
    {

        #region Private Field

        ExperimentalProteoform selected_pf = null;

        #endregion Private Field

        #region Public Constructor

        public AggregatedProteoforms()
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
            InitializeParameterSet();
        }

        #endregion Public Constructor

        #region Private Methods

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                selected_pf = (ExperimentalProteoform)((DisplayExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[e.RowIndex].DataBoundItem).display_object;
            else
                selected_pf = null;
            display_light_proteoforms();
        }

        private void dgv_AggregatedProteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
                selected_pf = (ExperimentalProteoform)((DisplayExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[e.RowIndex].DataBoundItem).display_object;
            else
                selected_pf = null;
            display_light_proteoforms();
        }

        private void display_light_proteoforms()
        {
            List<Component> components = selected_pf == null ? new List<Component>() :
                rb_displayIdentificationComponents.Checked ?
                selected_pf.aggregated_components :
                rb_displayLightQuantificationComponents.Checked ?
                    selected_pf.lt_quant_components :
                    selected_pf.hv_quant_components;
            DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, 
                Sweet.lollipop.neucode_labeled && rb_displayIdentificationComponents.Checked ? 
                    components.Select(c => new DisplayNeuCodePair(c as NeuCodePair)) : 
                    components.Select(c => new DisplayComponent(c)));
            if (Sweet.lollipop.neucode_labeled && rb_displayIdentificationComponents.Checked)
                DisplayNeuCodePair.FormatNeuCodeTable(dgv_AcceptNeuCdLtProteoforms);
            else
                DisplayComponent.FormatComponentsTable(dgv_AcceptNeuCdLtProteoforms, !rb_displayIdentificationComponents.Checked);
        }

        private void rb_displayIdentificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayIdentificationComponents.Checked)
                display_light_proteoforms();
        }

        private void rb_displayLightQuantificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayLightQuantificationComponents.Checked)
                display_light_proteoforms();
        }

        private void rb_displayHeavyQuantificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayHeavyQuantificationComponents.Checked)
                display_light_proteoforms();
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(p => p.accepted).ToString();
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.mass_tolerance = Convert.ToDouble(nUP_mass_tolerance.Value);
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.retention_time_tolerance = Convert.ToDouble(nUD_RetTimeToleranace.Value);
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.maximum_missed_monos = Convert.ToInt32(nUD_Missed_Monos.Value);
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.maximum_missed_lysines = Convert.ToInt32(nUD_Missed_Ks.Value);
        }

        private void bt_aggregate_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut();
                Cursor = Cursors.Default;
            }
            else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
            {
                MessageBox.Show("Go back and load in deconvolution results.");
            }
        }

        private void nUD_min_agg_count_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_agg_count = Convert.ToInt16(nUD_min_agg_count.Value);
        }

        private void nUD_min_num_CS_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_num_CS = Convert.ToInt16(nUD_min_num_CS.Value);
        }

        private void cb_validateProteoforms_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.validate_proteoforms = cb_validateProteoforms.Checked;
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_aggregates = tb_tableFilter.Text == "" ?
                Sweet.lollipop.target_proteoform_community.experimental_proteoforms :
                ExtensionMethods.filter(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, tb_tableFilter.Text);

            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, selected_aggregates.OfType<ExperimentalProteoform>().Select(ep => new DisplayExperimentalProteoform(ep)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }
        private void nUD_min_num_bioreps_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_num_bioreps = Convert.ToInt16(nUD_min_num_bioreps.Value);
        }

        #endregion Private Methods

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.neucode_labeled && Sweet.lollipop.raw_neucode_pairs.Count > 0 || Sweet.lollipop.raw_experimental_components.Count > 0;
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, Sweet.lollipop.min_num_CS);
            Sweet.lollipop.assign_best_components_for_manual_validation(Sweet.lollipop.target_proteoform_community.experimental_proteoforms);
            FillTablesAndCharts();
            updateFiguresOfMerit();
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_AggregatedProteoforms };
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        public void InitializeParameterSet()
        {
            //Min and Max set from designer
            nUP_mass_tolerance.Value = Convert.ToDecimal(Sweet.lollipop.mass_tolerance);
            nUD_RetTimeToleranace.Value = Convert.ToDecimal(Sweet.lollipop.retention_time_tolerance);
            nUD_Missed_Monos.Value = Sweet.lollipop.maximum_missed_monos;
            nUD_Missed_Ks.Value = Sweet.lollipop.maximum_missed_lysines;
            nUD_min_num_bioreps.Value = Sweet.lollipop.min_num_bioreps;
            nUD_min_agg_count.Value = Sweet.lollipop.min_agg_count;
            nUD_min_num_CS.Value = Sweet.lollipop.min_num_CS;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    if (sweet as TheoreticalDatabase == null)
                        sweet.ClearListsTablesFigures(false);
                }
            }

            Sweet.lollipop.clear_aggregation();
            dgv_AcceptNeuCdLtProteoforms.DataSource = null;
            dgv_AcceptNeuCdLtProteoforms.Rows.Clear();
            dgv_AggregatedProteoforms.DataSource = null;
            dgv_AggregatedProteoforms.Rows.Clear();
            tb_tableFilter.Clear();
            tb_totalAggregatedProteoforms.Clear();
        }

        #endregion Public Methods

    }
}
