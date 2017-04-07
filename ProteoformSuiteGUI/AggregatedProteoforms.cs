﻿using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ProteoformSuiteGUI
{
    public partial class AggregatedProteoforms : Form
    {
        public AggregatedProteoforms()
        {
            InitializeComponent();
            InitializeSettings();
        }

        public void AggregatedProteoforms_Load(object sender, EventArgs e)
        { }

        private bool ready_to_aggregate()
        {
            return Lollipop.proteoform_community.experimental_proteoforms.Length <= 0 && (Lollipop.neucode_labeled && Lollipop.raw_neucode_pairs.Count > 0) || Lollipop.raw_experimental_components.Count > 0;
        }

        public void aggregate_proteoforms()
        {
            if (ready_to_aggregate())
            {
                run_the_gamut();
            }
            else MessageBox.Show("Go back and load in deconvolution results.");
        }

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

        public DataGridView GetDGV()
        {
            return dgv_AggregatedProteoforms;
        }

        private void InitializeSettings()
        {
            nUP_mass_tolerance.Minimum = 0;
            nUP_mass_tolerance.Maximum = 10;
            nUP_mass_tolerance.Value = Lollipop.mass_tolerance;

            nUD_RetTimeToleranace.Minimum = 0;
            nUD_RetTimeToleranace.Maximum = 200;
            nUD_RetTimeToleranace.Value = Lollipop.retention_time_tolerance;

            nUD_Missed_Monos.Minimum = 0;
            nUD_Missed_Monos.Maximum = 5;
            nUD_Missed_Monos.Value = Lollipop.missed_monos;

            nUD_Missed_Ks.Minimum = 0;
            nUD_Missed_Ks.Maximum = 3;
            nUD_Missed_Ks.Value = Lollipop.missed_lysines;

            nUD_min_num_bioreps.Minimum = 1;
            nUD_min_num_bioreps.Maximum = 100;
            nUD_min_num_bioreps.Value = Lollipop.min_num_bioreps;

            nUD_min_agg_count.Minimum = 1;
            nUD_min_agg_count.Maximum = 100;
            nUD_min_agg_count.Value = Lollipop.min_agg_count;

            nUD_min_num_CS.Minimum = 0;
            nUD_min_num_CS.Maximum = 20;
            nUD_min_num_CS.Value = Lollipop.min_num_CS;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
        }

        public void FillAggregatesTable()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, Lollipop.proteoform_community.experimental_proteoforms);
            DisplayUtility.FormatAggregatesTable(dgv_AggregatedProteoforms);
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
            ExperimentalProteoform selected_pf = (ExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[row_index].DataBoundItem;
            DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, selected_pf.aggregated_components);
            Format_dgv_AcceptNeuCdLtProteoforms();
        }

        private void Format_dgv_AcceptNeuCdLtProteoforms()
        {
            ////round table values
            //dgv_AcceptNeuCdLtProteoforms.Columns["monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["weighted_monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["corrected_mass"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["rt_apex"].DefaultCellStyle.Format = "0.##";
            //dgv_AcceptNeuCdLtProteoforms.Columns["relative_abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["fract_abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].DefaultCellStyle.Format = "0.####";
            //if (Lollipop.neucode_labeled) { dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].DefaultCellStyle.Format = "0.####"; }


            ////set column header
            //dgv_AcceptNeuCdLtProteoforms.Columns["monoisotopic_mass"].HeaderText = "Monoisotopic Mass";
            //dgv_AcceptNeuCdLtProteoforms.Columns["delta_mass"].HeaderText = "Delta Mass";
            //dgv_AcceptNeuCdLtProteoforms.Columns["weighted_monoisotopic_mass"].HeaderText = "Weighted Monoisotopic Mass";
            //dgv_AcceptNeuCdLtProteoforms.Columns["corrected_mass"].HeaderText = "Corrected Mass";
            //dgv_AcceptNeuCdLtProteoforms.Columns["rt_apex"].HeaderText = "Apex RT";
            //dgv_AcceptNeuCdLtProteoforms.Columns["relative_abundance"].HeaderText = "Relative Abundance";
            //dgv_AcceptNeuCdLtProteoforms.Columns["fract_abundance"].HeaderText = "Fractional Abundance";
            //dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].HeaderText = "Intensity Sum of Overlapping Charge States (of all if unlabeled)";
            //dgv_AcceptNeuCdLtProteoforms.Columns["input_file"].HeaderText = "Filename";
            //dgv_AcceptNeuCdLtProteoforms.Columns["scan_range"].HeaderText = "Scan Range";
            //dgv_AcceptNeuCdLtProteoforms.Columns["rt_range"].HeaderText = "RT Range";
            //dgv_AcceptNeuCdLtProteoforms.Columns["num_charge_states"].HeaderText = "No. Charge States";
            //dgv_AcceptNeuCdLtProteoforms.Columns["accepted"].HeaderText = "Accepted";
            //dgv_AcceptNeuCdLtProteoforms.Columns["manual_mass_shift"].HeaderText = "Manual Mass Shift (Da)";


            //dgv_AcceptNeuCdLtProteoforms.AllowUserToAddRows = false;
            //dgv_AcceptNeuCdLtProteoforms.Columns["_manual_mass_shift"].Visible = false;
            //dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum"].Visible = false;
            //dgv_AcceptNeuCdLtProteoforms.Columns["num_charge_states_fromFile"].Visible = false;


            //if (Lollipop.neucode_labeled)
            //{
            //    dgv_AcceptNeuCdLtProteoforms.Columns["lysine_count"].HeaderText = "Lysine Count";
            //    dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].HeaderText = "Intensity Ratio";
            //    dgv_AcceptNeuCdLtProteoforms.Columns["id_light"].HeaderText = "ID Light";
            //    dgv_AcceptNeuCdLtProteoforms.Columns["id_heavy"].HeaderText = "ID Heavy";
            //    dgv_AcceptNeuCdLtProteoforms.Columns["neuCodeHeavy"].Visible = false;
            //    dgv_AcceptNeuCdLtProteoforms.Columns["neuCodeLight"].Visible = false;
            //}
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = Lollipop.proteoform_community.experimental_proteoforms.Count(p => p.accepted).ToString();
        }

        public void ClearListsAndTables()
        {
            Lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            Lollipop.vetted_proteoforms.Clear();
            Lollipop.ordered_components = new ProteoformSuiteInternal.Component[0];
            Lollipop.remaining_components.Clear();
            Lollipop.remaining_verification_components.Clear();
            dgv_AcceptNeuCdLtProteoforms.DataSource = null;
            dgv_AcceptNeuCdLtProteoforms.Rows.Clear();

            ((ProteoformSweet)MdiParent).experimentalTheoreticalComparison.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).quantification.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).experimentExperimentComparison.ClearListsAndTables();
            ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsAndTables();
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
            else if (Lollipop.proteoform_community.experimental_proteoforms.Length <= 0) MessageBox.Show("Go back and load in deconvolution results.");

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
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, selected_aggregates);
            if (selected_aggregates.Count() > 0) DisplayUtility.FormatAggregatesTable(dgv_AggregatedProteoforms);
        }
        private void nUD_min_num_bioreps_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_num_bioreps = Convert.ToInt16(nUD_min_num_bioreps.Value);
        }
    }
}
