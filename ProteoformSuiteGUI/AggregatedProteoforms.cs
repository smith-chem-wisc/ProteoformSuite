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

        private void AggregatedProteoforms_Load(object sender, EventArgs e)
        { }

        private void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearListsAndTables();
            Lollipop.aggregate_proteoforms(Lollipop.validate_proteoforms, Lollipop.raw_neucode_pairs, Lollipop.raw_experimental_components, Lollipop.raw_quantification_components, Lollipop.min_num_CS);
            FillAggregatesTable();
            if (Lollipop.neucode_labeled && Lollipop.proteoform_community.theoretical_proteoforms.Length > 0)
            {
                ((ProteoformSweet)MdiParent).experimentalTheoreticalComparison.run_the_gamut();
                ((ProteoformSweet)MdiParent).experimentExperimentComparison.run_the_gamut();
                ((ProteoformSweet)MdiParent).quantification.perform_calculations();
            }

            updateFiguresOfMerit();
            this.Cursor = Cursors.Default;
        }

        private bool ready_to_aggregate()
        {
            return Lollipop.proteoform_community.experimental_proteoforms.Length <= 0 && (Lollipop.neucode_labeled && Lollipop.raw_neucode_pairs.Count > 0 || Lollipop.raw_experimental_components.Count > 0);
        }

        private void InitializeSettings()
        {
            //Min and Max set from designer
            nUP_mass_tolerance.Value = Lollipop.mass_tolerance;
            nUD_RetTimeToleranace.Value = Lollipop.retention_time_tolerance;
            nUD_Missed_Monos.Value = Lollipop.missed_monos;
            nUD_Missed_Ks.Value = Lollipop.missed_lysines;
            nUD_min_num_bioreps.Value = Lollipop.min_num_bioreps;
            nUD_min_agg_count.Value = Lollipop.min_agg_count;
            nUD_min_num_CS.Value = Lollipop.min_num_CS;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
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
            tb_totalAggregatedProteoforms.Text = Lollipop.proteoform_community.experimental_proteoforms.Count(p => p.accepted).ToString();
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.mass_tolerance = nUP_mass_tolerance.Value;
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.retention_time_tolerance = nUD_RetTimeToleranace.Value;
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_monos = nUD_Missed_Monos.Value;
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_lysines = nUD_Missed_Ks.Value;
        }
        private void bt_aggregate_Click(object sender, EventArgs e)
        {
            if (Lollipop.neucode_labeled && Lollipop.raw_neucode_pairs.Count > 0 || Lollipop.raw_experimental_components.Count > 0)
            {
                run_the_gamut();
            }
            else if (Lollipop.proteoform_community.experimental_proteoforms.Length <= 0)
            {
                MessageBox.Show("Go back and load in deconvolution results.");
            }
        }

        private void nUD_min_agg_count_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_agg_count = Convert.ToInt16(nUD_min_agg_count.Value);
        }

        private void nUD_min_num_CS_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_num_CS = Convert.ToInt16(nUD_min_num_CS.Value);
        }

        private void cb_validateProteoforms_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.validate_proteoforms = cb_validateProteoforms.Checked;
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_aggregates = tb_tableFilter.Text == "" ?
                Lollipop.proteoform_community.experimental_proteoforms :
                ExtensionMethods.filter(Lollipop.proteoform_community.experimental_proteoforms, tb_tableFilter.Text);

            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, selected_aggregates.OfType<ExperimentalProteoform>().Select(ep => new DisplayExperimentalProteoform(ep)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        private void nUD_min_num_bioreps_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_num_bioreps = Convert.ToInt16(nUD_min_num_bioreps.Value);
        }

        #endregion

        #region Public Methods

        public void aggregate_proteoforms()
        {
            if (ready_to_aggregate())
            {
                run_the_gamut();
            }
            else if (Lollipop.proteoform_community.experimental_proteoforms.Length <= 0) MessageBox.Show("Go back and load in deconvolution results.");
        }

        public DataGridView GetDGV()
        {
            return dgv_AggregatedProteoforms;
        }

        public void FillAggregatesTable()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, Lollipop.proteoform_community.experimental_proteoforms.Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }

        public void ClearListsAndTables()
        {
            Lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            Lollipop.vetted_proteoforms.Clear();
            Lollipop.ordered_components = new Component[0];
            Lollipop.remaining_components.Clear();
            Lollipop.remaining_verification_components.Clear();
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
