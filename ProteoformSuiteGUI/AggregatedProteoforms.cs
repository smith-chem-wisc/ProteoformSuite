using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class AggregatedProteoforms : Form, ISweetForm
    {

        #region Public Constructor

        public AggregatedProteoforms()
        {
            InitializeComponent();
            InitializeParameterSet();
        }

        #endregion

        #region Private Methods

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) display_light_proteoforms(e.RowIndex);
        }

        private void dgv_AggregatedProteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_light_proteoforms(e.RowIndex);
        }

        private void display_light_proteoforms(int row_index)
        {
            ExperimentalProteoform selected_pf = (ExperimentalProteoform)((DisplayExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[row_index].DataBoundItem).display_object;
            DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, selected_pf.aggregated_components.Select(c => new DisplayComponent(c)));
            DisplayComponent.FormatComponentsTable(dgv_AcceptNeuCdLtProteoforms, false);
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(p => p.accepted).ToString();
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.mass_tolerance = nUP_mass_tolerance.Value;
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.retention_time_tolerance = nUD_RetTimeToleranace.Value;
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.missed_monos = nUD_Missed_Monos.Value;
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.missed_lysines = nUD_Missed_Ks.Value;
        }

        private void bt_aggregate_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut();
                Cursor = Cursors.Default;
            }
            else if (SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
            {
                MessageBox.Show("Go back and load in deconvolution results.");
            }
        }

        private void nUD_min_agg_count_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_agg_count = Convert.ToInt16(nUD_min_agg_count.Value);
        }

        private void cb_merge_RT_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.merge_by_RT = cb_merge_RT.Checked;
        }

        private void nUD_min_num_CS_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_num_CS = Convert.ToInt16(nUD_min_num_CS.Value);
        }

        private void nUD_min_signal_to_noise_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_signal_to_noise = Convert.ToDouble(nUD_min_signal_to_noise.Value);
        }

        private void nUD_min_left_peaks_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_left_peaks = Convert.ToInt16(nUD_min_left_peaks.Value);
        }

        private void nUD_min_right_peaks_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_right_peaks = Convert.ToInt16(nUD_min_right_peaks.Value);
        }

        private void cb_validateProteoforms_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.validate_proteoforms = cb_validateProteoforms.Checked;
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_aggregates = tb_tableFilter.Text == "" ?
                SaveState.lollipop.target_proteoform_community.experimental_proteoforms :
                ExtensionMethods.filter(SaveState.lollipop.target_proteoform_community.experimental_proteoforms, tb_tableFilter.Text);

            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, selected_aggregates.OfType<ExperimentalProteoform>().Select(ep => new DisplayExperimentalProteoform(ep)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }
        private void nUD_min_num_bioreps_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_num_bioreps = Convert.ToInt16(nUD_min_num_bioreps.Value);
        }

        #endregion

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.neucode_labeled && SaveState.lollipop.raw_neucode_pairs.Count > 0 || SaveState.lollipop.raw_experimental_components.Count > 0;
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            SaveState.lollipop.aggregate_proteoforms(SaveState.lollipop.validate_proteoforms, SaveState.lollipop.raw_neucode_pairs, SaveState.lollipop.raw_experimental_components, SaveState.lollipop.raw_quantification_components, SaveState.lollipop.min_num_CS, SaveState.lollipop.min_signal_to_noise, SaveState.lollipop.min_left_peaks, SaveState.lollipop.min_right_peaks);
            SaveState.lollipop.assign_best_components_for_manual_validation(SaveState.lollipop.target_proteoform_community.experimental_proteoforms);
            FillTablesAndCharts();
            updateFiguresOfMerit();
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_AggregatedProteoforms };
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        public void InitializeParameterSet()
        {
            //Min and Max set from designer
            nUP_mass_tolerance.Value = SaveState.lollipop.mass_tolerance;
            nUD_RetTimeToleranace.Value = SaveState.lollipop.retention_time_tolerance;
            nUD_Missed_Monos.Value = SaveState.lollipop.missed_monos;
            nUD_Missed_Ks.Value = SaveState.lollipop.missed_lysines;
            nUD_min_num_bioreps.Value = SaveState.lollipop.min_num_bioreps;
            nUD_min_agg_count.Value = SaveState.lollipop.min_agg_count;
            nUD_min_num_CS.Value = SaveState.lollipop.min_num_CS;

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

            SaveState.lollipop.clear_aggregation();
            dgv_AcceptNeuCdLtProteoforms.DataSource = null;
            dgv_AcceptNeuCdLtProteoforms.Rows.Clear();
        }

        #endregion Public Methods


    }
}
