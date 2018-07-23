using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class AggregatedProteoforms : Form, ISweetForm
    {
        #region Private Field

        private ExperimentalProteoform selected_pf;

        #endregion Private Field

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

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

        private void CellClick(int rowIndex)
        {
            if (rowIndex >= 0)
            {
                selected_pf = (ExperimentalProteoform)((DisplayExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[rowIndex].DataBoundItem).display_object;
            }
            else
            {
                selected_pf = null;
            }
            display_light_proteoforms();
        }

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CellClick(e.RowIndex);
        }

        private void dgv_AggregatedProteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CellClick(e.RowIndex);
        }

        private void display_light_proteoforms()
        {
            List<IAggregatable> components = selected_pf == null ? new List<IAggregatable>() :
                rb_displayIdentificationComponents.Checked ?
                selected_pf.aggregated :
                rb_displayLightQuantificationComponents.Checked ?
                    selected_pf.lt_quant_components.ToList<IAggregatable>() :
                    selected_pf.hv_quant_components.ToList<IAggregatable>();

            if (Sweet.lollipop.neucode_labeled && rb_displayIdentificationComponents.Checked)
            {
                DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, components.Select(c => new DisplayNeuCodePair(c as NeuCodePair)));
            }
            else if (rb_displayIdentificationComponents.Checked && selected_pf != null && selected_pf.topdown_id)
            {
                DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, (selected_pf as TopDownProteoform).topdown_hits.Select(h => new DisplayTopDownHit(h)));
            }
            else
            {
                DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, components.Select(c => new DisplayComponent(c as Component)));
            }

            if (Sweet.lollipop.neucode_labeled && rb_displayIdentificationComponents.Checked)
            {
                DisplayNeuCodePair.FormatNeuCodeTable(dgv_AcceptNeuCdLtProteoforms);
            }
            else if (rb_displayIdentificationComponents.Checked && selected_pf != null && selected_pf.topdown_id)
            {
                DisplayTopDownHit.FormatTopDownHitsTable(dgv_AcceptNeuCdLtProteoforms);
            }
            else
            {
                DisplayComponent.FormatComponentsTable(dgv_AcceptNeuCdLtProteoforms);
            }
        }

        private void rb_displayIdentificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayIdentificationComponents.Checked)
            {
                display_light_proteoforms();
            }
        }

        private void rb_displayLightQuantificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayLightQuantificationComponents.Checked)
            {
                display_light_proteoforms();
            }
        }

        private void rb_displayHeavyQuantificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_displayHeavyQuantificationComponents.Checked)
            {
                display_light_proteoforms();
            }
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
                RunTheGamut(false);
                Cursor = Cursors.Default;
            }
            else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
            {
                MessageBox.Show("Go back and load in deconvolution results.");
            }
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

        #endregion Private Methods

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.topdown_proteoforms.Count > 0 || (Sweet.lollipop.neucode_labeled && Sweet.lollipop.raw_neucode_pairs.Count > 0 || Sweet.lollipop.raw_experimental_components.Count > 0);
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, Sweet.lollipop.min_num_CS);
            Sweet.lollipop.assign_best_components_for_manual_validation(Sweet.lollipop.target_proteoform_community.experimental_proteoforms);
            FillTablesAndCharts();
            updateFiguresOfMerit();
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayExperimentalProteoform.FormatAggregatesTable(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)).ToList(), "AggregatedProteoforms")
            };
            return DataTables;
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
            nUD_min_num_CS.Value = Sweet.lollipop.min_num_CS;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;

            decimal agg_minBiorepsWithObservations = Sweet.lollipop.agg_minBiorepsWithObservations;
            cmbx_observationsTypeRequired.Items.Clear();
            cmbx_observationsTypeRequired.SelectedIndexChanged -= cmbx_observationsTypeRequired_SelectedIndexChanged;
            cmbx_observationsTypeRequired.Items.AddRange(Lollipop.observation_requirement_possibilities);
            cmbx_observationsTypeRequired.SelectedIndex = Sweet.lollipop.agg_observation_requirement == new Lollipop().agg_observation_requirement ? // check that the default has not been changed (haven't loaded presets)
                0 :
                Lollipop.observation_requirement_possibilities.ToList().IndexOf(Sweet.lollipop.agg_observation_requirement);
            cmbx_observationsTypeRequired.SelectedItem = Sweet.lollipop.agg_observation_requirement;
            cmbx_observationsTypeRequired.SelectedIndexChanged += cmbx_observationsTypeRequired_SelectedIndexChanged;

            nud_minObservations.Minimum = 1;
            set_nud_minObs_maximum();
            if (agg_minBiorepsWithObservations <= nud_minObservations.Maximum
                && agg_minBiorepsWithObservations >= nud_minObservations.Minimum) nud_minObservations.Value = agg_minBiorepsWithObservations;
            else nud_minObservations.Value = nud_minObservations.Maximum;
            Sweet.lollipop.agg_minBiorepsWithObservations = (int)nud_minObservations.Value;
            cb_add_td_proteoforms.Checked = Sweet.lollipop.add_td_proteoforms;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
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

        private void cmbx_observationsTypeRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.agg_observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            set_nud_minObs_maximum();
            nud_minObservations.Value = nud_minObservations. Maximum;
        }

        private void set_nud_minObs_maximum()
        {
            List<string> conditions = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(f => f.lt_condition).Distinct().ToList();

            if (Sweet.lollipop.neucode_labeled)
            {
                conditions.AddRange(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(f => f.hv_condition).Distinct());
            }

            if (Sweet.lollipop.agg_observation_requirement == Lollipop.observation_requirement_possibilities[1]) // From any  condition
            {
                nud_minObservations.Maximum = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(x => x.lt_condition + x.biological_replicate).Distinct().Count();
            }
            else if (Lollipop.observation_requirement_possibilities.ToList().IndexOf(Sweet.lollipop.agg_observation_requirement) < 3)
            {
                nud_minObservations.Maximum = Sweet.lollipop.input_files.Count(f => f.purpose == Purpose.Identification) == 0 ? 0 : conditions.Min(c => Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Where(x => x.lt_condition == c).Concat(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Where(x => x.hv_condition == c)).Select(x => x.biological_replicate).Distinct().Count());
            }
            else if (Sweet.lollipop.agg_observation_requirement == Lollipop.observation_requirement_possibilities[4]) // From any single condition
            {
                nud_minObservations.Maximum = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(x => x.lt_condition + x.biological_replicate + x.technical_replicate).Distinct().Count();
            }
            else
            {
                nud_minObservations.Maximum = Sweet.lollipop.input_files.Count(f => f.purpose == Purpose.Identification) == 0 ? 0 : conditions.Min(c => Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Where(x => x.lt_condition == c).Concat(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Where(x => x.hv_condition == c)).Select(x => x.biological_replicate + x.technical_replicate).Distinct().Count());
            }
        }

        private void nud_minObservations_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.agg_minBiorepsWithObservations = (int)nud_minObservations.Value;
        }

        private void cb_add_td_proteoforms_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.add_td_proteoforms = cb_add_td_proteoforms.Checked;
        }
    }
}