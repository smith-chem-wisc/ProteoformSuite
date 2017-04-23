using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class AggregatedProteoforms : Form
    {

        #region Public Constructor

        public AggregatedProteoforms()
        {
            InitializeComponent();
            InitializeSettings();
        }

        #endregion

        #region Private Methods

        private bool ready_to_aggregate()
        {
            return SaveState.lollipop.proteoform_community.experimental_proteoforms.Length <= 0 && (SaveState.lollipop.neucode_labeled && SaveState.lollipop.raw_neucode_pairs.Count > 0 || SaveState.lollipop.raw_experimental_components.Count > 0);
        }



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
            tb_totalAggregatedProteoforms.Text = SaveState.lollipop.proteoform_community.experimental_proteoforms.Count(p => p.accepted).ToString();
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
            if (SaveState.lollipop.neucode_labeled && SaveState.lollipop.raw_neucode_pairs.Count > 0 || SaveState.lollipop.raw_experimental_components.Count > 0)
            {
                RunTheGamut();
            }
            else if (SaveState.lollipop.proteoform_community.experimental_proteoforms.Length <= 0)
            {
                MessageBox.Show("Go back and load in deconvolution results.");
            }
        }

        private void nUD_min_agg_count_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_agg_count = Convert.ToInt16(nUD_min_agg_count.Value);
        }

        private void nUD_min_num_CS_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_num_CS = Convert.ToInt16(nUD_min_num_CS.Value);
        }

        private void cb_validateProteoforms_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.validate_proteoforms = cb_validateProteoforms.Checked;
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_aggregates = tb_tableFilter.Text == "" ?
                SaveState.lollipop.proteoform_community.experimental_proteoforms :
                ExtensionMethods.filter(SaveState.lollipop.proteoform_community.experimental_proteoforms, tb_tableFilter.Text);

            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, selected_aggregates.OfType<ExperimentalProteoform>().Select(ep => new DisplayExperimentalProteoform(ep)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        private void nUD_min_num_bioreps_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_num_bioreps = Convert.ToInt16(nUD_min_num_bioreps.Value);
        }

        #endregion

        #region Public Methods

        public void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearListsAndTables();
            SaveState.lollipop.aggregate_proteoforms(SaveState.lollipop.validate_proteoforms, SaveState.lollipop.raw_neucode_pairs, SaveState.lollipop.raw_experimental_components, SaveState.lollipop.raw_quantification_components, SaveState.lollipop.min_num_CS);
            FillAggregatesTable();
            if (SaveState.lollipop.neucode_labeled && SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length > 0)
            {
                ((ProteoformSweet)MdiParent).experimentalTheoreticalComparison.run_the_gamut();
                ((ProteoformSweet)MdiParent).experimentExperimentComparison.run_the_gamut();
                ((ProteoformSweet)MdiParent).quantification.perform_calculations();
            }

            updateFiguresOfMerit();
            this.Cursor = Cursors.Default;
        }

        public void aggregate_proteoforms()
        {
            if (ready_to_aggregate())
            {
                RunTheGamut();
            }
            else if (SaveState.lollipop.proteoform_community.experimental_proteoforms.Length <= 0) MessageBox.Show("Go back and load in deconvolution results.");
        }

        public DataGridView GetDGV()
        {
            return dgv_AggregatedProteoforms;
        }

        public void FillAggregatesTable()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, SaveState.lollipop.proteoform_community.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        public void InitializeSettings()
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

        public void ClearListsAndTables()
        {
            SaveState.lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            SaveState.lollipop.vetted_proteoforms.Clear();
            SaveState.lollipop.ordered_components = new Component[0];
            SaveState.lollipop.remaining_components.Clear();
            SaveState.lollipop.remaining_verification_components.Clear();
            dgv_AcceptNeuCdLtProteoforms.DataSource = null;
            dgv_AcceptNeuCdLtProteoforms.Rows.Clear();

            ((ProteoformSweet)MdiParent).experimentalTheoreticalComparison.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).quantification.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).experimentExperimentComparison.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsAndTables();
        }

        #endregion Public Methods
        
    }
}
